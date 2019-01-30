using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    public class CustomFilterSin : MonoBehaviour
    {
        public bool filter = true;
        // un-optimized version
        [Range(1,20000)]
        public double frequency = 440;

        [Range(-1f, 1f)]
        public double gain = 0.05;

        private double increment;
        private double phase;
        private double sampling_frequency = 48000;
        // Use this for initialization
        void Start()
        {

        }

        // http://www.mclimatiano.com/audio-filters-in-unity3d/
        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!filter)
                return;
            if (channels <= 0)
                return;
            // update increment in case frequency has changed
            increment = frequency * 2 * Math.PI / sampling_frequency;
            for (var i = 0; i < data.Length; i = i + channels)
            {
                phase = phase + increment;
                // this is where we copy audio data to make them “available” to Unity
                data[i] = (float)(gain * Math.Sin(phase));
                // if we have stereo, we copy the mono data to each channel
                if (channels == 2) data[i+1] = data[i];
                if (phase > 2 * Math.PI) phase = 0;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
