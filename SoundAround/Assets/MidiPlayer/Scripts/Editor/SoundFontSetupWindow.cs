//#define MPTK_PRO
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
    public class SoundFontSetupWindow : EditorWindow
    {
        private static SoundFontSetupWindow window;

        Vector2 scrollPosBanks = Vector2.zero;
        Vector2 scrollPosSoundFont = Vector2.zero;
        Vector2 scrollPosListPatchs = Vector2.zero;


        static float widthLeft;
        static float widthRight;

        static float heightTop;
        static float heightMiddle;
        static float heightBottom;

        static int itemHeight;
        static int buttonWidth;
        static int buttonHeight;
        static float espace;

        static float xpostitlebox;
        static float ypostitlebox;

        static GUIStyle styleBold;
        static GUIStyle styleRed;
        static GUIStyle styleRichText;
        static float heightLine;

#if MPTK_PRO
        static BuilderInfo OptimInfo;
        Vector2 scrollPosOptim = Vector2.zero;
#endif
        static bool KeepAllPatchs = false;
        static bool KeepAllZones = false;
        static bool RemoveUnusedWaves = true;

        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        [MenuItem("Tools/MPTK - SoundFont Setup &F")]
        public static void Init()
        {
            // Get existing open window or if none, make a new one:
            try
            {
                window = (SoundFontSetupWindow)EditorWindow.GetWindow(typeof(SoundFontSetupWindow));
                window.titleContent.text = "SoundFont Setup";
                window.minSize = new Vector2(828 + 65, 565);

                styleBold = new GUIStyle(EditorStyles.boldLabel);
                styleBold.fontStyle = FontStyle.Bold;


                styleRed = new GUIStyle(EditorStyles.label);
                styleRed.normal.textColor = new Color(0.5f, 0, 0);
                styleRed.fontStyle = FontStyle.Bold;

                styleRichText = new GUIStyle(EditorStyles.label);
                styleRichText.richText = true;
                styleRichText.alignment = TextAnchor.UpperLeft;
                heightLine = styleRichText.lineHeight * 1.2f;

                espace = 5;
                widthLeft = 415 + 25;
                heightTop = 130;
                heightMiddle = 150;
                itemHeight = 25;
                buttonWidth = 200;
                buttonHeight = 18;

                xpostitlebox = 2;
                ypostitlebox = 5;
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        //        private void OnLostFocus()
        //        {
        //#if UNITY_2017_1_OR_NEWER
        //            // Trig an  error before v2017...
        //            if (Application.isPlaying)
        //            {
        //                window.Close();
        //            }
        //#endif
        //        }

        /// <summary>
        /// Reload data
        /// </summary>
        private void OnFocus()
        {
            // Load description of available soundfont
            try
            {
                ToolsEditor.LoadMidiSet();
                ToolsEditor.CheckMidiSet();
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    KeepAllPatchs = MidiPlayerGlobal.ImSFCurrent.KeepAllPatchs;
                    KeepAllZones = MidiPlayerGlobal.ImSFCurrent.KeepAllZones;
                    RemoveUnusedWaves = MidiPlayerGlobal.ImSFCurrent.RemoveUnusedWaves;
                }
                // cause catch if call when playing (setup open on run mode)
                if (!Application.isPlaying)
                    AssetDatabase.Refresh();
                // Exec after Refresh, either cause errror
                ToolsEditor.LoadImSF();
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

                GUIContent content = new GUIContent() { text = "Setup SoundFont in your application - Version " + ToolsEditor.version, tooltip = "" };
                EditorGUI.LabelField(new Rect(startx, starty, 500, itemHeight), content, styleBold);

                GUI.color = ToolsEditor.ButtonColor;
                content = new GUIContent() { text = "Help & Contact", tooltip = "Get some help" };
                // Set position of the button
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
                heightBottom = window.position.size.y - heightTop - heightMiddle - 3 * espace - starty;

                // Display list of soundfont already loaded
                ShowListSoundFonts(startx, starty, widthLeft, heightTop);

                ShowListBanks(startx + widthLeft + espace, starty, widthRight, heightBottom);

                ShowOptim(startx, starty + espace + heightTop, widthLeft, heightMiddle + heightBottom + espace);

                ShowLogOptim(startx + widthLeft + espace, starty + heightBottom + espace, widthRight, heightMiddle + heightTop + espace);

            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display, add, remove Soundfont
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowListSoundFonts(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;
                if (MidiPlayerGlobal.CurrentMidiSet != null && MidiPlayerGlobal.CurrentMidiSet.SoundFonts != null)
                {
                    string caption = "SoundFont available";
                    if (MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count == 0)
                    {
                        caption = "No SoundFont available yet";
                        MidiPlayerGlobal.ImSFCurrent = null;
                    }

                    GUIContent content = new GUIContent() { text = caption, tooltip = "Each SoundFonts contains a set of bank of sound. \nOnly one SoundFont can be active at the same time for the midi player" };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                    Rect rect = new Rect(width - buttonWidth, localstartY + ypostitlebox, buttonWidth, buttonHeight);
#if MPTK_PRO
                    if (GUI.Button(rect, "Add SoundFont"))
                    {
                        //if (EditorUtility.DisplayDialog("Import SoundFont", "This action could take time, do you confirm ?", "Ok", "Cancel"))
                        {
                            SoundFontOptim.AddSoundFont();
                            scrollPosSoundFont = Vector2.zero;
                            KeepAllPatchs = false;
                            KeepAllZones = false;
                            //listPatchs = PatchOptim.PatchUsed();
                        }
                    }
#else
                    if (GUI.Button(rect, "Add SoundFont [PRO]"))
                    {
                        try
                        {
                            PopupWindow.Show(rect, new GetVersionPro());

                        }
                        catch (Exception)
                        {
                            // generate some weird exception ...
                        }
                    }
#endif

                    Rect listVisibleRect = new Rect(localstartX, localstartY + itemHeight, width - 5, height - itemHeight - 5);
                    Rect listContentRect = new Rect(0, 0, width - 20, MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count * itemHeight + 5);

                    scrollPosSoundFont = GUI.BeginScrollView(listVisibleRect, scrollPosSoundFont, listContentRect);
                    float boxY = 0;

                    for (int i = 0; i < MidiPlayerGlobal.CurrentMidiSet.SoundFonts.Count; i++)
                    {
                        SoundFontInfo sf = MidiPlayerGlobal.CurrentMidiSet.SoundFonts[i];

                        GUI.color = new Color(.7f, .7f, .7f, 1f);
                        float boxX = 5;
                        GUI.Box(new Rect(boxX, boxY + 5, width - 30, itemHeight), "");
                        GUI.color = Color.white;

                        content = new GUIContent() { text = sf.Name, tooltip = "" };
                        EditorGUI.LabelField(new Rect(boxX, boxY + 9, 200, itemHeight), content);

                        if (sf.Name == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                        {
                            GUI.color = ToolsEditor.ButtonColor;
                        }

                        boxX += 200 + espace;
                        if (GUI.Button(new Rect(boxX, boxY + 9, 80, buttonHeight), "Select"))
                        {
#if MPTK_PRO
                            OptimInfo = new BuilderInfo();
#endif
                            MidiPlayerGlobal.CurrentMidiSet.SetActiveSoundFont(i);
                            string soundPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                            soundPath = Path.Combine(soundPath + "/", sf.Name);
                            ToolsEditor.LoadImSF(soundPath, sf.Name);
                            MidiPlayerGlobal.CurrentMidiSet.Save();
                            if (MidiPlayerGlobal.ImSFCurrent != null)
                            {
                                KeepAllPatchs = MidiPlayerGlobal.ImSFCurrent.KeepAllPatchs;
                                KeepAllZones = MidiPlayerGlobal.ImSFCurrent.KeepAllZones;
                                RemoveUnusedWaves = MidiPlayerGlobal.ImSFCurrent.RemoveUnusedWaves;
                                if (Application.isPlaying)
                                {
                                    MidiPlayerGlobal.MPTK_SelectSoundFont(null);
                                }
                            }
                            //listPatchs = PatchOptim.PatchUsed();
                        }
                        boxX += 80 + espace;

                        GUI.color = Color.white;
                        rect = new Rect(boxX, boxY + 9, 80, buttonHeight);
                        if (GUI.Button(rect, "Remove"))
                        {
#if MPTK_PRO
                            OptimInfo = new BuilderInfo();
                            string soundFontPath = Path.Combine(Application.dataPath + "/", MidiPlayerGlobal.PathToSoundfonts);
                            string path = Path.Combine(soundFontPath, sf.Name);
                            if (!string.IsNullOrEmpty(path) && EditorUtility.DisplayDialog("Delete SoundFont", "Are you sure to delete all the content of this folder ? " + path, "ok", "cancel"))
                            {
                                try
                                {
                                    Directory.Delete(path, true);
                                    File.Delete(path + ".meta");

                                }
                                catch (Exception ex)
                                {
                                    Debug.Log("Remove SF " + ex.Message);
                                }
                                AssetDatabase.Refresh();
                                ToolsEditor.CheckMidiSet();
                            }
#else
                            try
                            {
                                PopupWindow.Show(rect, new GetVersionPro());
                            }
                            catch (Exception)
                            {
                                // generate some weird exception ...
                            }
#endif
                        }

                        GUI.color = Color.white;

                        //boxX = 5;
                        //boxY += itemHeight;
                        //if (MidiPlayerGlobal.ImSFCurrent.WaveSize < 1000000)
                        //    strSize = Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000d).ToString() + " Ko";
                        //else
                        //    strSize = Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000000d).ToString() + " Mo";
                        //string.Format("Patch count: {0} Wave count:{1} Wave size:{2}", MidiPlayerGlobal.ImSFCurrent.PatchCount, MidiPlayerGlobal.ImSFCurrent.WaveCount, strSize);

                        //content = new GUIContent() { text = sf.Name, tooltip = "" };
                        //EditorGUI.LabelField(new Rect(boxX, boxY + 9, 200, itemHeight), content);

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



        private void ShowListBanks(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                if (MidiPlayerGlobal.ImSFCurrent != null && MidiPlayerGlobal.ImSFCurrent.Banks != null)
                {
                    string tooltip = "Each bank contains a set of patchs (instrument).\nOnly two banks can be active at the same time : default sound (piano, ...) and drum kit (percussive)";
                    GUIContent content = new GUIContent() { text = "Banks available in SoundFont " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = tooltip };
                    //GUIContent content = new GUIContent() { text = "Banks available in SoundFont ", tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, width, itemHeight), content, styleBold);

                    //if (GUI.Button(new Rect(localstartX + width - buttonWidth - espace, localstartY + ypostitlebox, buttonWidth, buttonHeight), new GUIContent() { text = "2) Removed not used", tooltip = tooltip }))
                    //{
                    //    SoundFontOptim.OptimizeBanks(MidiPlayerGlobal.ImSFCurrent);
                    //    MidiPlayerGlobal.CurrentMidiSet.Save();
                    //}

                    // Count available banks
                    int countBank = 0;
                    foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        if (bank != null) countBank++;
                    Rect listVisibleRect = new Rect(localstartX, localstartY + itemHeight, width, height - itemHeight - 5);
                    Rect listContentRect = new Rect(0, 0, width - 15, countBank * itemHeight + 5);

                    scrollPosBanks = GUI.BeginScrollView(listVisibleRect, scrollPosBanks, listContentRect);

                    float boxY = 0;
                    SoundFontInfo sfi = MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo;
                    if (sfi != null)
                    {
                        foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                        {
                            if (bank != null)
                            {
                                GUI.color = new Color(.7f, .7f, .7f, 1f);
                                GUI.Box(new Rect(5, boxY + 5, width - 25, itemHeight), "");

                                GUI.color = Color.white;

                                content = new GUIContent() { text = string.Format("Bank [{0,3:000}] Patch:{1}", bank.BankNumber, bank.PatchCount), tooltip = bank.Description };
                                GUI.Label(new Rect(10, boxY + 9, 130, itemHeight), content);

                                //Debug.Log(sfi.DefaultBankNumber );
                                if (sfi.DefaultBankNumber == bank.BankNumber) GUI.color = ToolsEditor.ButtonColor;
                                if (GUI.Button(new Rect(155, boxY + 9, 120, buttonHeight), new GUIContent("Default Bank", "Select this bank to be used for all instruments except drum")))
                                {
                                    sfi.DefaultBankNumber = sfi.DefaultBankNumber != bank.BankNumber ? bank.BankNumber : -1;
                                    MidiPlayerGlobal.ImSFCurrent.DefaultBankNumber = bank.BankNumber;
                                    MidiPlayerGlobal.CurrentMidiSet.Save();
                                }
                                GUI.color = Color.white;

                                if (sfi.DrumKitBankNumber == bank.BankNumber) GUI.color = ToolsEditor.ButtonColor;
                                if (GUI.Button(new Rect(155+120+5, boxY + 9, 120, buttonHeight), new GUIContent("Drum Bank", "Select this bank to be used for playing drum hit")))
                                {
                                    sfi.DrumKitBankNumber = sfi.DrumKitBankNumber != bank.BankNumber ? bank.BankNumber : -1;
                                    MidiPlayerGlobal.ImSFCurrent.DrumKitBankNumber = bank.BankNumber;
                                    MidiPlayerGlobal.CurrentMidiSet.Save();
                                }
                                GUI.color = Color.white;
                                boxY += itemHeight;
                            }
                        }
                    }

                    GUI.EndScrollView();
                }
                else
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), "No SoundFont selected", styleBold);
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display optimization
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;

                string tooltip = "Remove all banks and Presets not used in the Midi file list";

                GUIContent content;
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    float xpos = localstartX + xpostitlebox;
                    float ypos = localstartY + ypostitlebox;
                    content = new GUIContent() { text = "Extract Patchs & Waves from " + MidiPlayerGlobal.ImSFCurrent.SoundFontName, tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), content, styleBold);
                    ypos += itemHeight;// + espace;

                    if (MidiPlayerGlobal.ImSFCurrent.PatchCount == 0 || MidiPlayerGlobal.ImSFCurrent.WaveCount == 0)
                    {
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), "No patchs and waves has been yet extracted from the Soundfont.", styleRed);
                        ypos += itemHeight / 1f;// + espace;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), "On the right panel, select one bank for instruments and", styleRed);
                        ypos += itemHeight / 2f;// + espace;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), "optional, one bank for drums kit, others banks will be removed.", styleRed);
                        ypos += itemHeight / 1f;// + espace;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), "Then, choose between 'Optimize from Midi file list'", styleRed);
                        ypos += itemHeight / 2f;// + espace;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), "or 'Extract all Patchs & Waves' with the buttons below.", styleRed);

                        ypos += itemHeight;// + espace;
                    }
                    else
                    {
                        string infoSf = string.Format("Patch count: {0}   Wave count:{1}   Wave size:{2}",
                            MidiPlayerGlobal.ImSFCurrent.PatchCount,
                            MidiPlayerGlobal.ImSFCurrent.WaveCount,
                             (MidiPlayerGlobal.ImSFCurrent.WaveSize < 1000000) ?
                                Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000d).ToString() + " Ko"
                                :
                                Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000000d).ToString() + " Mo"
                                );
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380 + 85, itemHeight), infoSf);

                        ypos += 2 * heightLine;
                    }
                    int widthCheck = 110;
                    /*
                    KeepAllZones = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllZones, new GUIContent("Keep all Zones", "Keep all Waves associated with a Patch regardless of notes and velocities played in Midi files.\n Usefull if you want transpose Midi files."));
                    xpos += widthCheck + espace;
                    KeepAllPatchs = GUI.Toggle(new Rect(xpos, ypos, widthCheck, itemHeight), KeepAllPatchs, new GUIContent("Keep all Patchs", "Keep all Patchs and waves found in the SoundFont selected.\nWarning : a huge volume of files coud be created"));
                    xpos += widthCheck + +2 * espace;
                    RemoveUnusedWaves = GUI.Toggle(new Rect(xpos, ypos, widthCheck + 40, itemHeight), RemoveUnusedWaves, new GUIContent("Remove unused waves", "If check, keep only waves used by your midi files"));
                    ypos += itemHeight;// + espace;
                    */

                    // Always true
                    RemoveUnusedWaves = true;
                    xpos = localstartX + xpostitlebox;
                    Rect rect = new Rect(xpos, ypos, widthCheck * 2 + espace, buttonHeight);
