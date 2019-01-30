using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;
using UnityEngine.Events;

namespace MidiPlayerTK
{
    public class MusicView : MonoBehaviour
    {

        public MidiFilePlayer midiFilePlayer;
        public MidiStreamPlayer midiStreamPlayer;
        public static Color ButtonColor = new Color(.7f, .9f, .7f, 1f);
        public NoteView NoteDisplay;
        public Collide Collider;
        public GameObject Plane;
        public float minZ, maxZ, minX, maxX;
        public float LastTimeCollider;
        public float DelayCollider = 25;

        public void EndLoadingSF()
        {
            Debug.Log("End loading SF, MPTK is ready to play");
            Debug.Log("   Time To Load SoundFont: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadSoundFont.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Time To Load Waves: " + Math.Round(MidiPlayerGlobal.MPTK_TimeToLoadWave.TotalSeconds, 3).ToString() + " second");
            Debug.Log("   Presets Loaded: " + MidiPlayerGlobal.MPTK_CountPresetLoaded);
            Debug.Log("   Waves Loaded: " + MidiPlayerGlobal.MPTK_CountWaveLoaded);
        }

        void Start()
        {
            // Default size of a Unity Plan
            float planSize = 10f;

            minZ = Plane.transform.localPosition.z - Plane.transform.localScale.z * planSize / 2f;
            maxZ = Plane.transform.localPosition.z + Plane.transform.localScale.z * planSize / 2f;

            minX = Plane.transform.localPosition.x - Plane.transform.localScale.x * planSize / 2f;
            maxX = Plane.transform.localPosition.x + Plane.transform.localScale.x * planSize / 2f;

            //midiFilePlayer.MPTK_MidiIndex = 4; // UnityEngine.Random.Range(0, MidiPlayerGlobal.MPTK_ListMidi.Count);

            // If call is already set from the inspector there is no need to set another listeneer
            if (midiFilePlayer.OnEventNotesMidi.GetPersistentEventCount() == 0)
            {
                // No listener defined, set now by script. NotesToPlay will be called for each new notes read from Midi file
                Debug.Log("No OnEventNotesMidi defined, set by script");
                midiFilePlayer.OnEventNotesMidi = new MidiFilePlayer.ListNotesEvent();
                midiFilePlayer.OnEventNotesMidi.AddListener(NotesToPlay);
            }
        }

        public void NotesToPlay(List<MidiNote> notes)
        {
            //Debug.Log(notes.Count);
            foreach (MidiNote note in notes)
            {
                if (note.Midi > 40 && note.Midi < 100)// && note.Channel==1)
                {
                    float z = Mathf.Lerp(minZ, maxZ, (note.Midi - 40) / 60f);
                    Vector3 position = new Vector3(maxX, 8, z);
                    NoteView n = Instantiate<NoteView>(NoteDisplay, position, Quaternion.identity);
                    n.gameObject.SetActive(true);
                    n.midiFilePlayer = midiFilePlayer;
                    n.note = note;
                    n.gameObject.GetComponent<Renderer>().material = n.NewNote;
                    n.zOriginal = position.z;

                    MPTKNote mptkNote = new MPTKNote() { Delay = 0, Drum = false, Duration = 0.2f, Note = 60, Patch = 10, Velocity = 100 };
                    mptkNote.Play(midiStreamPlayer);
                }
            }
        }


        void OnGUI()
        {
            int startx = 5;
            int starty = 5;
            int maxwidth = Screen.width;// / 2;

            if (midiFilePlayer != null)
            {
                GUILayout.BeginArea(new Rect(startx, starty, maxwidth, 200));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Previous", ""), GUILayout.Width(150))) midiFilePlayer.MPTK_Previous();
                if (GUILayout.Button(new GUIContent("Next", ""), GUILayout.Width(150))) midiFilePlayer.MPTK_Next();
                if (GUILayout.Button(new GUIContent("Clear", ""), GUILayout.Width(150))) Clear();
                GUILayout.Label("   " + midiFilePlayer.MPTK_MidiName + (midiFilePlayer.MPTK_IsPlaying ? " is playing" : " is not playing"));
                GUILayout.EndHorizontal();
                //GUILayout.Space(30);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Position :", GUILayout.Width(100));
                float currentposition = midiFilePlayer.MPTK_Position / 1000f;
                float position = GUILayout.HorizontalSlider(currentposition, 0f, (float)midiFilePlayer.MPTK_Duration.TotalSeconds, GUILayout.Width(200));
                if (position != currentposition) midiFilePlayer.MPTK_Position = position * 1000f;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed Music :", GUILayout.Width(100));
                float speed = GUILayout.HorizontalSlider(midiFilePlayer.MPTK_Speed, 1f, 5f, GUILayout.Width(200));
                if (speed != midiFilePlayer.MPTK_Speed) midiFilePlayer.MPTK_Speed = speed;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Speed Note :", GUILayout.Width(100));
                NoteView.Speed = GUILayout.HorizontalSlider(NoteView.Speed, 5f, 20f, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.Label("Be careful with the notes traffic jam!!!");

                GUILayout.EndArea();
            }
        }

        public void Clear()
        {
            NoteView[] components = GameObject.FindObjectsOfType<NoteView>();
            foreach (NoteView noteview in components)
            {
                if (noteview.enabled)
                    //Debug.Log("destroy " + ut.name);
                    DestroyImmediate(noteview.gameObject);
            }
        }

        void Update()
        {
            if (midiFilePlayer != null && midiFilePlayer.MPTK_IsPlaying)
            {
                float time = Time.realtimeSinceStartup - LastTimeCollider;
                if (time > DelayCollider)
                {
                    LastTimeCollider = Time.realtimeSinceStartup;

                    float zone = 10;
                    Vector3 position = new Vector3(UnityEngine.Random.Range(minX + zone, maxX - zone), -5, UnityEngine.Random.Range(minZ + zone, maxZ - zone));
                    Collide n = Instantiate<Collide>(Collider, position, Quaternion.identity);
                    n.gameObject.SetActive(true);
                }
            }
        }
    }
}