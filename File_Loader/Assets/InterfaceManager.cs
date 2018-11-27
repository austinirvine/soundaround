using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Singleton that gives global control to calling buttons
   NOTED: It is a gross singleton, avoid these when possible,
   but it will work for this simple task of button management.
*/
public class InterfaceManager : MonoBehaviour {

	#region Private Variables & Properties

	#endregion

	#region Public Variables & Properties
	public GameObject SubAudioMenu;
	public bool SubAudioMenuOpen = false;
	#endregion

	#region Public Custom Button Calls
	void Awake(){

	}

	public void AudioMenu() {
		if(SubAudioMenuOpen == false) {
			SubAudioMenu.SetActive(true);
			SubAudioMenuOpen = true;
			Debug.Log("We Instantiated onece");
			RetrieveAudioList();
		} else if(SubAudioMenu == true){
			Debug.Log("bullshit");
		}
	}

	public void ExitSubMenu() {
		if(SubAudioMenuOpen == true && SubAudioMenu.activeSelf) {
			SubAudioMenu.SetActive(false);
			SubAudioMenuOpen = false;
		}
	}

	public void SelectAudio() {
		Debug.Log("Select Audio");
	}

	public void ImportAudio() {
		Debug.Log("Import Audio");
	}

	public void RecordAudio() {
		Debug.Log("Record Audio");
	}

	private void DisplayAudioList(List<string> audio_array) {
		Debug.Log("Audio Array: "+ audio_array);
	}

	private void RetrieveAudioList() {
		List<string> audio_array = new List<string>();
		audio_array.Add("Cat");
		audio_array.Add("Mouse");
		audio_array.Add("Frog");
		//reads audio folder
		//check current filters and place in audio_array
		DisplayAudioList(audio_array);
	}

	public void AdjustFilters() {
		//check filter name
		//then do your thing
		RetrieveAudioList();
		return;
	}
	#endregion
}
