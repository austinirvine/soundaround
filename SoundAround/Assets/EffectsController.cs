using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class EffectsController : MonoBehaviour {
    public Piano piano;
    public AudioSource audioSourceForEffect;
    public AudioMixerGroup effectGroup;
    public AudioMixerGroup cleanGroup;
    bool isClean = true;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            if (isClean)
            {
                Debug.Log("Effect on");
                foreach (AudioSource src in sources){
                    src.outputAudioMixerGroup = effectGroup;
                }
                isClean = false;
            }
            else
            {
                Debug.Log("Effect off");
                foreach (AudioSource src in sources)
                {
                    src.outputAudioMixerGroup = cleanGroup;
                }
                isClean = true;
            }
        }
	}
}
