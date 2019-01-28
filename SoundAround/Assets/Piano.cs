using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piano : MonoBehaviour {
    const double middleC = 261.626;
    const double halfStepMultiplier = 1.0595;
    float[] keyValues = new float[25];
    public GameObject[] tones = new GameObject[25];
	// Use this for initialization
	void Start () {
		for(int i = 0; i <= 24; i++)
        {
            stopFrequency(tones[i]);
            ((SinewaveGenerator)tones[i].GetComponent("SinewaveGenerator")).enabled = false;
            ((SinewaveGenerator)tones[i].GetComponent("SinewaveGenerator")).createAudioSource();

        }
        //for (int i = 0; i <= 24; i++)
        //{
        //    stopFrequency(tones[i]);
        //    ((SinewaveGenerator)tones[i].GetComponent("SinewaveGenerator")).enabled = false;
        //}
    }

    void playFrequency(double freq, GameObject tone)
    {
        Debug.Log("Starting source " + tone);
        SinewaveGenerator sine = (SinewaveGenerator)tone.GetComponent("SinewaveGenerator");
        sine.frequency1 = (float)freq;
        sine.enabled = true;
        AudioSource source = (AudioSource)tone.GetComponent("AudioSource");
        source.Play();
        // sine.togglePlaying();
    }

    void stopFrequency(GameObject tone)
    {
        SinewaveGenerator sine = (SinewaveGenerator)tone.GetComponent("SinewaveGenerator");
        Debug.Log("Stopping source " + tone);
        sine.enabled = false;
       // sine.togglePlaying();
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
                playFrequency(freq, tones[i-48]);
            }
            else if (keyReleased)
            {
                stopFrequency(tones[i - 48]);
            }
            //else
            //{
            //   if (MidiJack.MidiMaster.GetKey(MidiJack.MidiChannel.All, i) == 0)
            //    {
            //        stopFrequency(tones[i - 48]);
            //    }
            //}

        }
	}
}
