using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Events;
using System;
using System.Collections.ObjectModel;

namespace MidiPlayerTK
{
    /// <summary>
    /// Singleton class to manage all features of MPTK
    /// </summary>
    public class MidiPlayerGlobal : MonoBehaviour
    {
        private static MidiPlayerGlobal instance;
        private MidiPlayerGlobal() { }
        public static MidiPlayerGlobal Instance { get { return instance; } }

        public const string SoundfontsDB = "SoundfontsDB";
        public const string MidiFilesDB = "MidiDB";
        public const string SongFilesDB = "SongDB";
        public const string PathToResources = "MidiPlayer/Resources/";
        public const string PathToSoundfonts = PathToResources + SoundfontsDB;
        public const string ExtensionMidiFile = ".bytes";
        public const string ExtensionSoundFileFile = ".txt";
        public const string ExtensionSong = "txt";
        public const string PathToMidiFile = PathToResources + MidiFilesDB;
        public const string FilenameMidiSet = "MidiSet";
        public const string PathToMidiSet = PathToResources + FilenameMidiSet + ".txt";
        public const string PathToSong = PathToResources + SongFilesDB;
        public const string PathSF2 = "SoundFont";
        public const string PathToWave = "wave";

        /// <summary>
        /// True if soundfont is loaded
        /// </summary>
        public static bool SoundFontLoaded = false;
        private static TimeSpan timeToLoadSoundFont = TimeSpan.MaxValue;
        public static TimeSpan MPTK_TimeToLoadSoundFont
        {
            get
            {
                return timeToLoadSoundFont;
            }
        }

        private static TimeSpan timeToLoadWave = TimeSpan.MaxValue;
        public static TimeSpan MPTK_TimeToLoadWave
        {
            get
            {
                return timeToLoadWave;
            }
        }

        public static int MPTK_CountPresetLoaded;
        public static int MPTK_CountWaveLoaded;

        /// <summary>
        /// Current Simplified SoundFont loaded
        /// </summary>
        public static ImSoundFont ImSFCurrent;

        /// <summary>
        /// Event triggered when Soundfont is loaded
        /// </summary>
        public UnityEvent InstanceOnEventPresetLoaded = new UnityEvent();
        public static UnityEvent OnEventPresetLoaded
        {
            get { return Instance != null ? Instance.InstanceOnEventPresetLoaded : null; }
            set { Instance.InstanceOnEventPresetLoaded = value; }
        }

        /// <summary>
        /// Current Midi Set loaded
        /// </summary>
        public static MidiSet CurrentMidiSet;

        private static string WavePath;
        private static AudioListener AudioListener;
        private static bool Initialized = false;

        void Awake()
        {
            //Debug.Log("Awake MidiPlayerGlobal");
            if (instance != null && instance != this)
                Destroy(gameObject);    // remove previous instance
            else
            {
                //DontDestroyOnLoad(gameObject);
                instance = this;
                instance.StartCoroutine(instance.InitThread());
            }
        }

