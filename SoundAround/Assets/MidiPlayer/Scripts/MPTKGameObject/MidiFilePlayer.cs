
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    /// <summary>
    /// Script for the prefab MidiFilePlayer. 
    /// Play a selected midi file. 
    /// List of Midi file must be defined with Midi Player Setup (menu tools).
    /// </summary>
    public class MidiFilePlayer : MidiPlayer
    {
        /// <summary>
        /// Midi name to play. 
        /// Must exists in MidiPlayerGlobal.CurrentMidiSet.MidiFiles.
        /// List of Midi file must be defined with Midi Player Setup (menu tools)
        /// and contains that name.
        /// </summary>
        public virtual string MPTK_MidiName { get { return midiNameToPlay; } set { midiNameToPlay = value; } }
        [SerializeField]
        [HideInInspector]
        private string midiNameToPlay;

        /// <summary>
        /// Should the Midi start playing when application start ?
        /// </summary>
        public virtual bool MPTK_PlayOnStart { get { return playOnStart; } set { playOnStart = value; } }

        /// <summary>
        /// Should automatically restart when Midi reach the end ?
        /// </summary>
        public virtual bool MPTK_Loop { get { return loop; } set { loop = value; } }

        /// <summary>
        /// Get default tempo defined in Midi file or modified with Speed. 
        /// Return QuarterPerMinuteValue similar to BPM (Beat Per Measure)
        /// </summary>
        public virtual double MPTK_Tempo { get { if (miditoplay != null) return miditoplay.QuarterPerMinuteValue; else return 0d; } }


        public string MPTK_SequenceTrackName { get { return miditoplay != null ? miditoplay.SequenceTrackName : ""; } }
        public string MPTK_ProgramName { get { return miditoplay != null ? miditoplay.ProgramName : ""; } }
        public string MPTK_TrackInstrumentName { get { return miditoplay != null ? miditoplay.TrackInstrumentName : ""; } }
        public string MPTK_TextEvent { get { return miditoplay != null ? miditoplay.TextEvent : ""; } }
        public string MPTK_Copyright { get { return miditoplay != null ? miditoplay.Copyright : ""; } }

        /// <summary>
        /// Speed of playing. 
        /// Between 0.1 (10%) to 5.0 (500%). 
        /// Set to 1 for normal speed. 
        /// </summary>
        public virtual float MPTK_Speed
        {
            get { return speed; }
            set
            {
                try
                {
                    if (value >= 0.1f && value <= 5.0f)
                    {
                        MPTK_Pause(0.3f);
                        speed = value;
                        if (miditoplay != null)
                            miditoplay.ChangeSpeed(speed);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Speed value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        /// <summary>
        /// Position to play from 0 ms to lenght time of midi playing (in millisecond)
        /// </summary>
        public virtual float MPTK_Position
        {
            get { return (float)timeFromStartPlay; }
            set
            {
                try
                {
                    if (value >= 0f && value <= (float)MPTK_Duration.TotalMilliseconds)
                    {
                        MPTK_Pause(0.2f);
                        timeFromStartPlay = value;
                        if (miditoplay != null)
                            miditoplay.CalculateNextPosEvents(timeFromStartPlay);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Position value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        [SerializeField]
        [HideInInspector]
        private float speed = 1f;

        /// <summary>
        /// Is Midi file playing is paused ?
        /// </summary>
        public virtual bool MPTK_IsPaused { get { return playPause; } }

        /// <summary>
        /// Is Midi file is playing ?
        /// </summary>
        public virtual bool MPTK_IsPlaying { get { return midiIsPlaying; } }

        /// <summary>
        /// Value updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public string durationEditorModeOnly;

        /// <summary>
        /// Get duration of current Midi with current tempo
        /// </summary>
        public virtual TimeSpan MPTK_Duration { get { try { if (miditoplay != null) return miditoplay.Duration; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Lenght in millisecond of a quarter
        /// </summary>
        public virtual float MPTK_PulseLenght { get { try { if (miditoplay != null) return (float)miditoplay.PulseLengthMs; } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return 0f; } }

        /// <summary>
        /// Updated only when playing in Unity (for inspector refresh)
        /// </summary>
        public string playTimeEditorModeOnly;

        /// <summary>
        /// Time from the start of playing the current midi
        /// </summary>
        public virtual TimeSpan MPTK_PlayTime { get { try { return TimeSpan.FromMilliseconds(timeFromStartPlay); } catch (System.Exception ex) { MidiPlayerGlobal.ErrorDetail(ex); } return TimeSpan.Zero; } }

        /// <summary>
        /// Log midi events
        /// </summary>
        public virtual bool MPTK_LogEvents
        {
            get { return logEvents; }
            set { logEvents = value; }
        }

        /// <summary>
        /// Should change tempo from Midi Events ? 
        /// </summary>
        public virtual bool MPTK_EnableChangeTempo
        {
            get { return enableChangeTempo; }
            set { enableChangeTempo = value; }
        }

        /// <summary>
        /// Should change tempo from Midi Events ? 
        /// </summary>
        public virtual bool MPTK_KeepNoteOff
        {
            get { return keepNoteOff; }
            set { keepNoteOff = value; }
        }

        /// <summary>
        /// Should change pan from Midi Events ? 
        /// </summary>
        public virtual bool MPTK_EnablePanChange
        {
            get { return enablePanChange; }
            set { enablePanChange = value; }
        }

        /// <summary>
        /// If true (default) then Midi events are sent automatically to the midi player when available. [version 1.7]
        /// Set to false if you want to process events before playing. 
        /// OnEventNotesMidi can be used to process each notes.
        /// </summary>
        public virtual bool MPTK_DirectSendToPlayer
        {
            get { return sendNotes; }
            set { sendNotes = value; }
        }

        [System.Serializable]
        public class ListNotesEvent : UnityEvent<List<MidiNote>>
        {
        }

        /// <summary>
        /// Define unity event to trigger when notes available from the Midi file. [version 1.7]
        /// </summary>
        [HideInInspector]
        public ListNotesEvent OnEventNotesMidi;


        /// <summary>
        /// Define unity event to trigger at start
        /// </summary>
        [HideInInspector]
        public UnityEvent OnEventStartPlayMidi;

        /// <summary>
        /// Define unity event to trigger at end
        /// </summary>
        [HideInInspector]
        public UnityEvent OnEventEndPlayMidi;

        /// <summary>
        /// Level of quantization : 
        ///     0 = none to 
        ///     5 = 64th Note
        /// </summary>
        public virtual int MPTK_Quantization
        {
            get { return quantization; }
            set
            {
                try
                {
                    if (value >= 0 && value <= 5)
                    {
                        quantization = value;
                        miditoplay.ChangeQuantization(quantization);
                    }
                    else
                        Debug.LogWarning("MidiFilePlayer - Set Quantization value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }
        [SerializeField]
        [HideInInspector]
        private int quantization = 0;


        [SerializeField]
        [HideInInspector]
        private bool playOnStart = false, newMidiToPlay = false, stopMidiToPlay = false,
            midiIsPlaying = false, loop = false, sendNotes = true,
            logEvents = false, enableChangeTempo = true, keepNoteOff = false, enablePanChange = true;

        [SerializeField]
        [HideInInspector]
        protected bool playPause = false;

        private float delayMilliSeconde = 10f;
        private float timeToPauseMilliSeconde = -1f;
        public double timeFromStartPlay = 0d;
        private MidiLoad miditoplay;

        /// <summary>
        /// New 1.9
        /// </summary>
        public virtual List<TrackMidiEvent> MPTK_MidiEvents
        {
            get
            {
                List<TrackMidiEvent> tme = null;
                try
                {
                    tme = miditoplay.MidiSorted;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return tme;
            }
        }

        public virtual int MPTK_DeltaTicksPerQuarterNote
        {
            get
            {
                int DeltaTicksPerQuarterNote = 0;
                try
                {
                    DeltaTicksPerQuarterNote = miditoplay.DeltaTicksPerQuarterNote;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return DeltaTicksPerQuarterNote;
            }
        }

        /// <summary>
        /// Index Midi to play or playing. 
        /// return -1 if not found
        /// </summary>
        /// <param name="index"></param>
        public virtual int MPTK_MidiIndex
        {
            get
            {
                try
                {
                    return MidiPlayerGlobal.MPTK_FindMidi(MPTK_MidiName);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
                return -1;
            }
            set
            {
                try
                {
                    if (value >= 0 && value < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[value];
                    else
                        Debug.LogWarning("MidiFilePlayer - Set MidiIndex value not valid : " + value);
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
        }

        new void Awake()
        {
            //Debug.Log("Awake midiIsPlaying:" + midiIsPlaying);
            midiIsPlaying = false;
            base.Awake();
        }

        new void Start()
        {
            //Debug.Log("Start midiIsPlaying:" + midiIsPlaying);
            base.Start();
            try
            {
                if (MPTK_PlayOnStart)
                {
                    StartCoroutine(TheadPlayIfReady());
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        protected IEnumerator TheadPlayIfReady()
        {
            while (!MidiPlayerGlobal.SoundFontLoaded)
                yield return new WaitForSeconds(0.2f);

            // Wait a few of millisecond to let app to start (usefull when play on start)
            yield return new WaitForSeconds(0.2f);

            MPTK_Play();
        }

        /// <summary>
        /// Play the midi file defined in MPTK_MidiName
        /// </summary>
        public virtual void MPTK_Play()
        {
            try
            {
                if (MidiPlayerGlobal.SoundFontLoaded)
                {
                    playPause = false;
                    if (!midiIsPlaying)
                    {
                        // Load description of available soundfont
                        if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                        {
                            //Debug.Log(MPTK_MidiName);
                            if (string.IsNullOrEmpty(MPTK_MidiName))
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            int selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                            if (selectedMidi < 0)
                            {
                                Debug.LogWarning("MidiFilePlayer - MidiFile " + MPTK_MidiName + " not found. Try with the first in list.");
                                selectedMidi = 0;
                                MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[0];
                            }

                            StartCoroutine(ThreadPlay());
                            //StartCoroutine(TestWithDelay());
                        }
                        else
                            Debug.LogWarning("MidiFilePlayer - no SoundFont or Midi set defined, go to Unity menu Tools to setup MPTK");
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public virtual void MPTK_Stop()
        {
            try
            {
                midiIsPlaying = false;
                playPause = false;
                stopMidiToPlay = true;
                MPTK_ClearAllSound();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Restart playing the current midi file
        /// </summary>
        public virtual void MPTK_RePlay()
        {
            try
            {
                playPause = false;
                if (midiIsPlaying)
                {
                    MPTK_ClearAllSound();
                    newMidiToPlay = true;
                }
                else
                    MPTK_Play();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Pause the current playing
        /// </summary>
        public virtual void MPTK_Pause(float timeToPauseMS = -1f)
        {
            try
            {
                timeToPauseMilliSeconde = timeToPauseMS;
                playPause = true;
                MPTK_ClearAllSound();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play next Midi in list
        /// </summary>
        public virtual void MPTK_Next()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi++;
                        if (selectedMidi >= MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count)
                            selectedMidi = 0;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                        MPTK_RePlay();
                    }
                }
                else
                    Debug.LogWarning("MidiFilePlayer - no Midi defined, go to menu 'Tools/MPTK - Midi File Setup' or alt-m");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Play previous Midi in list
        /// </summary>
        public virtual void MPTK_Previous()
        {
            try
            {
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null && MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count > 0)
                {
                    int selectedMidi = 0;
                    if (!string.IsNullOrEmpty(MPTK_MidiName))
                        selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.FindIndex(s => s == MPTK_MidiName);
                    if (selectedMidi >= 0)
                    {
                        selectedMidi--;
                        if (selectedMidi < 0)
                            selectedMidi = MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count - 1;
                        MPTK_MidiName = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[selectedMidi];
                        MPTK_RePlay();
                    }
                }
                else
                    Debug.LogWarning("MidiFilePlayer - no Midi defined, go to menu 'Tools/MPTK - Midi File Setup' or alt-m");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private float lastTimePlay = 0;
        /// <summary>
        /// In case of delay in the application, resync is usefull to avoid multi tock play at the same time
        /// </summary>
        public void MPTK_ReSyncTime()
        {
            lastTimePlay = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Return note length as https://en.wikipedia.org/wiki/Note_value [New 1.9]
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        public MidiNote.EnumLength NoteLength(MidiNote note)
        {
            if (miditoplay != null)
                return miditoplay.NoteLength(note);
            return MidiNote.EnumLength.Sixteenth;
        }

        protected IEnumerator ThreadPlay(byte[] midibytestoplay = null)
        {
            midiIsPlaying = true;
            stopMidiToPlay = false;
            newMidiToPlay = false;
            bool first = true;
            //Debug.Log("Start play");
            try
            {
                miditoplay = new MidiLoad();

                // No midi byte array, try to load from MidiFilesDN from resource
                if (midibytestoplay == null || midibytestoplay.Length == 0)
                {
                    TextAsset mididata = Resources.Load<TextAsset>(Path.Combine(MidiPlayerGlobal.MidiFilesDB, MPTK_MidiName));
                    midibytestoplay = mididata.bytes;
                }

                miditoplay.KeepNoteOff = MPTK_KeepNoteOff;
                miditoplay.Load(midibytestoplay);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (miditoplay != null)
            {
                yield return StartCoroutine(MPTK_ClearAllSound(true));

                try
                {
                    OnEventStartPlayMidi.Invoke();
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    miditoplay.ChangeSpeed(MPTK_Speed);
                    miditoplay.ChangeQuantization(MPTK_Quantization);

                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                lastTimePlay = Time.realtimeSinceStartup;
                timeFromStartPlay = 0d;
                // Loop on each events midi
                do
                {
                    miditoplay.LogEvents = MPTK_LogEvents;
                    miditoplay.EnableChangeTempo = MPTK_EnableChangeTempo;
                    miditoplay.EnablePanChange = MPTK_EnablePanChange;

                    if (MPTK_PauseOnDistance)
                    {
                        distanceEditorModeOnly = MidiPlayerGlobal.MPTK_DistanceToListener(this.transform);
                        if (distanceEditorModeOnly > AudioSourceTemplate.maxDistance)
                        {
                            lastTimePlay = Time.realtimeSinceStartup;
                            yield return new WaitForSeconds(0.2f);
                            continue;
                        }
                    }

                    if (playPause)
                    {
                        lastTimePlay = Time.realtimeSinceStartup;
                        yield return new WaitForSeconds(0.2f);
                        if (miditoplay.EndMidiEvent || newMidiToPlay || stopMidiToPlay)
                        {
                            break;
                        }
                        if (timeToPauseMilliSeconde > -1f)
                        {
                            timeToPauseMilliSeconde -= 0.2f;
                            if (timeToPauseMilliSeconde <= 0f)
                                playPause = false;
                        }
                        continue;
                    }

                    if (!first)
                    {
                        timeFromStartPlay += (Time.realtimeSinceStartup - lastTimePlay) * 1000f;
                    }
                    else
                    {
                        timeFromStartPlay = 0d;
                        first = false;
                    }
                    lastTimePlay = Time.realtimeSinceStartup;

                    //Debug.Log("---------------- " + timeFromStartPlay );
                    // Read midi events until this time
                    List<MidiNote> notes = miditoplay.ReadMidiEvents(timeFromStartPlay);

                    if (miditoplay.EndMidiEvent || newMidiToPlay || stopMidiToPlay)
                    {
                        break;
                    }

                    // Play notes read
                    if (notes != null && notes.Count > 0)
                    {
                        try
                        {
                            if (OnEventNotesMidi != null)
                                OnEventNotesMidi.Invoke(notes);
                        }
                        catch (System.Exception ex)
                        {
                            MidiPlayerGlobal.ErrorDetail(ex);
                        }

                        if (MPTK_DirectSendToPlayer)
                            MPTK_PlayNotes(notes);
                        //Debug.Log("---------------- play count:" + notes.Count + " " + timeFromStartMS );
                    }
                    if (Application.isEditor)
                    {
                        TimeSpan times = TimeSpan.FromMilliseconds(timeFromStartPlay);
                        playTimeEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", times.Hours, times.Minutes, times.Seconds, times.Milliseconds);
                        durationEditorModeOnly = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", MPTK_Duration.Hours, MPTK_Duration.Minutes, MPTK_Duration.Seconds, MPTK_Duration.Milliseconds);
                    }

                    yield return new WaitForSeconds(delayMilliSeconde / 1000f);// 0.01f);
                }
                while (true);
            }

            midiIsPlaying = false;

            try
            {
                if (OnEventEndPlayMidi != null && !stopMidiToPlay && !newMidiToPlay)
                    OnEventEndPlayMidi.Invoke();

                if ((MPTK_Loop || newMidiToPlay) && !stopMidiToPlay)
                    MPTK_Play();
                //stopMidiToPlay = false;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            //Debug.Log("Stop play");

        }

        /// <summary>
        /// For unitary test
        /// </summary>
        /// <returns></returns>
        IEnumerator TestWithDelay()
        {
            int velocity = 62;
            int startmidi = 66;
            int endmidi = 68;
            //int startmidi = 30;
            //int endmidi = 108;
            int duration = 500;
            do
            {
                List<MidiNote> notes = new List<MidiNote>();
                for (int note = startmidi; note <= endmidi; note++)
                    notes.Add(new MidiNote() { Midi = note, Velocity = velocity, Duration = duration, Delay = (note - startmidi) * duration });

                MPTK_PlayNotes(notes);
                yield return new WaitForSeconds((endmidi - startmidi) * duration / 1000f + 2f);
                Debug.Log("End loop");
            } while (MPTK_Loop && !stopMidiToPlay);
        }

        //private string InfoNote(MidiNote note)
        //{
        //    return string.Format("Time:{0,8} Pitch:{1:0.00000000} Duration:{2:00000} Delay:{3:000000}", Time.realtimeSinceStartup, note.Pitch, note.Duration, note.Delay);
        //}


    }
}

