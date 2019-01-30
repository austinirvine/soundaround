using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MidiPlayerTK
{
    // The position of the window is displayed when it is
    // external from Unity.

    using UnityEngine;
    using UnityEditor;

    public class MenuSelectMidi : EditorWindow
    {
        public static Rect Position;
        public static int Selected;
        private static Vector2 scroller = Vector2.zero;

        private static MenuSelectMidi window;

        //[MenuItem("Examples/Midi")]
       public  static void Init()
        {
            // Get existing open window or if none, make a new one:
            window = (MenuSelectMidi)EditorWindow.GetWindow(typeof(MenuSelectMidi));
            window.titleContent = new GUIContent("Select Midi");

            window.maxSize = new Vector2(260, 600);
            //window.ShowAsDropDown(Position, window.maxSize);
            window.ShowUtility();
        }
      
        void OnGUI()
        {
            scroller = GUILayout.BeginScrollView(scroller,false,false,GUILayout.Width(260));
            BeginWindows();
            //if (Position.x < 0) Position.x = 0;
            //if (Position.y < 0) Position.y = 0;
            //GUILayout.Window(10, new Rect(Position, DefaultSize), DoMyWindow, "Select a Midi",
            //    GUILayout.MaxWidth(Screen.width - Position.x - 30),
            //    GUILayout.ExpandWidth(true),
            //    GUILayout.MaxHeight(Screen.height - Position.y - 30),
            //    GUILayout.ExpandHeight(true));

            int index = -1;
            foreach (string patch in MidiPlayerGlobal.MPTK_ListMidi)
            {
                index++;
                //  if (index % 10 == 0) GUILayout.BeginHorizontal();
                if (GUILayout.Button(patch,GUILayout.Width(200)))
                {
                    Selected = index;
                    //EditorWindow win = GetWindow<Inspec>();
                    //win.SendEvent(EditorGUIUtility.CommandEvent("Paste"));
                    Close();
                }
                //  if (index % 10 == 9) GUILayout.EndHorizontal();
                // if (index > 10) break;
            }
            //GUILayout.EndHorizontal();
            if (GUILayout.Button("Close", GUILayout.Width(200)))
                Close();

            EndWindows();
            GUILayout.EndScrollView();

        }
    }
}
