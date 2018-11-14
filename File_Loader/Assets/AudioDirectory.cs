using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

[System.Serializable]
public class AudioFile {
	public string clip_name;
	public Sprite icon_image;
	public List<string> filter_contents;
}

public class AudioDirectory : MonoBehaviour {

	public List<AudioFile> audio_files;
	public AudioButtonPool audioPool;
	public List<string> known_filters;
	public Transform content_panel;

	// Use this for initialization
	void Start() {
		string audio_path = "/Users/austinirvine/Documents/soundaround/File_Loader/Assets/AudioFiles";
		RefreshAudioDisplay(audio_path);
		
	}

	void RefreshAudioDisplay(string audio_path) {
		RemoveButtons();
		AssociateAudio(audio_path);
		AddButtons();
	}

	void RemoveButtons() {
		while(content_panel.childCount > 0) {
			GameObject toRemove = transform.GetChild(0).gameObject;
			audioPool.ReturnObject(toRemove);
		}
	}
	void AssociateAudio(string audio_path) {
		DirectoryInfo dir = new DirectoryInfo(audio_path);
		FileInfo[] file_info = dir.GetFiles("*.*");

		foreach (FileInfo f in file_info) {
			Debug.Log(f.Name);
			AudioFile new_file = new AudioFile();
			new_file.clip_name = f.Name;
			audio_files.Add(new_file);
			//audio_files.Add(new_file);
			//GameObject new_button = audioPool.GetObject();
			//new_button.transform.SetParent(content_panel);

			//AudioFileButton audio_button = new_button.GetComponent<AudioFileButton>();
            //audio_button.Setup(f, this);
		}
	}

	void AddButtons() {
		foreach (AudioFile file in audio_files) {
			Debug.Log(file.clip_name);
			GameObject new_button = audioPool.GetObject();
			new_button.transform.SetParent(content_panel);

			AudioFileButton audio_button = new_button.GetComponent<AudioFileButton>();
            audio_button.Setup(file, this);
		}
	}
}
