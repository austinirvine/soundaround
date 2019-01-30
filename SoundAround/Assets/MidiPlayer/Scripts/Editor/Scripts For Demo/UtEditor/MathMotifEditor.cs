using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using MidiPlayerTK;

namespace InfinityMusic
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UtMathMotif))]
    public class MathMotifEditor : Editor
    {
        private static UtMathMotif instance;
        private string[] list;
        private GUIStyle styleBold;
        void OnEnable()
        {
            try
            {
                instance = (UtMathMotif)target;
                if (MidiPlayerGlobal.MPTK_ListPreset != null && MidiPlayerGlobal.MPTK_ListPreset.Count > 0)
                {
                    list = new string[MidiPlayerGlobal.MPTK_ListPreset.Count];
                    MidiPlayerGlobal.MPTK_ListPreset.CopyTo(list, 0);
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
                styleBold = new GUIStyle("Label")
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold
                };


                int MeasureCount = EditorGUILayout.IntSlider(new GUIContent("Measure"), instance.MeasureCount, 1, 8);
                if (MeasureCount != instance.MeasureCount)
                {
                    instance.MeasureCount = MeasureCount;
                    instance.Generate(true);
                }
                instance.CurrentCadence = (UtCadence)EditorGUILayout.ObjectField(new GUIContent("Cadence"), instance.CurrentCadence, typeof(UtCadence), true);

                instance.DrumKit = EditorGUILayout.Toggle(new GUIContent("Drum Kit", ""), instance.DrumKit);
                if (!instance.DrumKit)
                {
                    if (list != null)
                    {
                        instance.PatchIndex = EditorGUILayout.Popup("Patch", instance.PatchIndex, list);
                    }
                    else
                        EditorGUILayout.LabelField(new GUIContent("No patchs (wave) found", ""));
                }

                //
                // Scale
                //
                EditorGUILayout.LabelField(new GUIContent("Scale", "Used a predefined scale"));
                EditorGUI.indentLevel++;
                int ScaleIndex = EditorGUILayout.Popup("Select", instance.ScaleIndex, ScaleDefinition.Names.ToArray());
                if (ScaleIndex != instance.ScaleIndex)
                {
                    instance.ScaleIndex = ScaleIndex;
                    instance.Generate(true);
                }
                int StepInScale = EditorGUILayout.IntSlider(new GUIContent("Step In Scale", "With 1 get all notes in scale, with n get note each n"), instance.StepInScale, 1, 10);
                if (StepInScale != instance.StepInScale)
                {
                    instance.StepInScale = StepInScale;
                    instance.Generate(true);
                }
                EditorGUI.indentLevel--;

                //
                // Volume
                //
                EditorGUILayout.LabelField(new GUIContent("Volume note", ""));
                EditorGUI.indentLevel++;
                instance.Velocity = EditorGUILayout.IntSlider(new GUIContent("Velocity", "Volume of the note played"), instance.Velocity, 0, 127);
                instance.Accentuation = EditorGUILayout.IntSlider(new GUIContent("Accentuation", "Increases the volume by percent on the first note of the measure"), instance.Accentuation, 0, 100);
                EditorGUI.indentLevel--;

                //
                // Generator
                //
                EditorGUILayout.LabelField(new GUIContent("Generator", ""));
                EditorGUI.indentLevel++;
                int SelectedAlgo = EditorGUILayout.Popup("Algo", (int)instance.SelectedAlgo, Enum.GetNames(typeof(Mode)));
                if (SelectedAlgo != (int)instance.SelectedAlgo)
                {
                    instance.SelectedAlgo = (Mode)SelectedAlgo;
                    instance.Generate(true);
                }

                int octave = EditorGUILayout.IntSlider(new GUIContent("Octave min", "Octave for starting generating notes"), instance.OctaveMin, 0, 10);
                if (octave != instance.OctaveMin)
                {
                    instance.OctaveMin = octave;
                    instance.Generate(true);
                }

                octave = EditorGUILayout.IntSlider(new GUIContent("Octave max", "Octave for ending generating notes"), instance.OctaveMax, 0, 10);
                if (octave != instance.OctaveMax)
                {
                    instance.OctaveMax = octave;
                    instance.Generate(true);
                }
                instance.Transpose = EditorGUILayout.IntSlider(new GUIContent("Transpose", "Transpose of the note played"), instance.Transpose, -48, 48);

                if (instance.SelectedAlgo == Mode.CircleDown || instance.SelectedAlgo == Mode.CircleUp)
                    instance.RotationSpeed = EditorGUILayout.IntSlider(new GUIContent("Rotation", "Rotation speed for Circle algo"), instance.RotationSpeed, -500, 500);

                int RepeatRate = EditorGUILayout.IntSlider(new GUIContent("Repeat rate", "Repeat rate of the last notes played"), instance.RepeatRate, 0, 100);
                if (RepeatRate != instance.RepeatRate)
                {
                    instance.RepeatRate = RepeatRate;
                    instance.Generate(true);
                }
                EditorGUI.indentLevel--;

                //
                // Running
                //
                if (Application.isPlaying)
                {
                    EditorGUILayout.LabelField(new GUIContent("Measure", ""),
                        new GUIContent(string.Format("{0,3:000}.{1,2:00}.{2,2:00}",
                        InfinityMusic.instance.IndexMeasure + 1, InfinityMusic.instance.IndexQuarterMeasure + 1, InfinityMusic.instance.IndexSixteenthMeasure + 1), ""), styleBold);
                    EditorGUILayout.LabelField(new GUIContent("Played Note", ""),
                        new GUIContent(string.Format("{0,3:000} / {1,3:000}", instance.CurrentIndexNotePlayed, instance.Score != null ? instance.Score.Length : 0), ""), styleBold);
                }

                //
                // Actions
                //
                if (Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Generate", ""), GUILayout.ExpandWidth(false)))
                        instance.Generate(true);
                    if (GUILayout.Button(new GUIContent("Set To Default", "Restore default value"), GUILayout.ExpandWidth(false)))
                    {
                        instance.DefaultValue();
                        instance.Generate(true);
                    }
                    EditorGUILayout.EndHorizontal();
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
