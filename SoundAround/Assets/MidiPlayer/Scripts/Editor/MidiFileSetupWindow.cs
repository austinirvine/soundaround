using System;
using System.Collections.Generic;
using System.IO;
using NAudio.Midi;
namespace MidiPlayerTK
{
    //using MonoProjectOptim;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Window editor for the setup of MPTK
    /// </summary>
    public class MidiFileSetupWindow : EditorWindow
    {
      
        private static MidiFileSetupWindow window;

        Vector2 scrollPosMidiFile = Vector2.zero;
        Vector2 scrollPosAnalyze = Vector2.zero;

        static float widthLeft;
        static float widthRight;

        static float heightList;

        static int itemHeight;
        static int buttonWidth;
        static int buttonHeight;
        static float espace;

        static float xpostitlebox;
        static float ypostitlebox;

        string midifile;

        static GUIStyle styleBold;
        static GUIStyle styleRichText;
        static float heightLine;

        static BuilderInfo ScanInfo;

        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("Tools/MPTK - Midi File Setup &M")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            try
            {
                window = (MidiFileSetupWindow)EditorWindow.GetWindow(typeof(MidiFileSetupWindow));
                window.titleContent.text = "Midi File List";
                window.minSize = new Vector2(828, 400);

                styleBold = new GUIStyle(EditorStyles.boldLabel);
                styleBold.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;
                heightLine = styleRichText.lineHeight * 1.2f;

                espace = 5;
                widthLeft = 415;
                itemHeight = 25;
                buttonWidth = 150;
                buttonHeight = 18;

                xpostitlebox = 2;
                ypostitlebox = 5;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
      
        private void OnLostFocus()
        {
#if UNITY_2017_1_OR_NEWER
            // Trig an  error before v2017...
            if (Application.isPlaying)
            {
                window.Close();
            }
#endif
        }
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        void OnGUI()
        {
            try
            {
                if (window == null) Init();
                float startx = 5;
                float starty = 7;
                //Log.Write("test");

                GUIContent content = new GUIContent() { text = "Setup Midi files to play in your application - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                GUI.color = ToolsEditor.ButtonColor;
                content = new GUIContent() { text = "Help & Contact", tooltip = "Get some help" };
                Rect rect = new Rect(window.position.size.x - buttonWidth - 5, starty, buttonWidth, buttonHeight);
                try
                {
                    if (GUI.Button(rect, content))
                        PopupWindow.Show(rect, new AboutMPTK());
                }
                catch (Exception)
                {
                    // generate some weird exception ...
                }

                starty += buttonHeight + espace;

                widthRight = window.position.size.x - widthLeft - 2 * espace - startx;
                heightList = window.position.size.y -  3 * espace - starty;

                ShowListMidiFiles(startx, starty + espace);
                ShowMidiAnalyse(startx + widthLeft + espace, starty + espace);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }



        /// <summary>
        /// Display, add, remove Midi file
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowListMidiFiles(float localstartX, float localstartY)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, widthLeft, heightList);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                string caption = "Midi file available";
                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null || MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count == 0)
                {
                    caption = "No Midi file available yet";
                    ScanInfo = new BuilderInfo();
                }

                GUIContent content = new GUIContent() { text = caption, tooltip = "" };
                EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);

                if (GUI.Button(new Rect(widthLeft - buttonWidth - espace, localstartY + ypostitlebox, buttonWidth, buttonHeight), "Add Midi file"))
                    AddMidifile();

