using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour {

	private AudioFile chosen = null;
	private AudioFile last_chosen = null;
	private float track_pos = 0.0f;
	private enum audio_state {stop=0,pause=1,play=2};

	public GameObject sound_holster;
	public GameObject sound_player_obj;

	// void PlayButton () {
	// 	//check if current sound has changed
	// 	///if so adjust set the track_pos to 0.0f
	// 	chosen = sound_holster.GetTopSong();
	// 	if (last_chosen == null) {
	// 		last_chosen = chosen;
	// 	}

	// 	if (last_chosen != chosen) {
	// 		track_pos = 0.0f;
	// 		last_chosen = chosen;
	// 	}
	// 	//assign current sound to sound_player_obj
	// 	sound_player_obj.assign(chosen);
	// 	//play sound
	// 	sound_player_obj.play();
	// }

	// void PauseButton () {
	// 	//pause the player
	// 	sound_player_obj.pause();
	// 	//get location of sound and store track_pos
	// 	track_pos = sound_player_obj.get_position();
	// }

	// void StopButton () {
	// 	sound_player_obj.stop();
	// 	//set track_pos to 0
	// 	track_pos = 0.0f;
	// }
}
