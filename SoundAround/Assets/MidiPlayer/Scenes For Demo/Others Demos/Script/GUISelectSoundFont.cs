using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;
using System;

namespace MidiPlayerTK
{
    public class GUISelectSoundFont : MonoBehaviour
    {
        static public List<string> SoundFonts = null;

        static public void Display(CustomStyle myStyle)
        {
            if (SoundFonts == null)
                SoundFonts = MidiPlayerGlobal.MPTK_ListSoundFont;

            if (SoundFonts != null)
            {
                GUILayout.Label("Select a soundfont",myStyle.TitleLabel3);
                foreach (string sfName in SoundFonts)
                {
                    GUI.color = Color.white;
                    if (sfName == MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.Name)
                    {
                        GUI.color = new Color(.7f, .9f, .7f, 1f);
                    }
                    if (GUILayout.Button(sfName))
                    {
                        MidiPlayerGlobal.MPTK_SelectSoundFont(sfName);
                    }
                }
                GUI.color = Color.white;
            }
            else
            {
                GUILayout.Label("No Soundfont found");
            }
        }
    }
}