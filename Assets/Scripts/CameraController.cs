using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//public class AudioController {
//	public static CameraController cameraController;
//	
//	public static void PlaySound (AudioClip sound, Vector3 where) {
//		if (cameraController.mute) return;
//		AudioSource.PlayClipAtPoint(sound, where);
//	}
//}
//
public class CameraController : MonoBehaviour {
	public GameObject player;
	TextMesh announcement;
	TextMesh notes;
	TextMesh notesOutline;
	TextMesh expMeter;
	TextMesh statsDisplay;
	bool shouldFadeIn;
	bool shouldFadeOut;
	const float fadeSpeed = 0.015f;
	const int baseDelayBeforeFadeOut = 90;
	int delayBeforeFadeOut = baseDelayBeforeFadeOut;
	public AudioSource audioSource;
	public AudioClip song1;
	public AudioClip song2;
	public AudioClip song3;
	public AudioClip song4;
	public bool muteSoundFX = false;
	public bool muteSongs;
	public static CameraController Instance { get { return GameObject.FindObjectOfType<CameraController>(); } }
	/// <summary> scripts should always use this, not volume </summary>
	public float Volume { get { return muteSoundFX ? 0f : volume; } }
	/// <summary> use this only from the GUI editor, scripts should call Volume </summary>
	public float volume;
	
	
	public void AnnounceText(string text) {
		announcement.text = text;
		shouldFadeIn = true;
		shouldFadeOut = false;
	}
	
	public void NoteText(string text) {
		var t = new List<string>(notes.text.Split('\n'));
		while (t.Count > 9) t.RemoveAt(0);
		t.Add(text);
		notes.text = string.Join("\n", t.ToArray());
		notesOutline.text = notes.text;
	}
	
	public void ExpToLevelChanged(int expToLevel, int nextLevel) {
		expMeter.text = expToLevel + " experience to level " + nextLevel;
	}
	
	public void StatsChanged(string mainStat, float mainStatValue, float armorClass) {
		if (mainStat == "acters") {
			statsDisplay.text = mainStat + " " + mainStatValue + "  framerate " + armorClass;
			return;
		}
		System.Func<float, float> Round = n => {
			return ((float)((int) (n * 100))) / 100;
		};
		statsDisplay.text = mainStat + " " + Round(mainStatValue) + "  armor " + Round(armorClass);
	}
	
	Vector3 offset;
	// Use this for initialization
	void Awake () {
		offset = transform.position;
		announcement = GetComponentsInChildren<TextMesh>()[0];
		notes = GetComponentsInChildren<TextMesh>()[1];
		notesOutline = GetComponentsInChildren<TextMesh>()[4];
		expMeter = GetComponentsInChildren<TextMesh>()[2];
		statsDisplay = GetComponentsInChildren<TextMesh>()[3];
		notes.text = "";
		notesOutline.text = "";
		var color = announcement.color;
		color.a = 0;
		announcement.color = color;
		if (audioSource == null) Debug.LogError("wwwwttttttffffff.");
		audioSource.volume = muteSoundFX ? 0 : Volume / 2;
//		_mute = mute;
//		AudioController.cameraController = this;
	}
	
	void LateUpdate () {
		transform.position = player.transform.position + offset;
		var color = announcement.color;
		if (shouldFadeIn) {
			color.a += fadeSpeed;
			if (color.a >= 1) {
				shouldFadeIn = false;
				shouldFadeOut = true;
				delayBeforeFadeOut = baseDelayBeforeFadeOut;
			}
		}
		if (shouldFadeOut) {
			if (delayBeforeFadeOut > 0) delayBeforeFadeOut--;
			else color.a -= fadeSpeed;
			if (color.a <= 0) shouldFadeOut = false;
		}
		if (shouldFadeIn == shouldFadeOut && delayBeforeFadeOut <= 0 && color.a > 0) {
			Debug.LogError("!!! WTF in announcement, " + announcement.text + " will never fade out!!!");
		}
		
		announcement.color = color;
		
		if (!audioSource.isPlaying) {
			switch(Random.Range(0,4)) {
				case 0:
					audioSource.clip = song1;
					break;
				case 1:
					audioSource.clip = song2;
					break;
				case 2:
					audioSource.clip = song3;
					break;
				case 3:
					audioSource.clip = song4;
					break;
				default:
					Debug.LogError("no such song");
					break;
			}
			if (!muteSongs) audioSource.Play();
		}
	}
}
