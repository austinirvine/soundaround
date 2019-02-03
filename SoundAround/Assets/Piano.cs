using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;

public class Piano : MonoBehaviour {
	// Use this for initialization
	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        for (int i = 48; i <= 72; i++)
        {
            int aboveMiddleC = i - 47;
            bool keyPressed = MidiJack.MidiMaster.GetKeyDown(MidiJack.MidiChannel.All, i);
            bool keyReleased = MidiJack.MidiMaster.GetKeyUp(MidiJack.MidiChannel.All, i);
            if (keyPressed)
            {
                Debug.Log("Playing " + aboveMiddleC);
                MPTKNote note = new MPTKNote() { Delay = 0,
                    Drum = false,
                    Duration = 300,
                    Note = aboveMiddleC,
                    Patch = 1,
                    Velocity = 115 };
                MidiStreamPlayer player = FindObjectOfType<MidiStreamPlayer>();
                note.Play(player);
            }
        }
    }
}
