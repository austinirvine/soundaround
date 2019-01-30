using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using InfinityMusic;

namespace MidiPlayerTK
{
    public class TestMidiStream : MonoBehaviour
    {

        // MPTK component able to play a stream of note like midi note generated
        public MidiStreamPlayer midiStreamPlayer;

        [Range(0.1f, 10f)]
        public float DelayTimeChange = 1;

        public bool RandomPlay = true;
        public bool DrumKit = false;

        [Range(0, 127)]
        public int StartNote = 50;

        [Range(0, 127)]
        public int EndNote = 60;

        [Range(0, 127)]
        public int Velocity = 100;

        [Range(0, 127)]
        public int CurrentNote;

        [Range(0, 127)]
        public int CurrentPatch;

        /// <summary>
        /// Current note playing
        /// </summary>
        private MPTKNote NotePlaying;

        private float LastTimeChange;

        /// <summary>
        /// Window srcoll
        /// </summary>
        private Vector2 scroller = Vector2.zero;

        /// <summary>
        /// Popup to select an instrument
        /// </summary>
        private PopupSelectPatch PopPatch;

        // Manage skin
        public GUISkin customSkin;
        public CustomStyle myStyle;

        // Use this for initialization
        void Start()
        {

            //MidiPlayerGlobal.MPTK_SelectSoundFont("CelloXylophone");

            PopPatch = new PopupSelectPatch();
            InitPlay();
            CurrentNote = StartNote;
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

        public void InitPlay()
        {
            LastTimeChange = Time.realtimeSinceStartup;
        }

        void OnGUI()
        {
            // Set custom Style. Good for background color 3E619800
            if (customSkin != null) GUI.skin = customSkin;
            if (myStyle == null) myStyle = new CustomStyle();

            float maxwidth = Screen.width / 2;
            if (maxwidth < 300) maxwidth = 300;

            if (midiStreamPlayer != null)
            {
                GUILayout.BeginArea(new Rect(25, 30, maxwidth, 600));

                GUILayout.Label("Test Midi Stream - A very simple music stream generated", myStyle.TitleLabel1);
                if (GUILayout.Button(new GUIContent("Return to menu", ""))) GoMainMenu.Go();
                GUILayout.Space(20);

                GUISelectSoundFont.Display(myStyle);

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Global Volume: " + Math.Round(midiStreamPlayer.MPTK_Volume, 2), myStyle.TitleLabel3, GUILayout.Width(200));
                midiStreamPlayer.MPTK_Volume = GUILayout.HorizontalSlider(midiStreamPlayer.MPTK_Volume * 100f, 0f, 100f) / 100f;
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Transpose: " + midiStreamPlayer.MPTK_Transpose, myStyle.TitleLabel3, GUILayout.Width(200));
                midiStreamPlayer.MPTK_Transpose = (int)GUILayout.HorizontalSlider((float)midiStreamPlayer.MPTK_Transpose, -24f, 24f);
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Velocity: " + Velocity, myStyle.TitleLabel3, GUILayout.Width(200));
                Velocity = (int)GUILayout.HorizontalSlider((int)Velocity, 0f, 127f);
                GUILayout.EndHorizontal();

                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Note Release Time: " + Math.Round(midiStreamPlayer.MPTK_TimeToRelease, 2), myStyle.TitleLabel3, GUILayout.Width(200));
                midiStreamPlayer.MPTK_TimeToRelease = GUILayout.HorizontalSlider(midiStreamPlayer.MPTK_TimeToRelease, 0.05f, 1f);
                GUILayout.EndHorizontal();

                GUILayout.Space(20);

                GUILayout.BeginHorizontal();
                RandomPlay = GUILayout.Toggle(RandomPlay, "   Random Play", GUILayout.Width(200));
                DrumKit = GUILayout.Toggle(DrumKit, "   Drum Kit", GUILayout.Width(200));


                if (MidiPlayerGlobal.MPTK_ListPreset != null && MidiPlayerGlobal.MPTK_ListPreset.Count > 0)
                {
                    if (!RandomPlay && DrumKit)
                        if (MidiPlayerGlobal.MPTK_ListDrum != null && CurrentNote < MidiPlayerGlobal.MPTK_ListDrum.Count)
                            // Display the drum playing (each key is a different drum in midi norm)
                            GUILayout.Label("Drum:" + MidiPlayerGlobal.MPTK_ListDrum[CurrentNote], myStyle.TitleLabel3);
                        else
                            GUILayout.Label("No Drumkit found in your SoundFont for key " + CurrentNote, myStyle.TitleLabel3);
                    else
                    {
                        if (CurrentPatch >= 0 && CurrentPatch < MidiPlayerGlobal.MPTK_ListPreset.Count)
                        {
                            // Open the popup to select an instrument
                            if (GUILayout.Button(MidiPlayerGlobal.MPTK_ListPreset[CurrentPatch]))
                            {
                                PopPatch.Selected = CurrentPatch;
                                PopPatch.DispatchPopupPatch = !PopPatch.DispatchPopupPatch;
                            }

                            // If need, display the popup to select an instrument
                            //New1.95
                            int newpatch = PopPatch.Draw(myStyle);
                            if (newpatch != CurrentPatch)
                                CurrentPatch = newpatch;


                            Event e = Event.current;
                            if (e.type == EventType.Repaint)
                            {
                                // Get the position of the button to set the position popup near the button : same X and above
                                Rect lastRect = GUILayoutUtility.GetLastRect();
                                // Don't forget that the window can be scrolled !
                                PopPatch.Position = new Vector2(
                                    lastRect.x - scroller.x,
                                    lastRect.y - PopPatch.RealRect.height - lastRect.height - scroller.y);
                            }
                        }
                        else
                            GUILayout.Label("Patch: " + CurrentPatch + " Nothing found", myStyle.TitleLabel3);
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(20);
                GUILayout.Label("Go to your Hierarchy, select GameObject TestMidiStream: inspector contains some parameters to control your generator.", myStyle.TitleLabel3);

                GUILayout.EndArea();
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (midiStreamPlayer != null)
            {
                float time = Time.realtimeSinceStartup - LastTimeChange;
                if (time > DelayTimeChange)
                {
                    // It's time to generate a note
                    LastTimeChange = Time.realtimeSinceStartup;

                    if (NotePlaying != null)
                    {
                        //Debug.Log("Stop note");
                        // Stop the note (method to simulate a real human on a keyboard : duration is not known when note is triggers)
                        NotePlaying.Stop();
                        NotePlaying = null;
                    }

                    if (RandomPlay)
                    {
                        //
                        // First method to play notes: send a list of notes directly to the MidiStreamPlayer
                        // Useful for a long list of notes. MPTKNote is the class to do this.
                        //
                        List<MPTKNote> notes = new List<MPTKNote>();
                        // Very light random notes generator
                        if (!DrumKit)
                        {
                            // Play 3 notes with no delay
                            int rnd = UnityEngine.Random.Range(-8, 8);
                            notes.Add(CreateNote(60 + rnd, 0));
                            notes.Add(CreateNote(64 + rnd, 0));
                            notes.Add(CreateNote(67 + rnd, 0));
                        }
                        else
                        {
                            // Play 3 hit with a short delay
                            
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 0));
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 150));
                            notes.Add(CreateDrum(UnityEngine.Random.Range(0, 127), 300));
                        }
                        // Send the note to the player. Notes are plays in a thread, so call returns immediately
                        midiStreamPlayer.MPTK_Play(notes);
                    }
                    else
                    {
                        //
                        // Second methods to play notes: Play note from the Play method of MPTKNote
                        // Useful if you want to stop playing the note by script.
                        // Here a big value (99 sec.) is defined for Duration, the note and the waves associated, 
                        // are stopped when NotePlaying.Stop() is call (si above)
                        // Notes are plays sequencially.
                        if (++CurrentNote > EndNote) CurrentNote = StartNote;
                        NotePlaying = new MPTKNote()
                        {
                            Note = CurrentNote,
                            Delay = 0,
                            Drum = DrumKit, // drum kit is plays (same as Midi canal 10) if true
                            Duration = 99999,
                            Patch = CurrentPatch,
                            Velocity = Velocity // Sound can vary depending on the velocity
                        };
                        NotePlaying.Play(midiStreamPlayer);
                    }
                }
            }
        }

