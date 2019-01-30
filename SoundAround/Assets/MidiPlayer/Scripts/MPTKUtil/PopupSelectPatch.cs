using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    public class PopupSelectPatch
    {
        public Vector2 Position;
        public Vector2 DefaultSize = new Vector2(800, 300);// Default size
        public Rect RealRect;
        public bool DispatchPopupPatch = false;
        public int Selected;
        private Vector2 scroller;

        public int Draw(CustomStyle myStyle)
        {
            if (DispatchPopupPatch)
            {
                if (Position.x < 0) Position.x = 0;
                if (Position.y < 0) Position.y = 0;
                GUILayout.Window(10, new Rect(Position, DefaultSize), DoMyWindow, "Select a patch",
                    myStyle.BackgWindow,
                    GUILayout.MaxWidth(Screen.width - Position.x - 30),
                    GUILayout.ExpandWidth(true),
                    GUILayout.MaxHeight(Screen.height - Position.y - 30),
                    GUILayout.ExpandHeight(true));
                //Debug.Log(Screen.height - position.y - 30);
            }
            return Selected;
        }

        void DoMyWindow(int windowID)
        {
            //(new GUIStyle("button")).

            scroller = GUILayout.BeginScrollView(scroller, false, false);//, GUILayout.Width(size.x));//, GUILayout.Height(size.y));
            int index = -1;
            //GUILayout.BeginHorizontal();
            foreach (string patch in MidiPlayerGlobal.MPTK_ListPreset)
            {
                index++;
                if (index % 10 == 0) GUILayout.BeginHorizontal();
                //New1.95
                if (GUILayout.Button(index.ToString() + ":" + patch))
                {
                    Selected = index;
                    DispatchPopupPatch = false;
                }
                if (index % 10 == 9) GUILayout.EndHorizontal();
                // if (index > 10) break;
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Close"))
                DispatchPopupPatch = false;
            GUILayout.EndScrollView();
            Event e = Event.current;
            if (e.type == EventType.Repaint)
            {
                RealRect = GUILayoutUtility.GetLastRect();
                //Debug.Log(lastRect);
            }
            //Debug.Log("Got a click");
        }
    }
}
