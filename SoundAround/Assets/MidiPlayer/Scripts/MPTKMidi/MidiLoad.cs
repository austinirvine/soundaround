using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NAudio.Midi;
using System;
using System.IO;
using System.Linq;

namespace MidiPlayerTK
{
    public class TrackMidiEvent
    {
        public int IndexTrack;
        public long AbsoluteQuantize;
        public MidiEvent Event;
    }

    public class MidiLoad
    {
        public MidiFile midifile;
        public List<TrackMidiEvent> MidiSorted;
        /// <summary>
        /// Duration of the midi. Updated when ChangeSpeed is called.
        /// </summary>
        public TimeSpan Duration;
        public bool EndMidiEvent;
        public double QuarterPerMinuteValue;
        public string SequenceTrackName = "";
        public string ProgramName = "";
        public string TrackInstrumentName = "";
        public string TextEvent = "";
        public string Copyright = "";
        public double PulseLengthMs;

        public bool EnableChangeTempo;
        public bool LogEvents;
        public bool KeepNoteOff;
        public bool EnablePanChange;

        //private double PulseInSecond;
        private long Quantization;
        private long CurrentPulse = 0;
        private double Speed = 1d;
        private double LastTimeFromStartMS;

        /// <summary>
        /// Last position played by tracks
        /// </summary>
        private int NextPosEvent;

        /// <summary>
        /// Last patch change by chanel
        /// </summary>
        private int[] PatchChanel;

        /// <summary>
        /// Volume change by chanel with Expression and MainVolume midi controler
        /// </summary>
        private int[] VolumeChanel;

        /// <summary>
        /// Volume change by chanel with Expression and MainVolume midi controler
        /// </summary>
        private int[] PanChanel;

        /// <summary>
        /// Whats chanel in this midi ?
        /// </summary>
        private bool[] Chanels;

        private int CountTracks;

        public int DeltaTicksPerQuarterNote;