        /// <summary>
        /// Helper to create a random note (not yet used)
        /// </summary>
        /// <returns></returns>
        private MPTKNote CreateRandomNote()
        {
            MPTKNote note = new MPTKNote()
            {
                Note = 50 + UnityEngine.Random.Range(0, 4) * 2,
                Drum = false,
                Duration = UnityEngine.Random.Range(100, 1000),
                Patch = CurrentPatch,
                Velocity = Velocity,
                Delay = UnityEngine.Random.Range(0, 200),
            };
            return note;
        }

        /// <summary>
        /// Helper to create a note 
        /// </summary>
        /// <returns></returns>
        private MPTKNote CreateNote(int key, float delay)
        {
            MPTKNote note = new MPTKNote()
            {
                Note = key,
                Drum = false,
                Duration = DelayTimeChange * 1000f,
                Patch = CurrentPatch,
                Velocity = Velocity,
                Delay = delay,
            };
            return note;
        }

        /// <summary>
        /// Helper to create a drum hit 
        /// </summary>
        /// <returns></returns>
        private MPTKNote CreateDrum(int key, float delay)
        {
            MPTKNote note = new MPTKNote()
            {
                Note = key,
                Drum = true,
                Duration = 0,
                Patch = 0,
                Velocity = Velocity,
                Delay = delay,
            };
            return note;
        }
    }
}