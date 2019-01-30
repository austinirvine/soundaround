using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace InfinityMusic
{
    public class ScaleDefinition
    {
        public string Name;
        public string Short;
        public int Index;
        public bool Main;
        /// <summary>
        /// 12 position possible correspondant à toutes les notes séparées par un 1/2 ton. Si vide indique note pas utilise dans la gamme
        /// </summary>
        public string[] Position;

        /// <summary>
        /// Inutulisé
        /// </summary>
        public int[] Interval;

        /// <summary>
        /// Ecart en 1/2 ton depuis la tonique. Contient 'Count' ecart
        /// </summary>
        public int[] Ecart;

        /// <summary>
        /// Nombre de notes dans la gamme. 
        /// </summary>
        public int Count;

        public static List<ScaleDefinition> Scales;
        public static List<string> Names=new List<string>();

        public static void Init()
        {
            Scales = new List<ScaleDefinition>();
            TextAsset mytxtData = Resources.Load<TextAsset>("GammeDefinition");
            //string[] sep = { "\r", "\n" };
            //string[] txt = mytxtData.text.Split(sep, StringSplitOptions.None);
            //string[] list1 = mytxtData.text.Split('\r');
            string text = System.Text.Encoding.UTF8.GetString(mytxtData.bytes);
            string[] list1 = text.Split('\r');
            if (list1.Length >= 1)
            {
                for (int i = 1; i < list1.Length; i++)
                {
                    string[] c = list1[i].Split(';');
                    if (c.Length >= 15)
                    {
                        ScaleDefinition scale = new ScaleDefinition();
                        try
                        {
                            scale.Name = c[0];
                            if (scale.Name[0] == '\n') scale.Name = scale.Name.Remove(0, 1);
                            scale.Short = c[1];
                            scale.Index = Convert.ToInt32(c[2]);
                            scale.Main = (c[3].ToUpper() == "X") ? true : false;
                            scale.Count = Convert.ToInt32(c[4]);
                            scale.Position = new string[12];
                            for (int j = 5; j <= 16; j++)
                            {
                                scale.Position[j - 5] = c[j];
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(list1[i] + " " + ex.Message);
                        }

                        Scales.Add(scale);
                        scale.Build();
                    }
                }
            }
            Names = new List<string>();
            foreach (ScaleDefinition sd in Scales)
                Names.Add(sd.Name);
            //Debug.Log("Loaded " + Scales.Count + " scales");
        }
        void Build()
        {
            //CountInterval = Interval.Length;
            Ecart = new int[Count + 1];
            int iEcart = 0;
            int vEcart = 1;
            Ecart[0] = 0;
            iEcart++;
            for (int i = 1; i < Position.Length; i++)
            {
                if (Position[i].Trim().Length == 0)
                {
                    vEcart++;
                }
                else
                {
                    Ecart[iEcart] = vEcart;
                    iEcart++;
                    vEcart += 1;
                }
            }
            Ecart[Ecart.Length - 1] = 12;

            //string info = "Build gamme " + Short + " Count:" + Count + " Ecart:";
            //for (int ecart = 0; ecart < Ecart.Length; ecart++)
            //    info += " " + Ecart[ecart];
            //Debug.Log(info);
        }
    }
}
