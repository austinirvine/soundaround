// 2001558366756129 
// qfaUxnsvKRhwFfVlTvWGyN2IZD4
// 31c3b02601922cba629745e75fd4c1e7
// 5735e6de5e1de44fcb265a9cc917c518
// CBAEURHCWSKTDO

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MidiPlayerTK;
using System.Reflection;

namespace InfinityMusic
{
    public class InfinityMusic : MonoBehaviour
    {
        static public InfinityMusic instance;
        public bool IsPlaying = true;

        public UtMathMotif TemplateMathMotif;
        //public UtChorder utChorder;
        //public UtMidiMotif utMidiMotif;
        public UtCadence TemplateCadence;
        //public UtModifier utModifier;
        //public UtDrum utDrum;
        //public UtActivator utActivator;

        /// <summary>
        /// Nombre de quarter par mesure
        /// </summary>
        [Range(1, 16)]
        public int MeasureLength = 4;
        [Range(1, 9999)]
        public int MaxMeasure = 9999;
        [Range(1, 500)]
        public int QuarterPerMinute = 80;

        public string SongName;
        public string Description;
        /// <summary>
        /// Duree d'une double croche, depend du nombre de temps par mesure
        /// </summary>
        public double SixteenthDurationMs;

        /// <summary>
        /// Delay of a tick in second
        /// </summary>
        [Range(0.0001f, 0.1f)]
        public float TickDurationSec = 0.0020f;

        /// <summary>
        /// Count of sixt per measure. 
        /// MeasureLengthValue=4 --> count=16  
        /// MeasureLengthValue=3 --> count=12  
        /// MeasureLengthValue=2 --> count=8
        /// </summary>
        public int CountSixteenthMeasure;

        /// <summary>
        /// Position de la mesure dans la partition
        /// </summary>
        public int IndexMeasure = 0;

        /// <summary>
        /// Position de la noire dans la mesure 
        /// </summary>
        public int IndexQuarterMeasure = 0;

        /// <summary>
        /// Position de la double croche dans la mesure
        /// </summary>
        public int IndexSixteenthMeasure = 0;

        /// <summary>
        /// 4 Q64 dans une double croche
        /// </summary>
        public int IndexTicks = 0;

        /// <summary>
        /// Real time play from start play
        /// </summary>
        public double TotalTimePlayMs = 0;


        /// <summary>
        /// Time from start of the measure
        /// </summary>
        public double MeasureTimePlayMs = 0;
        public double LastMeasureTimePlayMs = 0;

        /// <summary>
        /// Time from start of the sixt
        /// </summary>
        public double SixtTimePlayMs = 0;
        public bool PlayASixt = true;

        /// <summary>
        /// Real time length of the Q64
        /// </summary>
        public double TickTimePlayMs = 0;

        private DateTime LastDateTick;
        private DateTime DateStart;

        // MPTK component able to play a stream of notes generated
        public MidiStreamPlayer MidiStreamPlayer;

        static public bool SongIsModified = false;
        Vector2 scrollListSong = Vector2.zero;
        static public List<string> SoundFonts = null;
        public string GuiMessage = null;

        public string SelectedSongName;

        // Manage skin
        public GUISkin customSkin;
        public CustomStyle myStyle;

