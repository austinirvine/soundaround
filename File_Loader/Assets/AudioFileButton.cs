using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioFileButton : MonoBehaviour {

	public Button button;
	public Text button_text;
	public Image icon_image;

	private AudioFile audio_file;
	private AudioDirectory audio_dir;

	void Start() {
		//button.onClick.addListener(HandleClick);
	}
	public void Setup(AudioFile file, AudioDirectory dir) {
		//audio_file = file;
		//button_text.text = audio_file.clip_name;
		//icon_image.sprite = audio_file.icon_image;
		//audio_dir = dir;
	}

	public void HandleClick() {
		Debug.Log("Clicked Button!");
	}
}
