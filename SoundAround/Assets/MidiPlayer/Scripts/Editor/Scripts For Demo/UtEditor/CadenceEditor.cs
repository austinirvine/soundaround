using UnityEngine;
using UnityEditor;

using System;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using InfinityMusic;
using MidiPlayerTK;

namespace InfinityMusic
{
    /// <summary>
    /// Inspector for the midi global player component
    /// </summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UtCadence))]
    public class CadenceEditor : Editor
    {
        private static UtCadence instance;
        private GUIStyle styleBold;

        void OnEnable()
        {
            try
            {
                instance = (UtCadence)target;
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

                instance.MeasureCount = EditorGUILayout.IntSlider(new GUIContent("Measure Count", ""), instance.MeasureCount, 1, 8);
                instance.PctSilence = EditorGUILayout.IntSlider(new GUIContent("Silence Rate", "Rate of silence"), instance.PctSilence, 0, 100);
                instance.RatioWhole = EditorGUILayout.IntSlider(new GUIContent("Whole Rate", "Rate of Whole = 1 measure"), instance.RatioWhole, 0, 100);
                instance.RatioHalf = EditorGUILayout.IntSlider(new GUIContent("Half Rate", "Rate of Half = 1/2 measure"), instance.RatioHalf, 0, 100);
                instance.RatioQuarter = EditorGUILayout.IntSlider(new GUIContent("Quarter Rate", "Rate of Quarter = 1/4 measure"), instance.RatioQuarter, 0, 100);
                instance.RatioEighth = EditorGUILayout.IntSlider(new GUIContent("Eighth Rate", "Rate of Eighth = 1/8 measure"), instance.RatioEighth, 0, 100);
                instance.RatioSixteen = EditorGUILayout.IntSlider(new GUIContent("Sixteen Rate", "Rate of Sixteen = 1/16 measure"), instance.RatioSixteen, 0, 100);

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
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(new GUIContent("Generate", ""), GUILayout.ExpandWidth(false)))
                        instance.Generate(false);
                    EditorGUILayout.EndHorizontal();
                }

                //showDefault = EditorGUILayout.Foldout(showDefault, "Show default editor");
                //if (showDefault) DrawDefaultInspector();

                if (GUI.changed) EditorUtility.SetDirty(instance);
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        //private static bool showDefault = true;


    }

}
