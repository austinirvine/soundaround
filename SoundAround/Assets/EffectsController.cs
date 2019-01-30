using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsController : MonoBehaviour {
    public Piano piano;
    GameObject[] tones;
	// Use this for initialization
	void Start () {
        tones = piano.tones;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(GameObject t in tones)
            {
                SinewaveGenerator sine = ((SinewaveGenerator)t.GetComponent("SinewaveGenerator"));
                ((AudioSource)t.GetComponent("AudioSource")).outputAudioMixerGroup = sine.effectAudioGroup;
            }
        }
	}
}
