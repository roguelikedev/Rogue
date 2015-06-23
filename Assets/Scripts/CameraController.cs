using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CameraController : MonoBehaviour {		// FIXME: this class shouldn't be used as a kitchen sink singleton
	public PlayerController orc;
	public PlayerController hope;
	public PlayerController elf;
	public PlayerController beardMan;
	public PlayerController tusks;
	public PlayerController firewalker;
	public Color pinkSkin;
	public List<Acter> livingActers = new List<Acter>();
	public float npcSpeedModifier = 1;
	PlayerController player;
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
	public AudioClip song5;
	public bool muteSoundFX = false;
	public bool muteSongs;
	public static CameraController Instance { get { return GameObject.FindObjectOfType<CameraController>(); } }
	/// <summary> scripts should always use this, not volume </summary>
	public float Volume { get { return muteSoundFX ? 0f : volume; } }
	/// <summary> use this only from the GUI editor, scripts should call Volume </summary>
	public float volume;
	
	public void AddPlayer (string who) {
		if (who == "wizard" || who == "wretch") player = Instantiate(elf);
		else if (who == "rogue") player = Instantiate(tusks);
		else if (who == "priest") player = Instantiate(hope);
		else if (who == "firewalker") player = Instantiate(firewalker);
		else if (who == "fighter") player = Instantiate(beardMan);
		else player = Instantiate(orc);
		player.transform.position = new Vector3(10, 3.5f, 0);
		player.cameraController = this;
		player.SetClass(who);
	}
	
	bool isAnnouncingDeath = false;
	const string DEATH_MSG = "YOU DIED...\npress escape to restart";
	const string CREDITS = "\n\n\n\n\nRoguelike v.152 brought to you by\n---most skunked backer---\nRyan McHenry"
						+ "\n---biggest pledge and best death scream---\nJustin \"Katak\" McKeon"
						+ "\n---best mom and tied for biggest pledge---\nLaurie R. King"
						+ "\n---programming and hair loss\nNate \"roguelikedev\" King";
	void DidFadeOut () {
		if (!isAnnouncingDeath || announcement.text == CREDITS) return;
		AnnounceText(CREDITS);
		announcement.transform.position = announcement.transform.position - new Vector3(0, 5);
		delayBeforeFadeOut = 270;
	}
	
	public void AnnounceDeath (string cause) {
//		var lines = cause.Split(new char[]{'\n'}).Length;
//		for (int lcv = 0; lcv < lines / 2; ++lcv) {
//			cause = "\n" + cause;		
//		}
		AnnounceText(cause + "\n\npress escape to restart");
		isAnnouncingDeath = true;
		PlaySound(PlayerController.Instance.damageAnnouncer.playerDeath);
	}
	
	public void AnnounceText(string text) {
		if (isAnnouncingDeath && text != CREDITS) return;
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
		audioSource.volume = muteSoundFX ? 0 : Volume;
//		_mute = mute;
//		AudioController.cameraController = this;
	}
	
	public void PlaySound (AudioClip sound) {
		if (muteSoundFX) return;
		AudioSource.PlayClipAtPoint(sound, PlayerController.Instance.transform.position, Volume);
	}
	
	void PlayMusic () {
		if (!audioSource.isPlaying) {
			switch(Random.Range(0,5)) {
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
				case 4:
					audioSource.clip = song5;
					break;
				default:
					Debug.LogError("no such song");
					break;
			}
			if (!muteSongs) audioSource.Play();
		}
	}
	
	void LateUpdate () {
		if (player != null) {
			transform.position = player.transform.position + offset;
		}
		if (isAnnouncingDeath) {
			offset = offset + new Vector3(0, .05f);
			if (announcement.text == CREDITS) {
				announcement.transform.position = announcement.transform.position + new Vector3(0, .075f);
			}
		}
		var color = announcement.color;
		if (shouldFadeIn) {
			color.a += fadeSpeed;
			if (color.a >= 1) {
				shouldFadeIn = false;
				shouldFadeOut = true;
				if (delayBeforeFadeOut < baseDelayBeforeFadeOut) delayBeforeFadeOut = baseDelayBeforeFadeOut;  // allow long credits
			}
		}
		if (shouldFadeOut) {
			if (delayBeforeFadeOut > 0) delayBeforeFadeOut--;
			else color.a -= fadeSpeed;
			if (color.a <= 0) {
				shouldFadeOut = false;
				DidFadeOut();
			}
		}
		if (shouldFadeIn == shouldFadeOut && delayBeforeFadeOut <= 0 && color.a > 0) {
			Debug.LogError("!!! WTF in announcement, " + announcement.text + " will never fade out!!!");
		}
		
		announcement.color = color;
		
		PlayMusic();
	}
}