        /// <summary>
        /// Build OS path to the midi file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        static public string BuildOSPath(string filename)
        {
            try
            {
                string pathMidiFolder = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                string pathfilename = Path.Combine(pathMidiFolder, filename + MidiPlayerGlobal.ExtensionMidiFile);
                return pathfilename;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        /// <summary>
        /// Load Midi from MPTK referential
        /// </summary>
        /// <param name="index"></param>
        public bool Load(int index)
        {
            bool ok = true;
            try
            {
                if (index >= 0 && index < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                {
                    string midiname = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[index];
                    TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, midiname));
                    midifile = new MidiFile(mididata.bytes, false);
                    if (midifile != null)
                        AnalyseMidi();
                    DeltaTicksPerQuarterNote = midifile.DeltaTicksPerQuarterNote;
                }
                else
                {
                    Debug.LogWarning("MidiLoad - index out of range " + index);
                    ok = false;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from an array of bytes
        /// </summary>
        /// <param name="datamidi"></param>
        public bool Load(byte[] datamidi)
        {
            bool ok = true;
            try
            {
                midifile = new MidiFile(datamidi, false);
                if (midifile != null)
                    AnalyseMidi();
                DeltaTicksPerQuarterNote = midifile.DeltaTicksPerQuarterNote;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        /// <summary>
        /// Load Midi from a Midi file (OS)
        /// </summary>
        /// <param name="filename"></param>
        public bool Load(string filename)
        {
            bool ok = true;
            try
            {
                string pathfilename = BuildOSPath(filename);
                midifile = new MidiFile(pathfilename, false);
                if (midifile != null)
                    AnalyseMidi();
                DeltaTicksPerQuarterNote = midifile.DeltaTicksPerQuarterNote;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
                ok = false;
            }
            return ok;
        }

        private void AnalyseMidi()
        {
            try
            {
                CurrentPulse = 0;
                NextPosEvent = 0;
                LastTimeFromStartMS = 0;
                PatchChanel = new int[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                VolumeChanel = new int[16] { 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127, 127 };
                PanChanel = new int[16] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };

                Chanels = new bool[16] { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
                CountTracks = 0;
                QuarterPerMinuteValue = double.NegativeInfinity;

                SequenceTrackName = "";
                ProgramName = "";
                TrackInstrumentName = "";
                TextEvent = "";
                Copyright = "";

                // Get midi events from midifile.Events
                MidiSorted = GetEvents();

                // If no tempo event found, set a defulat value
                if (QuarterPerMinuteValue < 0d)
                    ChangeTempo(80d);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private List<TrackMidiEvent> GetEvents()
        {
            try
            {
                List<TrackMidiEvent> events = new List<TrackMidiEvent>();
                foreach (IList<MidiEvent> track in midifile.Events)
                {
                    CountTracks++;
                    foreach (MidiEvent e in track)
                    {
                        try
                        {
                            if (e.Channel > 0 && e.Channel <= 16) Chanels[e.Channel - 1] = true;

                            bool keepEvent = false;
                            switch (e.CommandCode)
                            {
                                case MidiCommandCode.NoteOn:
                                    //Debug.Log("NoteOn");
                                    if (KeepNoteOff)
                                        // keep note even if no offevent
                                        keepEvent = true;
                                    else
                                    if (((NoteOnEvent)e).OffEvent != null)
                                        keepEvent = true;
                                    break;
                                case MidiCommandCode.NoteOff:
                                    //Debug.Log("NoteOff");
                                    if (KeepNoteOff)
                                        keepEvent = true;
                                    break;
                                case MidiCommandCode.ControlChange:
                                    //ControlChangeEvent ctrl = (ControlChangeEvent)e;
                                    //Debug.Log("NoteOff");
                                    keepEvent = true;
                                    break;
                                case MidiCommandCode.PatchChange:
                                    keepEvent = true;
                                    break;
                                case MidiCommandCode.MetaEvent:
                                    MetaEvent meta = (MetaEvent)e;
                                    switch (meta.MetaEventType)
                                    {
                                        case MetaEventType.SetTempo:
                                            // Set the first tempo value find
                                            //if (QuarterPerMinuteValue < 0d)
                                            //Debug.Log("MicrosecondsPerQuarterNote:"+((TempoEvent)e).MicrosecondsPerQuarterNote);
                                            ChangeTempo(((TempoEvent)e).Tempo);
                                            break;
                                    }
                                    keepEvent = true;
                                    break;
                            }
                            if (keepEvent)
                                events.Add(new TrackMidiEvent() { IndexTrack = CountTracks, Event = e.Clone() });
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                            //List<TrackMidiEvent> MidiSorted = events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                            return events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                        }
                    }
                }

                /// Sort midi event by time
                List<TrackMidiEvent> MidiSorted = events.OrderBy(o => o.Event.AbsoluteTime).ToList();
                return MidiSorted;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }
        /// <summary>
        /// Change speed to play. 1=normal speed
        /// </summary>
        /// <param name="speed"></param>
        public void ChangeSpeed(float speed)
        {
            try
            {
                //Debug.Log("ChangeSpeed " + speed);
                Speed = speed;
                if (QuarterPerMinuteValue > 0d)
                {
                    ChangeTempo(QuarterPerMinuteValue);
                    //CancelNextReadEvents = true;
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public void ChangeQuantization(int level = 4)
        {
            try
            {
                if (level <= 0)
                    Quantization = 0;
                else
                    Quantization = midifile.DeltaTicksPerQuarterNote / level;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Calculate PulseLenghtMS from QuarterPerMinute value
        /// </summary>
        /// <param name="tempo"></param>
        private void ChangeTempo(double tempo)
        {
            try
            {
                QuarterPerMinuteValue = tempo;
                PulseLengthMs = (1000d / ((QuarterPerMinuteValue * midifile.DeltaTicksPerQuarterNote) / 60f)) / Speed;
                //The BPM measures how many quarter notes happen in a minute. To work out the length of each pulse we can use the following formula: Pulse Length = 60 / (BPM * PPQN)
                //16  Sixteen Double croche

                if (LogEvents)
                {
                    Debug.Log("ChangeTempo");
                    Debug.Log("     QuarterPerMinuteValue :" + QuarterPerMinuteValue);
                    Debug.Log("     Speed :" + Speed);
                    Debug.Log("     DeltaTicksPerQuarterNote :" + midifile.DeltaTicksPerQuarterNote);
                    Debug.Log("     Pulse length in ms :" + PulseLengthMs);
                }

                // UPdate total time of midi play
                if (MidiSorted != null && MidiSorted.Count > 0)
                    Duration = TimeSpan.FromMilliseconds(MidiSorted[MidiSorted.Count - 1].Event.AbsoluteTime * PulseLengthMs);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Get current time of playing
        /// </summary>
        /// <returns></returns>
        public TimeSpan CurrentMidiTime()
        {
            return TimeSpan.FromMilliseconds(CurrentPulse * PulseLengthMs);
        }

        public void CalculateNextPosEvents(double timeFromStartMS)
        {
            if (MidiSorted != null)
            {
                CurrentPulse = Convert.ToInt64(timeFromStartMS / PulseLengthMs);
                //Debug.Log(">>> CalculateNextPosEvents - CurrentPulse:" + CurrentPulse + " CurrentNextPosEvent:" + NextPosEvent + " LastTimeFromStartMS:" + LastTimeFromStartMS + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                if (CurrentPulse == 0)
                {
                    NextPosEvent = 0;
                    LastTimeFromStartMS = 0;
                }
                else
                {
                    LastTimeFromStartMS = timeFromStartMS;
                    // From the last position played
                    for (int currentPosEvent = 0; currentPosEvent < MidiSorted.Count; currentPosEvent++)
                    {
                        TrackMidiEvent trackEvent = MidiSorted[currentPosEvent];
                        //if (currentPosEvent + 1 < MidiSorted.Count)
                        {
                            //TrackMidiEvent nexttrackEvent = MidiSorted[currentPosEvent + 1];
                            //Debug.Log("CurrentPulse:" + CurrentPulse+ " trackEvent:" + trackEvent.AbsoluteQuantize+ " nexttrackEvent:" + nexttrackEvent.AbsoluteQuantize);

                            if (trackEvent.Event.AbsoluteTime > CurrentPulse)// && CurrentPulse < nexttrackEvent.Event.AbsoluteTime )
                            {
                                NextPosEvent = currentPosEvent;
                                //Debug.Log("     CalculateNextPosEvents - NextPosEvent:" + NextPosEvent + " trackEvent:" + trackEvent.Event.AbsoluteTime + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                                //Debug.Log("NextPosEvent:" + NextPosEvent);
                                break;
                            }
                            //if (currentPosEvent == MidiSorted.Count - 1) Debug.Log("Last CalculateNextPosEvents - currentPosEvent:" + currentPosEvent + " trackEvent:" + trackEvent.Event.AbsoluteTime + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS));
                        }
                    }
                }
                //Debug.Log("<<< CalculateNextPosEvents NextPosEvent:" + NextPosEvent);
            }
        }
        public List<MidiNote> ReadMidiEvents(double timeFromStartMS)
        {
            List<MidiNote> notes = null;
            try
            {
                EndMidiEvent = false;
                if (midifile != null)
                {
                    if (NextPosEvent < MidiSorted.Count)
                    {
                        // The BPM measures how many quarter notes happen in a minute. To work out the length of each pulse we can use the following formula: 
                        // Pulse Length = 60 / (BPM * PPQN)
                        // Calculate current pulse to play
                        CurrentPulse += Convert.ToInt64((timeFromStartMS - LastTimeFromStartMS) / PulseLengthMs);

                        LastTimeFromStartMS = timeFromStartMS;

                        // From the last position played
                        for (int currentPosEvent = NextPosEvent; currentPosEvent < MidiSorted.Count; currentPosEvent++)
                        {
                            TrackMidiEvent trackEvent = MidiSorted[currentPosEvent];
                            if (Quantization != 0)
                                trackEvent.AbsoluteQuantize = ((trackEvent.Event.AbsoluteTime + Quantization / 2) / Quantization) * Quantization;
                            else
                                trackEvent.AbsoluteQuantize = trackEvent.Event.AbsoluteTime;

                            //Debug.Log("ReadMidiEvents - timeFromStartMS:" + Convert.ToInt32(timeFromStartMS) + " LastTimeFromStartMS:" + Convert.ToInt32(LastTimeFromStartMS) + " CurrentPulse:" + CurrentPulse + " AbsoluteQuantize:" + trackEvent.AbsoluteQuantize);

                            if (trackEvent.AbsoluteQuantize <= CurrentPulse)
                            {
                                NextPosEvent = currentPosEvent + 1;

                                if (trackEvent.Event.CommandCode == MidiCommandCode.NoteOn)
                                {
                                    if (((NoteOnEvent)trackEvent.Event).OffEvent != null)
                                    {
                                        NoteOnEvent noteon = (NoteOnEvent)trackEvent.Event;
                                        // if (noteon.OffEvent != null)
                                        {
                                            if (notes == null) notes = new List<MidiNote>();

                                            //Debug.Log(string.Format("Track:{0} NoteNumber:{1,3:000} AbsoluteTime:{2,6:000000} NoteLength:{3,6:000000} OffDeltaTime:{4,6:000000} ", track, noteon.NoteNumber, noteon.AbsoluteTime, noteon.NoteLength, noteon.OffEvent.DeltaTime));
                                            MidiNote note = new MidiNote()
                                            {
                                                AbsoluteQuantize = trackEvent.AbsoluteQuantize,
                                                Midi = noteon.NoteNumber,
                                                Channel = trackEvent.Event.Channel,
                                                Velocity = noteon.Velocity,
                                                Duration = noteon.NoteLength * PulseLengthMs,
                                                Length = noteon.NoteLength,
                                                Patch = PatchChanel[trackEvent.Event.Channel - 1],
                                                Drum = (trackEvent.Event.Channel == 10),
                                                Delay = 0,
                                                Pan = EnablePanChange ? PanChanel[trackEvent.Event.Channel - 1] : -1,

                                            };
                                            if (VolumeChanel[note.Channel - 1] != 127)
                                                note.Velocity = Mathf.RoundToInt(((float)note.Velocity) * ((float)VolumeChanel[trackEvent.Event.Channel - 1]) / 127f);
                                            notes.Add(note);
                                            if (LogEvents)
                                                Debug.Log(BuildInfoTrack(trackEvent) + string.Format("{0,-4} {1,3:000} Lenght:{2} {3} Veloc:{4}",
                                                    noteon.NoteName, noteon.NoteNumber, noteon.NoteLength, NoteLength(note), noteon.Velocity));
                                        }
                                    }
                                }
                                else if (trackEvent.Event.CommandCode == MidiCommandCode.NoteOff)
                                {
                                    // no need, noteoff are associated with noteon
                                }
                                else if (trackEvent.Event.CommandCode == MidiCommandCode.ControlChange)
                                {
                                    ControlChangeEvent controlchange = (ControlChangeEvent)trackEvent.Event;
                                    if (controlchange.Controller == MidiController.Expression)
                                        VolumeChanel[trackEvent.Event.Channel - 1] = controlchange.ControllerValue;
                                    else if (controlchange.Controller == MidiController.MainVolume)
                                        VolumeChanel[trackEvent.Event.Channel - 1] = controlchange.ControllerValue;
                                    else if (controlchange.Controller == MidiController.Pan)
                                        PanChanel[trackEvent.Event.Channel - 1] = controlchange.ControllerValue;
                                    // Other midi event
                                    if (LogEvents)
                                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Control {0} {1}", controlchange.Controller, controlchange.ControllerValue));

                                }
                                else if (trackEvent.Event.CommandCode == MidiCommandCode.PatchChange)
                                {
                                    PatchChangeEvent change = (PatchChangeEvent)trackEvent.Event;
                                    PatchChanel[trackEvent.Event.Channel - 1] = trackEvent.Event.Channel == 10 ? 0 : change.Patch;
                                    if (LogEvents)
                                        Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Patch   {0,3:000} {1}", change.Patch, PatchChangeEvent.GetPatchName(change.Patch)));
                                }
                                else if (trackEvent.Event.CommandCode == MidiCommandCode.MetaEvent)
                                {
                                    MetaEvent meta = (MetaEvent)trackEvent.Event;
                                    switch (meta.MetaEventType)
                                    {
                                        case MetaEventType.SetTempo:
                                            if (EnableChangeTempo)
                                            {
                                                TempoEvent tempo = (TempoEvent)meta;
                                                //NewQuarterPerMinuteValue = tempo.Tempo;
                                                ChangeTempo(tempo.Tempo);
                                                //if (LogEvents)Debug.Log(BuildInfoTrack(trackEvent) + string.Format("SetTempo   {0} MicrosecondsPerQuarterNote:{1}", tempo.Tempo, tempo.MicrosecondsPerQuarterNote));
                                            }
                                            break;
                                        case MetaEventType.SequenceTrackName:
                                            if (!string.IsNullOrEmpty(SequenceTrackName)) SequenceTrackName += "\n";
                                            SequenceTrackName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, ((TextEvent)meta).Text);
                                            if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Sequence   '{0}'", ((TextEvent)meta).Text));
                                            break;
                                        case MetaEventType.ProgramName:
                                            ProgramName += ((TextEvent)meta).Text + " ";
                                            if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Program   '{0}'", ((TextEvent)meta).Text));
                                            break;
                                        case MetaEventType.TrackInstrumentName:
                                            if (!string.IsNullOrEmpty(TrackInstrumentName)) TrackInstrumentName += "\n";
                                            TrackInstrumentName += string.Format("T{0,2:00} {1}", trackEvent.IndexTrack, ((TextEvent)meta).Text);
                                            if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Text      '{0}'", ((TextEvent)meta).Text));
                                            break;
                                        case MetaEventType.TextEvent:
                                            TextEvent += ((TextEvent)meta).Text + " ";
                                            if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Sequence  '{0}'", ((TextEvent)meta).Text));
                                            break;
                                        case MetaEventType.Copyright:
                                            Copyright += ((TextEvent)meta).Text + " ";
                                            if (LogEvents) Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Copyright '{0}'", ((TextEvent)meta).Text));
                                            break;
                                        case MetaEventType.Lyric: // lyric
                                        case MetaEventType.Marker: // marker
                                        case MetaEventType.CuePoint: // cue point
                                        case MetaEventType.DeviceName:
                                            break;
                                    }
                                    //Debug.Log(BuildInfoTrack(trackEvent) + string.Format("Meta {0} {1}", meta.MetaEventType, meta.ToString()));
                                }
                                else
                                {
                                    // Other midi event
                                    //Debug.Log(string.Format("Track:{0} Channel:{1,2:00} CommandCode:{2,3:000} AbsoluteTime:{3,6:000000}", track, e.Channel, e.CommandCode.ToString(), e.AbsoluteTime));
                                }
                            }
                            else
                                // Out of time, exit for loop
                                break;
                        }

                        if (notes != null)
                        {
                            //if (CancelNextReadEvents)
                            //{
                            //    notes = null;
                            //    //Debug.Log("CancelNextReadEvents");
                            //    CancelNextReadEvents = false;
                            //}
                            //else
                            //if (notes.Count > 3 && (notes[notes.Count - 1].AbsoluteQuantize - notes[0].AbsoluteQuantize) > midifile.DeltaTicksPerQuarterNote * 8)
                            //{
                            //    //notes.RemoveRange(0, notes.Count - 1);
                            //    Debug.Log("--> Too much notes " + notes.Count + " timeFromStartMS:" + Convert.ToInt32(timeFromStartMS) + " Start:" + notes[0].AbsoluteQuantize + " Ecart:" + (notes[notes.Count - 1].AbsoluteQuantize - notes[0].AbsoluteQuantize) + " CurrentPulse:" + CurrentPulse);
                            //    //notes = null;
                            //}
                        }
                    }
                    else
                    {
                        // End of midi events
                        EndMidiEvent = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return notes;
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Note_value
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public MidiNote.EnumLength NoteLength(MidiNote note)
        {
            if (midifile != null)
            {
                if (note.Length >= midifile.DeltaTicksPerQuarterNote * 4)
                    return MidiNote.EnumLength.Whole;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote * 2)
                    return MidiNote.EnumLength.Half;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote)
                    return MidiNote.EnumLength.Quarter;
                else if (note.Length >= midifile.DeltaTicksPerQuarterNote / 2)
                    return MidiNote.EnumLength.Eighth;
            }
            return MidiNote.EnumLength.Sixteenth;
        }

        private string BuildInfoTrack(TrackMidiEvent e)
        {
            return string.Format("[A:{0,5:00000} Q:{1,5:00000} P:{2,5:00000}] [T:{3,2:00} C:{4,2:00}] ", e.Event.AbsoluteTime, e.AbsoluteQuantize, CurrentPulse, e.IndexTrack, e.Event.Channel);
        }

        public void DebugTrack()
        {
            int itrck = 0;
            foreach (IList<MidiEvent> track in midifile.Events)
            {
                itrck++;
                foreach (MidiEvent midievent in track)
                {
                    string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", itrck, midievent.Channel, midievent.CommandCode, midievent.AbsoluteTime);
                    if (midievent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        NoteOnEvent noteon = (NoteOnEvent)midievent;
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                    }
                    Debug.Log(info);
                }
            }
        }
        public void DebugMidiSorted()
        {
            foreach (TrackMidiEvent midievent in MidiSorted)
            {
                string info = string.Format("Track:{0} Channel:{1,2:00} Command:{2} AbsoluteTime:{3:0000000} ", midievent.IndexTrack, midievent.Event.Channel, midievent.Event.CommandCode, midievent.Event.AbsoluteTime);
                switch (midievent.Event.CommandCode)
                {
                    case MidiCommandCode.NoteOn:
                        NoteOnEvent noteon = (NoteOnEvent)midievent.Event;
                        if (noteon.Velocity == 0)
                            info += string.Format(" Velocity 0");
                        if (noteon.OffEvent == null)
                            info += string.Format(" OffEvent null");
                        else
                            info += string.Format(" OffEvent.DeltaTimeChannel:{0:0000.00} ", noteon.OffEvent.DeltaTime);
                        break;
                }
                Debug.Log(info);
            }
        }

    }

}

