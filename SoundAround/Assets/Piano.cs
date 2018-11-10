using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour {
    const double middleC = 261.626;
    const double halfStepMultiplier = 1.0595;
    float[] keyValues = new float[25];
    public SinewaveGenerator sine;
    public AudioSource source;
	// Use this for initialization
	void Start () {
		
	}

    void playFrequency(double freq)
    {
        sine.frequency1 = (float)freq;
        source.Play();
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 48; i <= 72; i++)
        {
            int aboveMiddleC = i - 47;
            double freq = middleC;
            for(int j = 1; j < aboveMiddleC; j++)
            {
                freq *= 1.0595;
            }
            bool keyPressed = MidiJack.MidiMaster.GetKeyDown(MidiJack.MidiChannel.All, i);
            bool keyReleased = MidiJack.MidiMaster.GetKeyUp(MidiJack.MidiChannel.All, i);
            if (keyPressed)
            {
                Debug.Log(aboveMiddleC);
                playFrequency(freq);
            }
            if (keyReleased)
            {
                source.Stop();
            }

        }
	}
}
