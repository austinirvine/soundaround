using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using System.FileInfo;
//using System.FileSystemInfo;

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
		//DirectoryInfo dir = new DirectoryInfo(audio_path);
		List<FileInfo> file_info = new List<FileInfo>();
		List<string> ext = new List<string>{".mp3", ".ogg", ".wma"};
		var file_items = Directory.GetFiles(audio_path,"*.*",SearchOption.AllDirectories)
			.Where(s => ext.Contains(Path.GetExtension(s)));
		foreach (string file_item in file_items) {
			Debug.Log("ALL OF IT: " + file_item);
			FileInfo specific_file = new FileInfo(file_item);
			file_info.Add(specific_file);
		}
		foreach (FileInfo f in file_info) {
			Debug.Log(f.Name);
			AudioFile new_file = new AudioFile();
			new_file.clip_name = f.Name;
			audio_files.Add(new_file);
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
