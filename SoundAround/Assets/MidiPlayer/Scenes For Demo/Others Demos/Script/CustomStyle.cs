using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MidiPlayerTK
{
    public class CustomStyle
    {
        public GUIStyle BackgWindow;
        public GUIStyle BackgZone;
        public GUIStyle TextFieldMultiLine;
        public GUIStyle VScroll;
        public GUIStyle HScroll;
        public GUIStyle TitleLabel1;
        public GUIStyle TitleLabel2;
        public GUIStyle TitleLabel3;
        public GUIStyle LabelCentered;
        public GUIStyle KeyWhite;
        public GUIStyle KeyBlack;
        public GUIStyle BtStandard;
        public GUIStyle BtSelected;
        public GUIStyle ItemSelected;
        public GUIStyle ItemNotSelected;
        public GUIStyle DragZone;
        public GUIStyle LabelZone;
        public GUIStyle LabelZoneCentered;
        public GUIStyle LabelTitle;
        public float heightLine;

        public Color ButtonColor = new Color(.7f, .9f, .7f, 1f);

        /// <summary>
        /// Set custom Style. Good for background color 3E619800
        /// </summary>
        public CustomStyle()
        {
            BtStandard = new GUIStyle("Button");

            BtSelected = new GUIStyle("Button");
            BtSelected.fontStyle = FontStyle.Bold;
            BtSelected.normal.textColor = new Color(0.5f, 0.9f, 0.5f);
            BtSelected.hover.textColor = BtSelected.normal.textColor;
            BtSelected.active.textColor = BtSelected.normal.textColor;

            BackgWindow = new GUIStyle("window");
            //tex = new Texture2D(2, 2);
            //tex = SetColor(tex, new Color(0.3f, 0.3f, 0.3f));
            //styleBg.normal.background = tex;

            BackgZone = new GUIStyle("textField");
            
            VScroll = new GUIStyle("verticalScrollbar");

            HScroll = new GUIStyle("horizontalScrollbar");

            TitleLabel1 = new GUIStyle("label");
            TitleLabel1.fontSize = 20;
            TitleLabel1.alignment = TextAnchor.MiddleLeft;

            TitleLabel2 = new GUIStyle("label");
            TitleLabel2.fontSize = 16;

            TitleLabel3 = new GUIStyle("label");
            TitleLabel3.fontSize = 14;

            KeyWhite = new GUIStyle("Button");
            KeyWhite.normal.background = SetColor(new Texture2D(2, 2), new Color(0.9f, 0.9f, 0.9f));
            KeyWhite.normal.textColor = new Color(0.1f, 0.1f, 0.1f);
            KeyWhite.alignment = TextAnchor.UpperCenter;
            KeyWhite.fontSize = 8;

            KeyBlack = new GUIStyle("Button");
            KeyBlack.normal.background = SetColor(new Texture2D(2, 2), new Color(0.1f, 0.1f, 0.1f));
            KeyBlack.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            KeyBlack.alignment = TextAnchor.UpperCenter;
            KeyBlack.fontSize = 8;

            LabelCentered = new GUIStyle("Label");
            LabelCentered.alignment = TextAnchor.MiddleCenter;

            TextFieldMultiLine = new GUIStyle("textField");
            TextFieldMultiLine.alignment = TextAnchor.UpperLeft;
            TextFieldMultiLine.wordWrap = true;

            LabelZone = new GUIStyle("textField");
            LabelZone.alignment = TextAnchor.MiddleLeft;
            LabelZone.normal.background= SetColor(new Texture2D(2, 2), new Color(.6f, .8f, .6f, 1f));
            LabelZone.normal.textColor = Color.black;
            LabelZone.wordWrap = true;
            LabelZone.fontSize = 14;

            LabelZoneCentered = new GUIStyle("textField");
            LabelZoneCentered.alignment = TextAnchor.MiddleCenter;
            LabelZoneCentered.normal.background = SetColor(new Texture2D(2, 2), new Color(.6f, .8f, .6f, 1f));
            LabelZoneCentered.normal.textColor = Color.black;
            LabelZoneCentered.wordWrap = true;
            LabelZoneCentered.fontSize = 14;

            LabelTitle = new GUIStyle("textField");
            LabelTitle.alignment = TextAnchor.MiddleCenter;
            LabelTitle.normal.background = SetColor(new Texture2D(2, 2), new Color(.4f, .6f, .4f, 1f));
            LabelTitle.normal.textColor = Color.black;
            LabelTitle.wordWrap = true;
            LabelTitle.fontSize = 14;

            DragZone = new GUIStyle("textArea");
            DragZone.normal.textColor = new Color(0, 0, 0.99f);
            DragZone.alignment = TextAnchor.MiddleCenter;
            //styleDragZone.border = new RectOffset(2, 2, 2, 2);

            ItemSelected = new GUIStyle("label");
            ItemSelected.normal.background = SetColor(new Texture2D(2, 2), ButtonColor);

            ItemNotSelected = new GUIStyle("label");

            heightLine = GUI.skin.button.lineHeight * 1.2f;

        }
        /// <summary>
        /// Used to define color of GUI
        /// </summary>
        /// <param name="tex2"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public Texture2D SetColor(Texture2D tex2, Color32 color)
        {
            var fillColorArray = tex2.GetPixels32();
            for (var i = 0; i < fillColorArray.Length; ++i)
                fillColorArray[i] = color;
            tex2.SetPixels32(fillColorArray);
            tex2.Apply();
            return tex2;
        }
    }
}