        /// <summary>
        /// List of Soundfont(s) available
        /// </summary>
        public static List<string> MPTK_ListSoundFont
        {
            get
            {
                if (CurrentMidiSet != null && CurrentMidiSet.SoundFonts != null)
                {
                    List<string> sfNames = new List<string>();
                    foreach (SoundFontInfo sfi in CurrentMidiSet.SoundFonts)
                        sfNames.Add(sfi.Name);
                    return sfNames;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// List of midi(s) available
        /// </summary>
        public static ReadOnlyCollection<string> MPTK_ListMidi
        {
            get
            {
                if (CurrentMidiSet != null && CurrentMidiSet.MidiFiles != null)
                    return CurrentMidiSet.MidiFiles.AsReadOnly();
                else
                    return null;
            }
        }

        private static ReadOnlyCollection<string> listPreset;
        /// <summary>
        /// Get the list of presets available
        /// </summary>
        public static ReadOnlyCollection<string> MPTK_ListPreset { get { return listPreset; } }


        private static ReadOnlyCollection<string> listDrum;
        /// <summary>
        /// Get the list of presets available
        /// </summary>
        public static ReadOnlyCollection<string> MPTK_ListDrum { get { return listDrum; } }

        /// <summary>
        /// Call by the first MidiPlayer awake
        /// </summary>
        //public static void Init()
        //{
        //    Instance.StartCoroutine(Instance.InitThread());
        //}

        /// <summary>
        /// Call by the first MidiPlayer awake
        /// </summary>
        private IEnumerator InitThread()
        {
            if (!Initialized)
            {
                //Debug.Log("MidiPlayerGlobal InitThread");
                SoundFontLoaded = false;
                Initialized = true;
                ImSFCurrent = null;

                try
                {
                    AudioListener = Component.FindObjectOfType<AudioListener>();
                    if (AudioListener == null)
                    {
                        Debug.LogWarning("No audio listener found. Add one and only one AudioListener component to your hierarchy.");
                        //return;
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    AudioListener[] listeners = Component.FindObjectsOfType<AudioListener>();
                    if (listeners != null && listeners.Length > 1)
                    {
                        Debug.LogWarning("More than one audio listener found. Some unexpected behaviors could happen.");
                    }
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                try
                {
                    LoadMidiSetFromRsc();
                    DicAudioClip.Init();
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }

                if (CurrentMidiSet == null)
                {
                    Debug.LogWarning("No Midi defined, go to menu 'Tools/MPTK - Midi File Setup' or alt-m");
                    yield return 0;
                }
                else if (CurrentMidiSet.ActiveSounFontInfo == null)
                {
                    Debug.LogWarning("No Active SoundFont found. Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f");
                    yield return 0;
                }

                LoadCurrentSF();
                //Debug.Log("");

                if (ImSFCurrent != null)
                    SoundFontLoaded = true;
            }
        }

        /// <summary>
        /// Change current Soundfont on fly
        /// </summary>
        /// <param name="name"></param>
        public static void MPTK_SelectSoundFont(string name)
        {
            if (Application.isPlaying)
                Instance.StartCoroutine(SelectSoundFontThread(name));
            else
                SelectSoundFont(name);
        }

        private static IEnumerator SelectSoundFontThread(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name.Contains(name));
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found " + name);
                    yield return 0;
                }
            }
            // Load selected soundfont
            yield return Instance.StartCoroutine(LoadSoundFontThread());
        }

        private static void SelectSoundFont(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                int index = CurrentMidiSet.SoundFonts.FindIndex(s => s.Name.Contains(name));
                if (index >= 0)
                {
                    MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(index);
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                    // Load selected soundfont
                    LoadSoundFont();
                }
                else
                {
                    Debug.LogWarning("SoundFont not found " + name);
                }
            }
        }

        private static IEnumerator LoadSoundFontThread()
        {
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                Debug.Log("Load MidiPlayerGlobal.ImSFCurrent: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                Debug.Log("Load CurrentMidiSet.ActiveSounFontInfo: " + CurrentMidiSet.ActiveSounFontInfo.Name);

                MidiPlayerGlobal.SoundFontLoaded = false;
                MidiPlayer[] midiplayers = null;
                if (Application.isPlaying)
                {
                    midiplayers = GameObject.FindObjectsOfType<MidiPlayer>();
                    if (midiplayers != null)
                        foreach (MidiPlayer mp in midiplayers)
                        {
                            //if (mp is MidiFilePlayer) ((MidiFilePlayer)mp).MPTK_Pause();
                            yield return Instance.StartCoroutine(mp.MPTK_ClearAllSound(true));
                        }
                    //return;
                    DicAudioClip.Init();
                }
                MidiPlayerGlobal.LoadCurrentSF();
                Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
                Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
                MidiPlayerGlobal.SoundFontLoaded = true;

                if (Application.isPlaying)
                {
                    if (midiplayers != null)
                        foreach (MidiPlayer mp in midiplayers)
                        {
                            if (mp is MidiFilePlayer) ((MidiFilePlayer)mp).MPTK_ReSyncTime();
                        }
                }
            }
        }


        private static void LoadSoundFont()
        {
            if (MidiPlayerGlobal.ImSFCurrent != null)
            {
                Debug.Log("Load MidiPlayerGlobal.ImSFCurrent: " + MidiPlayerGlobal.ImSFCurrent.SoundFontName);
                Debug.Log("Load CurrentMidiSet.ActiveSounFontInfo: " + CurrentMidiSet.ActiveSounFontInfo.Name);

                MidiPlayerGlobal.SoundFontLoaded = false;
                MidiPlayerGlobal.LoadCurrentSF();
                Debug.Log("Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
                if (Application.isPlaying)
                    Debug.Log("Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
                MidiPlayerGlobal.SoundFontLoaded = true;
            }
        }

        public static void LoadCurrentSF()
        {
            // Load simplfied soundfont
            try
            {
                DateTime start = DateTime.Now;
                if (CurrentMidiSet == null)
                {
                    Debug.LogWarning("No SoundFont defined, go to Unity menu Tools to add a Soundfont");
                }
                else
                {
                    SoundFontInfo sfi = CurrentMidiSet.ActiveSounFontInfo;
                    if (sfi == null)
                        Debug.LogWarning("No SoundFont defined, go to Unity menu Tools to add a Soundfont");
                    else
                    {
                        // Path to the soundfonts directory for this SF, start from resource folder
                        string pathToImSF = Path.Combine(SoundfontsDB + "/", sfi.Name);
                        // Path to the soundfonts file for this SF
                        TextAsset sf = Resources.Load<TextAsset>(Path.Combine(pathToImSF + "/", sfi.Name));
                        if (sf == null)
                            Debug.LogWarning("No SoundFont found " + pathToImSF);
                        else
                        {
                            WavePath = Path.Combine(pathToImSF + "/", PathToWave);
                            // Load all presets defined in the XML sf
                            ImSFCurrent = ImSoundFont.Load(sf.text);
                            timeToLoadSoundFont = DateTime.Now - start;
                            BuildPresetList();
                            BuildDrumList();
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            if (ImSFCurrent == null)
            {
                Debug.LogWarning("SoundFont not loaded.");
                return;
            }

            // Load samples only in run mode
            if (Application.isPlaying)
            {
                try
                {
                    MPTK_CountWaveLoaded = 0;
                    MPTK_CountPresetLoaded = 0;
                    DateTime start = DateTime.Now;
                    for (int ibank = 0; ibank < ImSFCurrent.Banks.Length; ibank++)
                    {
                        if (ImSFCurrent.Banks[ibank] != null)
                        {
                            for (int ipreset = 0; ipreset < ImSFCurrent.Banks[ibank].Presets.Length; ipreset++)
                            {
                                MPTK_CountPresetLoaded++;
                                if (ImSFCurrent.Banks[ibank].Presets[ipreset] != null)
                                {
                                    LoadSamples(ibank, ipreset);
                                }
                            }
                        }
                    }
                    timeToLoadWave = DateTime.Now - start;
                }
                catch (System.Exception ex)
                {
                    MidiPlayerGlobal.ErrorDetail(ex);
                }
            }
            if (OnEventPresetLoaded != null) OnEventPresetLoaded.Invoke();
        }

        /// <summary>
        /// Build list of presets found in the SoundFont
        /// </summary>
        static public void BuildPresetList()
        {
            List<string> presets = new List<string>();
            try
            {
                //Debug.Log(">>> Load Preset - b:" + ibank + " p:" + ipatch);
                if (ImSFCurrent != null && CurrentMidiSet != null)
                {
                    if (ImSFCurrent.DefaultBankNumber >= 0 && ImSFCurrent.DefaultBankNumber < ImSFCurrent.Banks.Length)
                    {
                        int ibank = ImSFCurrent.DefaultBankNumber;
                        if (ImSFCurrent.Banks[ibank] != null)
                        {
                            for (int ipreset = 0; ipreset < ImSFCurrent.Banks[ibank].Presets.Length; ipreset++)
                            {
                                string presetName;
                                if (ImSFCurrent.Banks[ibank].Presets[ipreset] != null)
                                    presetName = ImSFCurrent.Banks[ibank].Presets[ipreset].Name;
                                else
                                    presetName = "";
                                presets.Add(presetName);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("BuildPresetList: Bank don't exists.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("BuildPresetList: No bank defined for instrument.");
                    }
                }
                else
                {
                    Debug.LogWarning("BuildPresetList: No MidiSet defined, go to menu 'Tools/MPTK - Midi File Setup' or alt-m");
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            listPreset = presets.AsReadOnly();
        }

        static public void BuildDrumList()
        {
            List<string> drums = new List<string>();
            try
            {
                //Debug.Log(">>> Load Preset - b:" + ibank + " p:" + ipatch);
                if (ImSFCurrent != null && CurrentMidiSet != null)
                {
                    if (ImSFCurrent.DrumKitBankNumber >= 0 && ImSFCurrent.DrumKitBankNumber < ImSFCurrent.Banks.Length)
                    {
                        int ibank = ImSFCurrent.DrumKitBankNumber;
                        if (ImSFCurrent.Banks[ibank] != null && ImSFCurrent.Banks[ibank].Presets[0] != null)
                        {
                            for (int key = 0; key < 127; key++)
                            {
                                string waveName;
                                ImSample sample = GetImSample(ibank, 0, key, 127);
                                if (sample != null)
                                    waveName = Path.GetFileNameWithoutExtension(sample.WaveFile);
                                else
                                    waveName = "";
                                drums.Add(waveName);
                            }
                        }
                        else
                        {
                            //Debug.LogWarning("Bank don't exists.");
                        }
                    }
                    else
                    {
                        //Debug.LogWarning("No bank defined for instrument.");
                    }
                }
                else
                {
                    Debug.LogWarning("BuildDrumList: No MidiSet defined, go to menu 'Tools/MPTK - Midi File Setup' or alt-m");
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            listDrum = drums.AsReadOnly();
        }



        /// <summary>
        /// Find index of a Midi file which contains "name". 
        /// return -1 if not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static int MPTK_FindMidi(string name)
        {
            int index = -1;
            try
            {
                if (!string.IsNullOrEmpty(name))
                    if (CurrentMidiSet != null && CurrentMidiSet.MidiFiles != null)
                        index = CurrentMidiSet.MidiFiles.FindIndex(s => s.Contains(name));

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return index;
        }

        /// <summary>
        /// Calculate distance with the AudioListener
        /// </summary>
        /// <param name="trf"></param>
        /// <returns></returns>
        public static float MPTK_DistanceToListener(Transform trf)
        {
            float distance = 0f;
            try
            {
                if (AudioListener != null)
                {
                    distance = Vector3.Distance(AudioListener.transform.position, trf.position);
                    //Debug.Log("Camera:" + AudioListener.name + " " + distance);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }

            return distance;
        }


        /// <summary>
        /// Load samples associated to a patch
        /// </summary>
        /// <param name="ibank"></param>
        /// <param name="ipatch"></param>
        private static void LoadSamples(int ibank, int ipatch)
        {
            try
            {
                float start = Time.realtimeSinceStartup;
                //Debug.Log(">>> Load Preset - b:" + ibank + " p:" + ipatch);
                if (ImSFCurrent != null)
                {

                    ImPreset preset = ImSFCurrent.Banks[ibank].Presets[ipatch];
                    //Debug.Log("Loading Preset - " + index + " '" + preset.Name + "'");
                    // Load each sample associated with this preset in DicAudioClip
                    foreach (ImInstrument inst in preset.Instruments)
                    {
                        foreach (ImSample smpl in inst.Samples)
                        {
                            if (smpl.WaveFile != null)
                            {
                                if (!DicAudioClip.Exist(smpl.WaveFile))
                                {
                                    AudioClip ac = Resources.Load<AudioClip>(WavePath + "/" + Path.GetFileNameWithoutExtension(smpl.WaveFile));
                                    if (ac != null)
                                    {
                                        DicAudioClip.Add(smpl.WaveFile, ac);
                                        MPTK_CountWaveLoaded++;
                                    }
                                    //else Debug.LogWarning("Wave " + smpl.WaveFile + " not found");
                                }
                            }
                        }
                    }
                    //Debug.Log("--- Loaded Preset - " + ipatch + " '" + preset.Name + "' " + count + " samples loaded");
                }
                else Debug.Log("Presets not loaded ");
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Load setup MPTK from resource
        /// </summary>
        private static void LoadMidiSetFromRsc()
        {
            try
            {
                TextAsset sf = Resources.Load<TextAsset>(MidiPlayerGlobal.FilenameMidiSet);
                if (sf == null)
                    Debug.LogWarning("No Midi set found. Create a midi set from the menu Tools/Midi Player Toolkit");
                else
                {
                    //UnityEngine.Debug.Log(sf.text);
                    CurrentMidiSet = MidiSet.LoadRsc(sf.text);
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Return sample to play according preset, key and velocity
        /// </summary>
        /// <param name="idxbank"></param>
        /// <param name="idxpreset"></param>
        /// <param name="key"></param>
        /// <param name="vel"></param>
        /// <returns></returns>
        public static ImSample GetImSample(int idxbank, int idxpreset, int key, int vel)
        {
            try
            {
                if (ImSFCurrent.Banks[idxbank] != null)
                {
                    ImPreset preset = ImSFCurrent.Banks[idxbank].Presets[idxpreset];
                    if (preset != null && preset.Instruments != null)
                    {
                        foreach (ImInstrument instrument in preset.Instruments)
                        {
                            if (!instrument.HasKey || (key >= instrument.KeyStart && key <= instrument.KeyEnd))
                            {
                                if (!instrument.HasVel || (vel >= instrument.VelStart && vel <= instrument.VelEnd))
                                {
                                    foreach (ImSample sample in instrument.Samples)
                                    {
                                        if (sample.WaveFile != null)
                                        {
                                            if (!sample.HasKey || (key >= sample.KeyStart && key <= sample.KeyEnd))
                                            {
                                                if (!sample.HasVel || (vel >= sample.VelStart && vel <= sample.VelEnd))
                                                {
                                                    return sample;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return null;
        }

        public static List<ImSample> GetImMultiSample(int idxbank, int idxpreset, int key, int vel)
        {
            List<ImSample> samples = new List<ImSample>();
            try
            {
                if (ImSFCurrent.Banks[idxbank] != null)
                {
                    ImPreset preset = ImSFCurrent.Banks[idxbank].Presets[idxpreset];
                    if (preset != null && preset.Instruments != null)
                    {
                        foreach (ImInstrument instrument in preset.Instruments)
                        {
                            if (!instrument.HasKey || (key >= instrument.KeyStart && key <= instrument.KeyEnd))
                            {
                                if (!instrument.HasVel || (vel >= instrument.VelStart && vel <= instrument.VelEnd))
                                {
                                    foreach (ImSample sample in instrument.Samples)
                                    {
                                        if (sample.WaveFile != null)
                                        {
                                            if (!sample.HasKey || (key >= sample.KeyStart && key <= sample.KeyEnd))
                                            {
                                                if (!sample.HasVel || (vel >= sample.VelStart && vel <= sample.VelEnd))
                                                {
                                                    samples.Add(sample);
                                                    if (!MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.MultiWaves)
                                                        return samples;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return samples;
        }
        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");

            Debug.Log("List of presets available");
            int i = 0;
            foreach (string preset in MidiPlayerGlobal.MPTK_ListPreset)
                Debug.Log("   " + string.Format("[{0,3:000}] - {1}", i++, preset));
            i = 0;
            Debug.Log("List of drums available");
            foreach (string drum in MidiPlayerGlobal.MPTK_ListDrum)
                Debug.Log("   " + string.Format("[{0,3:000}] - {1}", i++, drum));

            Debug.Log("Load statistique");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Waves Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);

        }

        public static void ErrorDetail(System.Exception ex)
        {
            Debug.LogWarning("MPTK Error " + ex.Message);
            Debug.LogWarning("   " + ex.TargetSite ?? "");
            var st = new System.Diagnostics.StackTrace(ex, true);
            if (st != null)
            {
                var frames = st.GetFrames();
                if (frames != null)
                {
                    foreach (var frame in frames)
                    {
                        if (frame.GetFileLineNumber() < 1)
                            continue;
                        Debug.LogWarning("   " + frame.GetFileName() + " " + frame.GetMethod().Name + " " + frame.GetFileLineNumber());
                    }
                }
                else
                    Debug.LogWarning("   " + ex.StackTrace ?? "");
            }
            else
                Debug.LogWarning("   " + ex.StackTrace ?? "");
        }
    }
}
