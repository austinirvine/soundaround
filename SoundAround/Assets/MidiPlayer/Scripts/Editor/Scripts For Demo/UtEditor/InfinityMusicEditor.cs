using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using MidiPlayerTK;
using System.Reflection;

namespace InfinityMusic
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CustomEditor(typeof(InfinityMusic))]
    public class InfinityMusicEditor : Editor
    {
        private static InfinityMusic instance;
        private GUIStyle styleBold;
        private Type ObjType;

        void OnEnable()
        {
            try
            {
                instance = (InfinityMusic)target;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        public override void OnInspectorGUI()
        {
            try
            {
                GUI.changed = false;
                GUI.color = Color.white;
                styleBold = new GUIStyle("Label")
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };

                string soundFontSelected = "No SoundFont selected.";
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    soundFontSelected = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name;
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name, "Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f"));
             //       EditorGUILayout.Separator();

                    instance.MidiStreamPlayer = (MidiStreamPlayer)EditorGUILayout.ObjectField(new GUIContent("Midi Stream Player"), instance.MidiStreamPlayer, typeof(MidiStreamPlayer), true);

                    ObjType = instance.GetType();
                    instance.MeasureLength = UtToolsEditor.IntSlider("Mesure Length", "Quarter number per measure", "MeasureLength", instance.MeasureLength, ObjType);
                    instance.QuarterPerMinute = UtToolsEditor.IntSlider("Quarter Per Minute", "Quarter Per Minute", "QuarterPerMinute", instance.QuarterPerMinute, ObjType);

                    //
                    // Running
                    //
                    if (Application.isPlaying)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Measure", ""),
                            new GUIContent(string.Format("{0,3:000}.{1,2:00}.{2,2:00}",
                            InfinityMusic.instance.IndexMeasure + 1, InfinityMusic.instance.IndexQuarterMeasure + 1, InfinityMusic.instance.IndexSixteenthMeasure + 1), ""), styleBold);
                    }

                    //
                    // Actions
                    //
                    if (Application.isPlaying)
                    {
                        instance.SongName = EditorGUILayout.TextField(new GUIContent("Song Name", ""), instance.SongName);
                        instance.Description = EditorGUILayout.TextArea(instance.Description);

                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField(new GUIContent("Song", ""));
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("New"), GUILayout.ExpandWidth(true)))
                        {
                            if (EditorUtility.DisplayDialog("New Song", "Erase current song ?", "Ok", "Cancel"))
                                InfinityMusic.UtNewSong();
                        }

                        if (GUILayout.Button(new GUIContent("Save"), GUILayout.ExpandWidth(true)))
                        {
                            string path = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToSong);
                            if (!Directory.Exists(path))
                                Directory.CreateDirectory(path);

                            string filepath = EditorUtility.SaveFilePanel("Save Song", path, null, MidiPlayerGlobal.ExtensionSong);
                            if (!string.IsNullOrEmpty(filepath))
                            {
                                SaveLoad.UtSave(filepath);
                            }
                        }

                        if (GUILayout.Button(new GUIContent("Open"), GUILayout.ExpandWidth(true)))
                        {
                            string path = EditorUtility.OpenFilePanel("Open Song", Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToSong), MidiPlayerGlobal.ExtensionSong);
                            if (!string.IsNullOrEmpty(path))
                            {
                                InfinityMusic.UtNewSong();
                                SaveLoad.UtLoad(path);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField(new GUIContent("Create components", ""));
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(new GUIContent("Math Motif", ""), GUILayout.ExpandWidth(true)))
                            UtComponent.UtNew(UtComponent.Component.Motif);
                        if (GUILayout.Button(new GUIContent("Cadence", ""), GUILayout.ExpandWidth(true)))
                            UtComponent.UtNew(UtComponent.Component.Cadence);
                        EditorGUILayout.EndHorizontal();

                    
                    }
                    else
                    {
                        //if (GUILayout.Button(new GUIContent("View folder", ""), GUILayout.ExpandWidth(false)))
                        //    EditorUtility.RevealInFinder(UtGlobal.BuildPathConfig(null));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + soundFontSelected, "Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f"));
                    ToolsEditor.LoadMidiSet();
                    ToolsEditor.CheckMidiSet();
                }

                showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                if (showDefault) DrawDefaultInspector();

                if (GUI.changed) EditorUtility.SetDirty(instance);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        private static bool showDefault = false;


    }

}