#if MPTK_PRO
                    if (GUI.Button(rect, new GUIContent("Optimize from Midi file list", "Your list of Midi files will be scanned to identify patchs and zones useful")))
                    {
                        KeepAllPatchs = false;
                        KeepAllZones = false;
                        OptimInfo = new BuilderInfo();
                        SoundFontOptim.OptimizeSFFromMidiFiles(OptimInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                        //listPatchs = PatchOptim.PatchUsed();
                    }

                    xpos += 3 * espace + 2 * widthCheck;
                    rect = new Rect(xpos, ypos, buttonWidth, buttonHeight);
                    if (GUI.Button(rect, new GUIContent("Extract all Patchs & Waves", "All patchs and waves will be extracted from the Soundfile")))
                    {
                        KeepAllPatchs = true;
                        KeepAllZones = true;
                        OptimInfo = new BuilderInfo();
                        SoundFontOptim.OptimizeSFFromMidiFiles(OptimInfo, KeepAllPatchs, KeepAllZones, RemoveUnusedWaves);
                        //listPatchs = PatchOptim.PatchUsed();
                    }
#else
                    if (GUI.Button(rect, new GUIContent("Optimize from Midi file list [PRO]", "You need to setup some midi files before to launch ths optimization")))
                        PopupWindow.Show(rect, new GetVersionPro());
                    xpos += 3 * espace + 2 * widthCheck;
                    rect = new Rect(xpos, ypos, buttonWidth, buttonHeight);
                    if (GUI.Button(rect, new GUIContent("Extract all Patchs & Waves [PRO]", "")))
                        PopupWindow.Show(rect, new GetVersionPro());

#endif
                    ypos += itemHeight + espace;


                    if (MidiPlayerGlobal.ImSFCurrent.PatchCount != 0 && MidiPlayerGlobal.ImSFCurrent.WaveCount != 0)
                    {

                        //
                        // Show patch list
                        //
                        xpos = localstartX + xpostitlebox;
                        EditorGUI.LabelField(new Rect(xpos, ypos, 380, itemHeight), "List of available Patchs:");
                        ypos += itemHeight / 2f;
                        List<string> patchList = new List<string>();
                        for (int i = 0; i < MidiPlayerGlobal.MPTK_ListPreset.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(MidiPlayerGlobal.MPTK_ListPreset[i]))
                                patchList.Add(string.Format("Instrument - Patch: [{0:000}] {1}", i, MidiPlayerGlobal.MPTK_ListPreset[i]));
                        }
                        for (int i = 0; i < MidiPlayerGlobal.MPTK_ListDrum.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(MidiPlayerGlobal.MPTK_ListDrum[i]))
                                patchList.Add(string.Format("Drum - Midi note: [{0:000}] {1}", i, MidiPlayerGlobal.MPTK_ListDrum[i]));
                        }

                        float heightAvailable = height - (ypos - (localstartY + ypostitlebox)) - 16;
                        Rect listVisibleRect = new Rect(localstartX, ypos, width - 5, heightAvailable);
                        GUI.color = new Color(.8f, .8f, .8f, 1f);
                        GUI.Box(new Rect(localstartX + 5, ypos + 5, width - 30, heightAvailable - 4), "");
                        Rect listContentRect = new Rect(0, 0, width - 20, patchList.Count * itemHeight + 5);

                        scrollPosListPatchs = GUI.BeginScrollView(listVisibleRect, scrollPosListPatchs, listContentRect);
                        float boxY = 0;

                        foreach (string patchInfo in patchList)
                        {
                            GUI.color = new Color(.7f, .7f, .7f, 1f);
                            float boxX = 5;
                            GUI.Box(new Rect(boxX, boxY + 5, width - 30, itemHeight), "");
                            GUI.color = Color.white;
                            EditorGUI.LabelField(new Rect(boxX, boxY + 9, 300, itemHeight), patchInfo);
                            boxY += itemHeight;
                        }
                        GUI.EndScrollView();


                        ypos += heightAvailable + heightLine;



                        //List<PatchOptim> listPatchs = PatchOptim.PatchUsed();

                        //float heightAvailable = height - (ypos - (localstartY + ypostitlebox)) - 15 - 4 * heightLine;
                        //Rect listVisibleRect = new Rect(localstartX, ypos, width - 5, heightAvailable);
                        //Rect listContentRect = new Rect(0, 0, width - 20, listPatchs.Count * itemHeight + 5);

                        //scrollPosListPatchs = GUI.BeginScrollView(listVisibleRect, scrollPosListPatchs, listContentRect);
                        //float boxY = 0;

                        //for (int i = 0; i < listPatchs.Count; i++)
                        //{
                        //    GUI.color = new Color(.7f, .7f, .7f, 1f);
                        //    float boxX = 5;
                        //    GUI.Box(new Rect(boxX, boxY + 5, width - 30, itemHeight), "");
                        //    GUI.color = Color.white;
                        //    PatchOptim patch = listPatchs[i];
                        //    string patchinfo = string.Format("{0,15} [{1:000}] {2}", patch.Drum?"Drum":"Instrument", patch.Patch, patch.Name);
                        //    //content = new GUIContent() { text = patchinfo, tooltip = MidiPlayerGlobal.CurrentMidiSet.MidiFiles[i] };
                        //    EditorGUI.LabelField(new Rect(boxX, boxY + 9, 200, itemHeight), patchinfo);

                        //    //boxX += 280 + espace;
                        //    //patch.Selected = GUI.Toggle(new Rect(boxX, boxY + 9, 80, buttonHeight), patch.Selected, "Selected");

                        //    boxY += itemHeight;
                        //}
                        //GUI.EndScrollView();
                        //ypos += heightAvailable + heightLine;
                    }
                    //
                    // Show stat
                    //
                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, ypos, 100, heightLine), "Patch count :");
                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox + 110, ypos, 100, heightLine), MidiPlayerGlobal.ImSFCurrent.PatchCount.ToString());
                    //ypos += heightLine;

                    //string strSize = "";
                    //if (MidiPlayerGlobal.ImSFCurrent.WaveSize < 1000000)
                    //    strSize = Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000d).ToString() + " Ko";
                    //else
                    //    strSize = Math.Round((double)MidiPlayerGlobal.ImSFCurrent.WaveSize / 1000000d).ToString() + " Mo";

                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, ypos, 100, heightLine), "Wave count :");
                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox + 110, ypos, 100, heightLine), MidiPlayerGlobal.ImSFCurrent.WaveCount.ToString());
                    //ypos += heightLine;

                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, ypos, 100, heightLine), "Wave size :");
                    //EditorGUI.LabelField(new Rect(localstartX + xpostitlebox + 110, ypos, 100, heightLine), strSize);
                    //ypos += heightLine;


                }
                else
                {
                    content = new GUIContent() { text = "No SoundFont selected", tooltip = tooltip };
                    EditorGUI.LabelField(new Rect(localstartX + xpostitlebox, localstartY + ypostitlebox, 300, itemHeight), content, styleBold);
                }
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }

        /// <summary>
        /// Display optimization log
        /// </summary>
        /// <param name="localstartX"></param>
        /// <param name="localstartY"></param>
        private void ShowLogOptim(float localstartX, float localstartY, float width, float height)
        {
            try
            {
                Rect zone = new Rect(localstartX, localstartY, width, height);
                GUI.color = new Color(.8f, .8f, .8f, 1f);
                GUI.Box(zone, "");
                GUI.color = Color.white;
#if MPTK_PRO

                if (OptimInfo != null)
                {
                    Rect listVisibleRect = new Rect(localstartX, localstartY + espace, width - 5, height - 10);
                    Rect listContentRect = new Rect(0, 0, width - 20, OptimInfo.Count * heightLine + 5);

                    scrollPosOptim = GUI.BeginScrollView(listVisibleRect, scrollPosOptim, listContentRect);
                    GUI.color = Color.white;
                    float labelY = -heightLine;
                    foreach (string s in OptimInfo.Infos)
                        EditorGUI.LabelField(new Rect(0, labelY += heightLine, width, heightLine), s, styleRichText);
                    GUI.EndScrollView();
                }
#endif
            }
            catch (Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}