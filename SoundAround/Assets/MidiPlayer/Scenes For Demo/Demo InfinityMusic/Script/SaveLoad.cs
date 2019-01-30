using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using MidiPlayerTK;
#if UNITY_EDITOR
using UnityEditor;
#endif
//using System.Text;

namespace InfinityMusic
{

    public class SaveLoad
    {
        public int MeasureLength;
        public int MaxMeasure;
        public int QuarterPerMinute;
        public string SongName;
        public string Description;

        public List<ImMathMotif> Motifs;
        public List<ImCadence> Cadences;
        //public List<ImChorder> Chorders;
        //public List<ImMidiMotif> Midis;
        //public List<ImModifier> Modifiers;
        //public List<ImDrum> Drums;
        //public List<ImActivator> Activators;

        public SaveLoad()
        {

        }

        static public void Delete(string songname)
        {
            string Filepath = Path.Combine(Application.persistentDataPath, songname + "." + MidiPlayerGlobal.ExtensionSong);
            //Debug.Log("Delete " + Filepath);

            try
            {
                File.Delete(Filepath);
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            catch (System.Exception)
            {
            }
        }
        static public void UtSave(string filepath)
        {
            // Use this for saving or loadibg
            SaveLoad slSong;

            slSong = new SaveLoad
            {
                MeasureLength = InfinityMusic.instance.MeasureLength,
                MaxMeasure = InfinityMusic.instance.MaxMeasure,
                QuarterPerMinute = InfinityMusic.instance.QuarterPerMinute,
                SongName = InfinityMusic.instance.SongName,
                Description = InfinityMusic.instance.Description
            };

            // Math Motif
            UtMathMotif[] motifs = GameObject.FindObjectsOfType<UtMathMotif>();
            if (motifs.Length > 0)
            {
                slSong.Motifs = new List<ImMathMotif>();
                foreach (UtMathMotif ut in motifs)
                {
                    ImMathMotif im = new ImMathMotif();
                    // Common
                    SaveCommon(ut, im);

                    // Specific
                    im.MeasureCount = ut.MeasureCount;
                    im.OctaveMin = ut.OctaveMin;
                    im.OctaveMax = ut.OctaveMax;
                    im.ScaleIndex = ut.ScaleIndex;
                    im.PatchIndex = ut.PatchIndex;
                    im.DrumKit = ut.DrumKit;
                    im.SelectedAlgo = ut.SelectedAlgo;
                    im.StepInScale = ut.StepInScale;
                    im.RotationSpeed = ut.RotationSpeed;
                    im.Accentuation = ut.Accentuation;
                    im.Velocity = ut.Velocity;
                    im.RepeatRate = ut.RepeatRate;
                    im.IdCadence = ut.CurrentCadence != null ? ut.CurrentCadence.UtId : -1;
                    im.Notes = (ut.Notes != null && ut.Notes.Count > 0) ? new List<MathMotifNote>(ut.Notes) : null;

                    slSong.Motifs.Add(im);
                }
            }

            //// Math Motif
            //UtChorder[] chorders = GameObject.FindObjectsOfType<UtChorder>();
            //if (chorders.Length > 0)
            //{
            //    slSong.Chorders = new List<ImChorder>();
            //    foreach (UtChorder ut in chorders)
            //    {
            //        ImChorder im = new ImChorder();
            //        // Common
            //        SaveCommon(ut, im);

            //        // Specific
            //        im.Trigger = ut.Trigger;
            //        im.StepCount = ut.StepCount;
            //        im.ChordProgression = ut.ChordProgression;

            //        slSong.Chorders.Add(im);
            //    }
            //}

            //// Midi Motif
            //UtMidiMotif[] midis = GameObject.FindObjectsOfType<UtMidiMotif>();
            //if (midis.Length > 0)
            //{
            //    slSong.Midis = new List<ImMidiMotif>();
            //    foreach (UtMidiMotif ut in midis)
            //    {
            //        ImMidiMotif im = new ImMidiMotif();
            //        // Common
            //        SaveCommon(ut, im);

            //        // Specific
            //        im.MidiName = ut.MidiName;
            //        im.Transpose = ut.Transpose;
            //        im.StartPlay = ut.StartPlay;
            //        im.EndPlay = ut.EndPlay;
            //        im.DeltaTicksPerQuarterNote = ut.DeltaTicksPerQuarterNote;
            //        im.NumberBeatsMeasure = ut.NumberBeatsMeasure;
            //        im.NumberQuarterBeat = ut.NumberQuarterBeat;
            //        im.SelectedMode = ut.SelectedMode;
            //        im.StepPlay = ut.StepPlay;
            //        //im.Measures = ut.MidiMeasures;
            //        im.Tracks = ut.Tracks;

            //        slSong.Midis.Add(im);
            //    }
            //}

            // Cadence
            UtCadence[] cadences = GameObject.FindObjectsOfType<UtCadence>();
            if (cadences.Length > 0)
            {
                slSong.Cadences = new List<ImCadence>();
                foreach (UtCadence ut in cadences)
                {
                    ImCadence im = new ImCadence();
                    // Common
                    SaveCommon(ut, im);
                    // Specific
                    im.MeasureCount = ut.MeasureCount;
                    im.PctSilence = ut.PctSilence;
                    im.RatioWhole = ut.RatioWhole;
                    im.RatioHalf = ut.RatioHalf;
                    im.RatioQuarter = ut.RatioQuarter;
                    im.RatioEighth = ut.RatioEighth;
                    im.RatioSixteen = ut.RatioSixteen;
                    im.Durations = (ut.Durations != null && ut.Durations.Count > 0) ? new List<Cadence>(ut.Durations) : null;

                    slSong.Cadences.Add(im);
                }
            }

            // Modifier
            //UtModifier[] modifiers = GameObject.FindObjectsOfType<UtModifier>();
            //if (modifiers.Length > 0)
            //{
            //    slSong.Modifiers = new List<ImModifier>();
            //    foreach (UtModifier ut in modifiers)
            //    {
            //        ImModifier im = new ImModifier();
            //        // Common
            //        SaveCommon(ut, im);
            //        // Specific
            //        im.MeasureCount = ut.MeasureCount;
            //        im.SelectedMode = ut.SelectedMode;
            //        im.IndexApplyTo = ut.IndexApplyTo;
            //        im.IndexProperties = ut.IndexProperties;
            //        im.MinSelected = ut.FromSelected;
            //        im.MaxSelected = ut.ToSelected;
            //        im.Step = ut.Step;
            //        slSong.Modifiers.Add(im);
            //    }
            //}

            ////
            //// Drum
            ////
            //UtDrum[] drums = GameObject.FindObjectsOfType<UtDrum>();
            //if (drums.Length > 0)
            //{
            //    slSong.Drums = new List<ImDrum>();
            //    foreach (UtDrum ut in drums)
            //    {
            //        ImDrum im = new ImDrum();
            //        // Common
            //        SaveCommon(ut, im);
            //        // Specific
            //        im.Tracks = ut.Tracks;
            //        im.RandomLevel = ut.RandomLevel;
            //        slSong.Drums.Add(im);
            //    }
            //}

            ////
            //// Activator
            ////
            //UtActivator[] activators = GameObject.FindObjectsOfType<UtActivator>();
            //if (activators.Length > 0)
            //{
            //    slSong.Activators = new List<ImActivator>();
            //    foreach (UtActivator ut in activators)
            //    {
            //        ImActivator im = new ImActivator();
            //        // Common
            //        SaveCommon(ut, im);
            //        // Specific
            //        im.MeasureCount = ut.MeasureCount;
            //        im.MeasureStart = ut.MeasureStart;
            //        im.ActivateBeforeStart = ut.ActivateBeforeStart;
            //        im.ActivateAfterLoop = ut.ActivateAfterLoop;
            //        im.LoopCount = ut.LoopCount;
            //        im.Activates = ut.Activates;
            //        slSong.Activators.Add(im);
            //    }
            //}

            slSong.SaveXML(filepath);
#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif
        }

        static public void UtLoad(string songname)
        {
            SaveLoad slSong;

            Debug.Log(">>> Load");
            lock (InfinityMusic.instance)
            {
                try
                {
                    slSong = SaveLoad.LoadXML(songname);
                    if (slSong != null)
                    {
                        // Cadence
                        //Debug.Log("Cadence Count:" + slSong.Cadences.Count);
                        foreach (ImCadence im in slSong.Cadences)
                        {
                            Debug.Log(" " + im.Name);
                            UtCadence ut = (UtCadence)InfinityMusic.Instantiate(InfinityMusic.instance.TemplateCadence, Vector3.zero, Quaternion.identity);
                            ut.transform.parent = InfinityMusic.instance.transform;
                            // Common
                            LoadCommon(im, ut);

                            // Specific
                            ut.MeasureCount = im.MeasureCount;
                            ut.PctSilence = im.PctSilence;
                            ut.RatioWhole = im.RatioWhole;
                            ut.RatioHalf = im.RatioHalf;
                            ut.RatioQuarter = im.RatioQuarter;
                            ut.RatioEighth = im.RatioEighth;
                            ut.RatioSixteen = im.RatioSixteen;
                            ut.Durations = (im.Durations != null && im.Durations.Count > 0) ? new List<Cadence>(im.Durations) : null;
                        }


                        // Math Motif
                        Debug.Log("Math Motif Count:" + slSong.Motifs.Count);
                        foreach (ImMathMotif im in slSong.Motifs)
                        {
                            //Debug.Log(" " + im.Name);
                            UtMathMotif ut = (UtMathMotif)InfinityMusic.Instantiate(InfinityMusic.instance.TemplateMathMotif, Vector3.zero, Quaternion.identity);
                            ut.transform.parent = InfinityMusic.instance.transform;

                            // Common
                            LoadCommon(im, ut);

                            // Specific
                            ut.MeasureCount = im.MeasureCount;
                            ut.OctaveMin = im.OctaveMin;
                            ut.OctaveMax = im.OctaveMax;
                            ut.ScaleIndex = im.ScaleIndex;
                            ut.PatchIndex = im.PatchIndex;
                            ut.DrumKit = im.DrumKit;
                            ut.SelectedAlgo = im.SelectedAlgo;
                            ut.StepInScale = im.StepInScale;
                            ut.RotationSpeed = im.RotationSpeed;
                            ut.Accentuation = im.Accentuation;
                            ut.Velocity = im.Velocity;
                            ut.RepeatRate = im.RepeatRate;
                            if (im.IdCadence >= 0)
                            {
                                UtCadence[] cadences = GameObject.FindObjectsOfType<UtCadence>();
                                foreach (UtCadence cadence in cadences)
                                {
                                    if (cadence.UtId==im.IdCadence)
                                    {
                                        ut.CurrentCadence = cadence;
                                        break;
                                    }
                                }
                            }
                            ut.Notes = (im.Notes != null && im.Notes.Count > 0) ? new List<MathMotifNote>(im.Notes) : null;
                        }

                        //Debug.Log("Chorder Count:" + slSong.Chorders.Count);
                        //foreach (ImChorder im in slSong.Chorders)
                        //{
                        //    Debug.Log(" " + im.ImName + " " + im.ImRadiusEffect);
                        //    UtChorder ut = (UtChorder)Instantiate(UtGlobal.instance.utChorder, im.Position, Quaternion.identity);

                        //    // Common
                        //    LoadCommon(im, ut);

                        //    // Specific
                        //    ut.Trigger = im.Trigger;
                        //    ut.StepCount = im.StepCount;
                        //    ut.ChordProgression = im.ChordProgression;
                        //}


                        //// Midi Motif
                        //Debug.Log("Midi Motif Count:" + slSong.Midis.Count);
                        //foreach (ImMidiMotif im in slSong.Midis)
                        //{
                        //    Debug.Log(" " + im.ImName + " " + im.ImRadiusEffect);
                        //    UtMidiMotif ut = (UtMidiMotif)Instantiate(UtGlobal.instance.utMidiMotif, im.Position, Quaternion.identity);

                        //    // Common
                        //    LoadCommon(im, ut);

                        //    // Specific
                        //    ut.MidiName = im.MidiName;
                        //    ut.Transpose = im.Transpose;
                        //    ut.StartPlay = im.StartPlay;
                        //    ut.EndPlay = im.EndPlay;
                        //    ut.DeltaTicksPerQuarterNote = im.DeltaTicksPerQuarterNote;
                        //    ut.NumberBeatsMeasure = im.NumberBeatsMeasure;
                        //    ut.NumberQuarterBeat = im.NumberQuarterBeat;
                        //    ut.SelectedMode = im.SelectedMode;
                        //    ut.StepPlay = im.StepPlay;
                        //    //ut.MidiMeasures = im.Measures;
                        //    ut.Tracks = im.Tracks;
                        //    ut.ImportTracksToMeasure();
                        //}

                        ////
                        //// Modifier
                        ////
                        //Debug.Log("Modifier Count:" + slSong.Modifiers.Count);
                        //foreach (ImModifier im in slSong.Modifiers)
                        //{
                        //    Debug.Log(">>> Create '" + im.ImName + "'");
                        //    UtModifier ut = (UtModifier)Instantiate(UtGlobal.instance.utModifier, im.Position, Quaternion.identity);
                        //    //Debug.Log("<<< Create '" + im.ImName + "'");

                        //    // Common
                        //    LoadCommon(im, ut);

                        //    // Specific
                        //    ut.MeasureCount = im.MeasureCount;
                        //    ut.SelectedMode = im.SelectedMode;

                        //    //if (im.IndexProperties < 5)
                        //    //{
                        //    //    ut.IndexApplyTo = 1;
                        //    //    ut.IndexProperties = im.IndexProperties;

                        //    //}
                        //    //else
                        //    //{
                        //    //    ut.IndexApplyTo = 0;
                        //    //    ut.IndexProperties = 0;
                        //    //}
                        //    ut.IndexApplyTo = im.IndexApplyTo;
                        //    ut.IndexProperties = im.IndexProperties;

                        //    //Debug.Log("   im.MinSelected " + im.MinSelected);
                        //    //Debug.Log("   im.MaxSelected " + im.MaxSelected);
                        //    ut.FromSelected = im.MinSelected;
                        //    ut.ToSelected = im.MaxSelected;
                        //    ut.Step = im.Step;
                        //}

                        ////
                        //// Drum
                        ////
                        //Debug.Log("Drum Count:" + slSong.Drums.Count);
                        //foreach (ImDrum im in slSong.Drums)
                        //{
                        //    Debug.Log(">>> Create '" + im.ImName + "'");
                        //    UtDrum ut = (UtDrum)Instantiate(UtGlobal.instance.utDrum, im.Position, Quaternion.identity);
                        //    //Debug.Log("<<< Create '" + im.ImName + "'");

                        //    // Common
                        //    LoadCommon(im, ut);

                        //    // Specific
                        //    ut.Tracks = im.Tracks;
                        //    ut.RandomLevel = im.RandomLevel;
                        //}

                        ////
                        //// Activator
                        ////
                        //Debug.Log("Activator Count:" + slSong.Activators.Count);
                        //foreach (ImActivator im in slSong.Activators)
                        //{
                        //    Debug.Log(">>> Create '" + im.ImName + "'");
                        //    UtActivator ut = (UtActivator)Instantiate(UtGlobal.instance.utActivator, im.Position, Quaternion.identity);
                        //    //Debug.Log("<<< Create '" + im.ImName + "'");

                        //    // Common
                        //    LoadCommon(im, ut);

                        //    // Specific
                        //    ut.MeasureCount = im.MeasureCount;
                        //    ut.MeasureStart = im.MeasureStart;
                        //    ut.ActivateBeforeStart = im.ActivateBeforeStart;
                        //    ut.ActivateAfterLoop = im.ActivateAfterLoop;
                        //    ut.LoopCount = im.LoopCount;
                        //    ut.Activates = im.Activates;
                        //}


                        InfinityMusic.instance.MeasureLength = slSong.MeasureLength;
                        InfinityMusic.instance.QuarterPerMinute = slSong.QuarterPerMinute;
                        InfinityMusic.instance.MaxMeasure = slSong.MaxMeasure;
                        if (InfinityMusic.instance.MaxMeasure <= 0) InfinityMusic.instance.MaxMeasure = 1000;
                        InfinityMusic.instance.SongName = slSong.SongName;
                        InfinityMusic.instance.Description = slSong.Description;
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            Debug.Log("<<< Load");

        }

        private static void LoadCommon(ImComponent im, UtComponent ut)
        {
            ut.UtId = im.Id;
            ut.name = im.Name;
            ut.UtIsEnabled = im.IsEnabled;
        }

        private static void SaveCommon(UtComponent ut, ImComponent im)
        {
            im.Id = ut.UtId;
            im.Name = ut.name;
            im.IsEnabled = ut.UtIsEnabled;
        }

        private void SaveXML(string filepath)
        {
            var serializer = new XmlSerializer(typeof(SaveLoad));
            var encoding = Encoding.GetEncoding("UTF-8");
            using (StreamWriter stream = new StreamWriter(filepath, false, encoding))
            {
                serializer.Serialize(stream, this);
            }
        }

        private static SaveLoad LoadXML(string Filepath)
        {
            SaveLoad loaded = null;

            if (File.Exists(Filepath))
            {
                var serializer = new XmlSerializer(typeof(SaveLoad));
                using (var stream = new FileStream(Filepath, FileMode.Open))
                {
                    loaded = serializer.Deserialize(stream) as SaveLoad;
                }
            }
            else
                Debug.LogWarning("Not found " + Filepath);

            return loaded;
        }
    }

    public class ImComponent
    {
        public int Id;
        public string Name;
        public bool IsEnabled;
        public ImComponent() { }
    }

    public class ImMathMotif : ImComponent
    {
        public int MeasureCount;
        public int OctaveMin;
        public int OctaveMax;
        public int ScaleIndex;
        public int PatchIndex;
        public bool DrumKit;
        public Mode SelectedAlgo;
        public int StepInScale;
        public int RotationSpeed;
        public int Accentuation;
        public int Velocity;
        public int RepeatRate;
        public int IdCadence;
        public List<MathMotifNote> Notes;
    }

    public class ImChorder : ImComponent
    {
        //public  UtChorder.TriggerChord Trigger;
        //public int StepCount;
        //public List<Chord> ChordProgression;
    }

    public class ImMidiMotif : ImComponent
    {
        //public string MidiName;
        //public int Transpose = 1;
        //public int StartPlay;
        //public int EndPlay;
        //public int DeltaTicksPerQuarterNote;
        //public int NumberBeatsMeasure;
        //public int NumberQuarterBeat;
        //public UtMidiMotif.Mode SelectedMode = UtMidiMotif.Mode.Normal;
        //public int StepPlay = 1;
        ////public List<MidiMeasure> Measures;
        //public List<MidiTrack> Tracks;
    }

    public class ImCadence : ImComponent
    {
        public int MeasureCount;
        public int PctSilence;
        public int RatioWhole;
        public int RatioHalf;
        public int RatioQuarter;
        public int RatioEighth;
        public int RatioSixteen;
        public List<Cadence> Durations;
    }

    public class ImModifier : ImComponent
    {
        //public int MeasureCount;
        //public UtModifier.Mode SelectedMode = UtModifier.Mode.Up;
        //public int IndexApplyTo;
        //public int IndexProperties;
        //public int MinSelected;
        //public int MaxSelected;
        //public int Step;
    }

    public class ImDrum : ImComponent
    {
        //public int RandomLevel;
        //public List<DrumTrack> Tracks;
    }
    public class ImActivator : ImComponent
    {
        //public int MeasureCount;
        //public int MeasureStart;
        //public bool ActivateBeforeStart;
        //public bool ActivateAfterLoop;
        //public int LoopCount;
        //public List<bool> Activates;

    }
}