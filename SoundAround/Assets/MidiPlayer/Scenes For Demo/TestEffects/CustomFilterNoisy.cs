using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MidiPlayerTK
{
    public class CustomFilterNoisy : MonoBehaviour
    {
        public bool filter = true;
        public int m_sampleRate;
        public int m_sampleDepth;

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
            if (m_sampleRate <= 0)
                return;

            float avgl, avgr;

            for (int i = 0; i < data.Length - m_sampleRate * channels; i += m_sampleRate * channels)
            {
                avgl = 0.0f;
                avgr = 0.0f;

                for (int j = 0; j < m_sampleRate * channels; j += channels)
                {
                    avgl += data[i + j];

                    if (channels > 1)
                    {
                        avgr += data[i + j + 1];
                    }
                }

                avgl /= (float)m_sampleRate;
                avgr /= (float)m_sampleRate;

                avgl *= m_sampleDepth;
                avgl = Mathf.Round(avgl);
                avgl /= m_sampleDepth;

                avgr *= m_sampleDepth;
                avgr = Mathf.Round(avgr);
                avgr /= m_sampleDepth;

                for (int j = 0; j < m_sampleRate * channels; j += channels)
                {
                    data[i + j] = avgl;
                    data[i + j + 1] = avgr;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
