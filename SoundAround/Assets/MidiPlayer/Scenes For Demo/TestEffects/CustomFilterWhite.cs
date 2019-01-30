using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    public class CustomFilterWhite : MonoBehaviour
    {
        private System.Random RandomNumber = new System.Random();
        public bool filter = true;

        [Range(-1f, 1f)]
        public float offset = 0;

        // Use this for initialization
        void Start()
        {

        }

        //https://www.mcvuk.com/development/procedural-audio-with-unity
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!filter)
                return;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = offset - 1.0f + (float)RandomNumber.NextDouble() * 2.0f;
            }
        }
    }
}