                if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles != null)
                {
                    Rect listVisibleRect = new Rect(localstartX, localstartY + itemHeight, widthLeft - 5, heightList - itemHeight - 5);
                    Rect listContentRect = new Rect(0, 0, widthLeft - 20, MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count * itemHeight + 5);

                    scrollPosMidiFile = GUI.BeginScrollView(listVisibleRect, scrollPosMidiFile, listContentRect);
                    float boxY = 0;

                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Count; i++)
                    {
                        GUI.color = new Color(.7f, .7f, .7f, 1f);
                        float boxX = 5;
                        GUI.Box(new Rect(boxX, boxY + 5, widthLeft - 30, itemHeight), "");
                        GUI.color = Color.white;

                        content = new GUIContent() { text = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i], tooltip = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i] };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, 200, itemHeight), content);

                        boxX += 200 + espace;
                        if (GUI.Button(new Rect(boxX, boxY + 9, 80, buttonHeight), "Analyse"))
                        {
                            ScanInfo = new BuilderInfo();
                            midifile = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i];
                            ScanInfo.Add(midifile);
                            MidiScan.GeneralInfo(midifile, ScanInfo);
                            scrollPosAnalyze = Vector2.zero;
                        }
                        boxX += 80 + espace;

                        GUI.color = Color.white;

                        if (GUI.Button(new Rect(boxX, boxY + 9, 80, buttonHeight), "Remove"))
                        {
                            if (!string.IsNullOrEmpty(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i]))
                            {
                                DeleteResource(MidiLoad.BuildOSPath(MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i]));
                                AssetDatabase.Refresh();
                                ToolsEditor.CheckMidiSet();
                            }
                        }
                        boxY += itemHeight;
                    }
                    GUI.EndScrollView();
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display analyse of midifile
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowMidiAnalyse(float localstartX, float localstartY)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, widthRight, heightList);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                if (ScanInfo != null)
                {
                    Rect listVisibleRect = new Rect(localstartX, localstartY, widthRight, heightList - 5);
                    Rect listContentRect = new Rect(0, 0, widthRight - 15, ScanInfo.Count * heightLine + 5);

                    scrollPosAnalyze = GUI.BeginScrollView(listVisibleRect, scrollPosAnalyze, listContentRect);
                    GUI.color = new Color(.8f, .8f, .8f, 1f);

                    float labelY = -heightLine;
                    foreach (string s in ScanInfo.Infos)
                        EditorGUI.LabelField(new Rect(0, labelY += heightLine, widthRight, heightLine), s, styleRichText);

                    GUI.color = Color.white;

                    GUI.EndScrollView();
                }
                else
                {
                    GUIContent content = new GUIContent() { text = "No Midi file analysed", tooltip = "" };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
        
        /// <summary>
        /// Add a new Midi file from desktop
        /// </summary>
        private static void AddMidifile()
        {
            try
            {
                string selectedFile = EditorUtility.OpenFilePanel("Open and import Midi file", ToolsEditor.lastDirectoryMidi, "mid");
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    ToolsEditor.lastDirectoryMidi = Path.GetDirectoryName(selectedFile);

                    // Build path to midi folder 
                    string pathMidiFile = Path.Combine(Application.dataPath, MidiPlayerGlobal.PathToMidiFile);
                    if (!Directory.Exists(pathMidiFile))
                        Directory.CreateDirectory(pathMidiFile);

                    string filenameToSave = Path.Combine(pathMidiFile, Path.GetFileNameWithoutExtension(selectedFile) + MidiPlayerGlobal.ExtensionMidiFile);
                    // Create a copy of the midi file in resources
                    File.Copy(selectedFile, filenameToSave, true);

                    if (MidiPlayerGlobal.CurrentMidiSet.MidiFiles == null)
                        MidiPlayerGlobal.CurrentMidiSet.MidiFiles = new List<string>();

                    MidiPlayerGlobal.CurrentMidiSet.MidiFiles.Add(Path.GetFileNameWithoutExtension(selectedFile));
                    MidiPlayerGlobal.CurrentMidiSet.Save();
                }
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        static private void DeleteResource(string filepath)
        {
            try
            {
                Debug.Log("Delete " + filepath);
                File.Delete(filepath);
                // delete also meta
                string meta = filepath + ".meta";
                Debug.Log("Delete " + meta);
                File.Delete(meta);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }

}