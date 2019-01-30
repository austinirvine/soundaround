using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class TestMidiFilePlayerScripting : MonoBehaviour
    {
        // MPTK component able to play a Midi file
        public MidiFilePlayer midiFilePlayer;
        private float LastTimeChange;

        [Range(0.1f, 10f)]
        public float DelayTimeChange = 5;
        public bool IsRandomPosition = false;
        public bool IsRandomSpeed = false;
        public bool IsRandomTranspose = false;

        public static Color ButtonColor = new Color(.7f, .9f, .7f, 1f);
        public bool IsRandomPlay = false;

        // Manage skin
        public GUISkin customSkin;
        public CustomStyle myStyle;

        private void Awake()
        {
            // Set also by Unity editor
            if (MidiPlayerGlobal.OnEventPresetLoaded != null)
                MidiPlayerGlobal.OnEventPresetLoaded.AddListener(() => EndLoadingSF());
        }

        /// <summary>
        /// This call is defined from MidiPlayerGlobal event inspector. Run when SF is loaded.
        /// </summary>
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

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is started (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void StartPlay()
        {
            Debug.Log("Start Midi " + midiFilePlayer.MPTK_MidiName);
            midiFilePlayer.MPTK_Speed = 1f;
            midiFilePlayer.MPTK_Transpose = 0;
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi notes are available (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void ReadNotes(List<MidiNote> notes)
        {
            //Debug.Log("Notes : " + notes.Count);
        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is ended (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void EndPlay()
        {
            Debug.Log("End Midi " + midiFilePlayer.MPTK_MidiName);
        }

        // Use this for initialization
        void Start()
        {
            InitPlay();
        }

        public void InitPlay()
        {
            if (MidiPlayerGlobal.MPTK_ListMidi != null && MidiPlayerGlobal.MPTK_ListMidi.Count > 0)
            {
                // Random select for the Midi
                int index = UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);
                midiFilePlayer.MPTK_MidiIndex = index;
                midiFilePlayer.MPTK_Play();
            }
        }

        void OnGUI()
        {
            // Set custom Style. Good for background color 3E619800
            if (customSkin != null) GUI.skin = customSkin;
            if (myStyle == null) myStyle = new CustomStyle();

            float maxwidth = Screen.width / 2;
            if (maxwidth < 300) maxwidth = 300;

            if (midiFilePlayer != null)
            {
                GUILayout.BeginArea(new Rect(25, 30, maxwidth, 600));

                GUILayout.Label("Test Midi File Player Scripting - Demonstrate how to use the MPTK API to plays Midi", myStyle.TitleLabel1);
                if (GUILayout.Button(new GUIContent("Return to menu", ""))) GoMainMenu.Go();
                GUILayout.Space(20);

                GUISelectSoundFont.Display(myStyle);

                // Status and current position of the midi playing
                GUILayout.Space(20);
                GUILayout.Label("Current midi '" + midiFilePlayer.MPTK_MidiName + "'" + (midiFilePlayer.MPTK_IsPlaying ? " is playing" : "is not playing"), myStyle.TitleLabel3);
                float currentposition = midiFilePlayer.MPTK_Position / 1000f;
                float position = GUILayout.HorizontalSlider(currentposition, 0f, (float)midiFilePlayer.MPTK_Duration.TotalSeconds);
                if (position != currentposition)
                    midiFilePlayer.MPTK_Position = position * 1000f;

                // Define the global volume
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Global Volume: " + Math.Round(midiFilePlayer.MPTK_Volume, 2), myStyle.TitleLabel3, GUILayout.Width(200));
                midiFilePlayer.MPTK_Volume = GUILayout.HorizontalSlider(midiFilePlayer.MPTK_Volume * 100f, 0f, 100f) / 100f;
                GUILayout.EndHorizontal();

                // Transpose each note
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Transpose: " + midiFilePlayer.MPTK_Transpose, myStyle.TitleLabel3, GUILayout.Width(200));
                midiFilePlayer.MPTK_Transpose = (int)GUILayout.HorizontalSlider((float)midiFilePlayer.MPTK_Transpose, -24f, 24f);
                GUILayout.EndHorizontal();

                // Time before a note is stop after note off
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Release Time: " + Math.Round(midiFilePlayer.MPTK_TimeToRelease, 2), myStyle.TitleLabel3, GUILayout.Width(200));
                midiFilePlayer.MPTK_TimeToRelease = GUILayout.HorizontalSlider(midiFilePlayer.MPTK_TimeToRelease, 0.05f, 1f);
                GUILayout.EndHorizontal();

                // Random playing ?
                GUILayout.Space(20);
                IsRandomPlay = GUILayout.Toggle(IsRandomPlay, "  Random Play Midi");
                GUILayout.Space(20);

                // Play/Pause/Stop/Restart actions on midi 
                GUILayout.BeginHorizontal();
                if (midiFilePlayer.MPTK_IsPlaying && !midiFilePlayer.MPTK_IsPaused)
                    GUI.color = ButtonColor;
                if (GUILayout.Button(new GUIContent("Play", ""))) midiFilePlayer.MPTK_Play();
                GUI.color = Color.white;

                if (midiFilePlayer.MPTK_IsPaused)
                    GUI.color = ButtonColor;
                if (GUILayout.Button(new GUIContent("Pause", "")))
                    if (midiFilePlayer.MPTK_IsPaused)
                        midiFilePlayer.MPTK_Play();
                    else
                        midiFilePlayer.MPTK_Pause();
                GUI.color = Color.white;

                if (GUILayout.Button(new GUIContent("Stop", ""))) midiFilePlayer.MPTK_Stop();

                if (GUILayout.Button(new GUIContent("Restart", ""))) midiFilePlayer.MPTK_RePlay();
                GUILayout.EndHorizontal();

                // Previous and Next button action on midi
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Previous", ""))) midiFilePlayer.MPTK_Previous();
                if (GUILayout.Button(new GUIContent("Next", ""))) midiFilePlayer.MPTK_Next();
                GUILayout.EndHorizontal();

                GUILayout.Label("Go to your Hierarchy, select GameObject MidiFilePlayer, inspector contains more parameters to control your Midi player.");

                GUILayout.EndArea();
            }

        }

        /// <summary>
        /// Event fired by MidiFilePlayer when a midi is ended (set by Unity Editor in MidiFilePlayer Inspector)
        /// </summary>
        public void RandomPlay()
        {
            if (IsRandomPlay)
            {
                //Debug.Log("Is playing : " + midiFilePlayer.MPTK_IsPlaying);
                int index = UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);
                midiFilePlayer.MPTK_MidiIndex = index;
                midiFilePlayer.MPTK_Play();
            }
            else
                midiFilePlayer.MPTK_RePlay();
        }

        // Update is called once per frame
        void Update()
        {
            if (midiFilePlayer != null && midiFilePlayer.MPTK_IsPlaying)
            {
                float time = Time.realtimeSinceStartup - LastTimeChange;
                if (time > DelayTimeChange)
                {
                    // It's time to apply randon change
                    LastTimeChange = Time.realtimeSinceStartup;

                    // Random position
                    if (IsRandomPosition) midiFilePlayer.MPTK_Position = UnityEngine.Random.Range(0f, (float)midiFilePlayer.MPTK_Duration.TotalMilliseconds);
                    // Random Speed
                    if (IsRandomSpeed) midiFilePlayer.MPTK_Speed = UnityEngine.Random.Range(0.1f, 5f);
                    // Random transmpose
                    if (IsRandomTranspose) midiFilePlayer.MPTK_Transpose = UnityEngine.Random.Range(-12, 12);
                }
            }
        }
    }
}