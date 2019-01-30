using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace MidiPlayerTK
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CustomEditor(typeof(MidiStreamPlayer))]
    public class StreamPlayerEditor : Editor
    {
        private static MidiStreamPlayer instance;

        void OnEnable()
        {
            try
            {
                instance = (MidiStreamPlayer)target;
                // Load description of available soundfont
                if (MidiPlayerGlobal.CurrentMidiSet == null || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo == null)
                {
                    ToolsEditor.LoadMidiSet();
                    ToolsEditor.CheckMidiSet();
                }
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

                //mDebug.Log(Event.current.type);

                string soundFontSelected = "No SoundFont selected.";
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo != null)
                {
                    soundFontSelected = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name;
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + soundFontSelected, "Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f"));
                    EditorGUILayout.Separator();

                    float volume = EditorGUILayout.Slider(new GUIContent("Volume", "Set global volume for this midi playing"), instance.MPTK_Volume, 0f, 1f);
                    if (instance.MPTK_Volume != volume)
                        instance.MPTK_Volume = volume;


                    EditorGUILayout.BeginHorizontal();
                    string tooltipDistance = "Playing is paused if distance between AudioListener and this component is greater than MaxDistance";
                    instance.MPTK_PauseOnDistance = EditorGUILayout.Toggle(new GUIContent("Pause With Distance", tooltipDistance), instance.MPTK_PauseOnDistance);
                    EditorGUILayout.LabelField(new GUIContent("Current:" + Math.Round(instance.distanceEditorModeOnly, 2), tooltipDistance));
                    EditorGUILayout.EndHorizontal();

                    float distance = EditorGUILayout.Slider(new GUIContent("Max Distance", tooltipDistance), instance.MPTK_MaxDistance, 0f, 500f);
                    if (instance.MPTK_MaxDistance != distance)
                        instance.MPTK_MaxDistance = distance;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Transpose");
                    instance.MPTK_Transpose = EditorGUILayout.IntSlider(instance.MPTK_Transpose, -24, 24);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Release Time (sec.)");
                    instance.MPTK_TimeToRelease = EditorGUILayout.Slider(instance.MPTK_TimeToRelease, 0.05f, 1f);
                    EditorGUILayout.EndHorizontal();

                    instance.MPTK_LogWaves = EditorGUILayout.Toggle(new GUIContent("Log Waves", "Log information about wave for each notes played"), instance.MPTK_LogWaves);
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("SoundFont: " + soundFontSelected, "Define SoundFont from the menu 'Tools/MPTK - SoundFont Setup' or alt-f"));
                    ToolsEditor.LoadMidiSet();
                    ToolsEditor.CheckMidiSet();
                }

                if (GUI.changed) EditorUtility.SetDirty(instance);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }


    }

}