        void Awake()
        {
            //Debug.Log("Awake InfinityMusic");
            HelperNoteLabel.Init();

            // Event set in Inspector
            //if (MidiPlayerGlobal.OnEventPresetLoaded != null)
            //    MidiPlayerGlobal.OnEventPresetLoaded.AddListener(() => EndLoadingSF());

            instance = this;
            LastDateTick = DateTime.Now;
            ScaleDefinition.Init();
            HelperNoteLabel.Init();
            HelperNoteRatio.Init();
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
        void Start()
        {

            if (MidiStreamPlayer == null)
            {
                Debug.LogWarning("No MidiStreamPlayer defined for UtGlobal. Set up in UtGlobal Inspector.");
            }
            else
            {
                Play();
                StartCoroutine(Trigger());
            }
        }



        public void OnGUI()
        {
            try
            {
                if (customSkin != null) GUI.skin = customSkin;
                if (myStyle == null) myStyle = new CustomStyle();


                GUI.changed = false;
                GUI.color = Color.white;
                //GUIStyle styleBold;
                //styleBold = new GUIStyle("Label")
                //{
                //    fontSize = 12,
                //    fontStyle = FontStyle.Bold
                //};

                float widthSelectSong = 200;
                float buttonHeight = 30;
                float buttonWidth = 200;
                float listHeight = 150;
                float maxwidth = Screen.width / 2;
                if (maxwidth < 300) maxwidth = 300;


                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    GUILayout.BeginArea(new Rect(25, 30, maxwidth, 600));

                    GUILayout.Label("InfinityMusic - Music generated by an algo", myStyle.TitleLabel1);
                    if (GUILayout.Button(new GUIContent("Return to menu", ""))) GoMainMenu.Go();
                    GUILayout.Space(20);

                    GUISelectSoundFont.Display(myStyle);

                    GUILayout.Space(20);
                    GUILayout.Label(
                        string.Format("Measure {0,3:000}.{1,2:00}.{2,2:00}", InfinityMusic.instance.IndexMeasure + 1, InfinityMusic.instance.IndexQuarterMeasure + 1, InfinityMusic.instance.IndexSixteenthMeasure + 1),
                        myStyle.TitleLabel2);
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal();

                    SelectSong(widthSelectSong, listHeight);

                    GUILayout.BeginVertical();

                    if (GUILayout.Button("New", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                    {
                        GuiMessage = null;
                        InfinityMusic.UtNewSong();
                        instance.SongName = "";
                    }

                    if (GUILayout.Button("Open", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                    {
                        if (string.IsNullOrEmpty(SelectedSongName))
                        {
                            GuiMessage = "Select a song name";
                        }
                        else
                        {
                            GuiMessage = null;
                            string path = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToSong);
                            string filepath = Path.Combine(path, SelectedSongName + "." + MidiPlayerGlobal.ExtensionSong);
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                instance.SongName = SelectedSongName;
                                InfinityMusic.UtNewSong();
                                SaveLoad.UtLoad(filepath);
                            }
                        }
                    }

                    instance.SongName = GUILayout.TextField(instance.SongName, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight));

                    if (GUILayout.Button("Save", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                    {
                        if (string.IsNullOrEmpty(instance.SongName))
                        {
                            GuiMessage = "Set a name for this song";
                        }
                        else
                        {
                            GuiMessage = null;

                            string path = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToSong);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            string filepath = Path.Combine(path, instance.SongName + "." + MidiPlayerGlobal.ExtensionSong);
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                SaveLoad.UtSave(filepath);
                            }
                        }
                    }
                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical();
                    if (!string.IsNullOrEmpty(GuiMessage))
                    {
                        GUI.color = Color.red;
                        GUILayout.Label(GuiMessage, myStyle.TitleLabel2);
                        GUI.color = Color.white;
                    }
                    GUILayout.Label("Go to your Hierarchy and select InfinityMusic:", myStyle.TitleLabel2);
                    GUILayout.Label("   - Add MathMotif or Cadence components by clicking on button.", myStyle.TitleLabel2);
                    GUILayout.Label("   - Select created components MathMotif or Cadence under InfinityMusic.", myStyle.TitleLabel2);
                    GUILayout.Label("   - Change parameters in inspector to change melody.", myStyle.TitleLabel2);
                    GUILayout.EndVertical();

                }
                else
                {
                    GUILayout.Label(new GUIContent("SoundFont: no soundfont selected", "Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f"));
                }
                GUILayout.EndArea();

            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void SelectSong(float width, float height)
        {
            try
            {
                string folder = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToSong);
                string[] fileEntries;
                if (Directory.Exists(folder))
                    fileEntries = Directory.GetFiles(folder, "*.txt", SearchOption.AllDirectories);
                else
                    fileEntries = new string[0];

                scrollListSong = GUILayout.BeginScrollView(scrollListSong, false, false, myStyle.HScroll, myStyle.VScroll, myStyle.BackgWindow, GUILayout.Width(200), GUILayout.Height(200));
                foreach (string s in fileEntries)
                {
                    string songname = Path.GetFileNameWithoutExtension(s);
                    GUIStyle styleBt = myStyle.BtStandard;
                    if (SelectedSongName == songname) styleBt = myStyle.BtSelected;

                    if (GUILayout.Button(songname, styleBt))
                    {
                        SelectedSongName = Path.GetFileNameWithoutExtension(s);
                    }
                }
                GUILayout.EndScrollView();
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        private void Stop()
        {
            IsPlaying = false;
        }

        private void Play()
        {
            IsPlaying = true;
            LastDateTick = DateTime.Now;
        }

        /// <summary>
        /// Loop every sisteenth duration to calculate music
        /// </summary>
        /// <returns></returns>
        IEnumerator Trigger()
        {
            DateTime now = DateTime.Now;
            DateStart = now;
            LastDateTick = now;

            while (true)
            {
                /// <summary>
                /// Duree d'un quarter (noire) en seconde, ne depend pas du nombre de temps par mesure
                /// </summary>
                double quarterDurationMs;

                quarterDurationMs = 60000d / (double)QuarterPerMinute;
                CountSixteenthMeasure = MeasureLength * 4;

                SixteenthDurationMs = quarterDurationMs / 4d;
                //DebugPosition();

                if (!IsPlaying)
                {
                    yield return new WaitForSeconds(TickDurationSec);
                }
                else
                {
                    lock (this)
                    {
                        // Action at each start of measure
                        if (MeasureTimePlayMs == 0d)
                        {

                        }

                        // Action at each sixt
                        if (PlayASixt)
                        {
                            List<MPTKNote> notes = new List<MPTKNote>();
                            UtMathMotif[] uts = GameObject.FindObjectsOfType<UtMathMotif>();
                            if (uts != null && uts.Length > 0)
                            {
                                foreach (UtMathMotif utmotif in uts)
                                {
                                    if (utmotif.IsEnabled)
                                    {
                                        MathMotifNote motifNote = utmotif.Calculate();
                                        if (motifNote != null)
                                        {
                                            notes.Add(new MPTKNote()
                                            {
                                                Note = motifNote.Note,
                                                Duration = motifNote.Duration,
                                                Velocity = motifNote.Velocity,
                                                Patch = motifNote.Patch,
                                                Drum = motifNote.Drum,
                                            });
                                        }
                                    }
                                }

                                MidiStreamPlayer.MPTK_Play(notes);
                            }
                        }
                    }

                    yield return new WaitForSeconds(TickDurationSec);

                    lock (this)
                    {
                        now = DateTime.Now;

                        TotalTimePlayMs = System.Convert.ToInt64((now - DateStart).TotalMilliseconds);

                        // Delay since last tick
                        TickTimePlayMs = (now - LastDateTick).TotalMilliseconds;

                        // if paused reset timeplay
                        if (TickTimePlayMs > 100) TickTimePlayMs = 0;
                        LastDateTick = now;

                        // Time since last sixt
                        SixtTimePlayMs += TickTimePlayMs;

                        // Time since last measure
                        MeasureTimePlayMs += TickTimePlayMs;

                        IndexTicks++;
                        PlayASixt = false;

                        // Check if need to play a sixt
                        if (SixtTimePlayMs >= SixteenthDurationMs)
                        {
                            PlayASixt = true;
                            IndexSixteenthMeasure++;
                            SixtTimePlayMs = SixtTimePlayMs - SixteenthDurationMs;
                        }

                        // Check if start of measure
                        if (IndexSixteenthMeasure >= CountSixteenthMeasure)
                        {
                            IndexTicks = 0;
                            IndexSixteenthMeasure = 0;
                            //Debug.Log("MeasureTimePlayMs:" + MeasureTimePlayMs);
                            LastMeasureTimePlayMs = MeasureTimePlayMs;
                            MeasureTimePlayMs = 0;// -2*SixteenthDurationMs;
                            IndexMeasure++;
                            if (IndexMeasure >= MaxMeasure)
                                IndexMeasure = 0;
                        }
                        IndexQuarterMeasure = IndexSixteenthMeasure / 4;
                    }
                }
            }
        }

        static public void UtNewSong()
        {
            InfinityMusic.instance.Description = "";
            UtComponent[] components = GameObject.FindObjectsOfType<UtComponent>();
            foreach (UtComponent ut in components)
            {
                //Debug.Log("destroy " + ut.name);
                DestroyImmediate(ut.gameObject);
            }
        }

        private void DebugPosition()
        {
            string text = string.Format("M={0,3:000} MeasureTimePlayMs={1,4:0000} LastMeasureTimePlayMs={2,6:000000} Quarter={3,1} / Sixte={4,2:00} SixtTimePlayMs={5,2:00} TickTimePlayMs={6,2:00}",
                IndexMeasure, MeasureTimePlayMs, LastMeasureTimePlayMs,
                IndexQuarterMeasure, //3
                IndexSixteenthMeasure, //4
                SixtTimePlayMs,
                TickTimePlayMs); // 6

            Debug.Log(text);
        }


#if UNITY_EDITOR
        [InitializeOnLoad]
        public static class SaveBeforePlay
        {
            static SaveBeforePlay()
            {
                EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
            }

            private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
            {
                if (obj == PlayModeStateChange.EnteredPlayMode)
                {
                    //Debug.Log("SaveOpenScenes");
                    //EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();
                }
            }
        }
#endif
    }
}