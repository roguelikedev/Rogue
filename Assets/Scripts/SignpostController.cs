using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SignpostController : ItemController {
	public TextMesh Speech { get { return GetComponentInChildren<TextMesh>(); } }
	public SpriteRenderer Bubble { get { return Speech.GetComponentInChildren<SpriteRenderer>(); } }
	string info = "";
	int timeToFadeout = int.MinValue;
	public WeaponController brick;
	public WeaponController payload;
	EnemyController spawnIn;
	public int fixedSpeech;
	List<int> HeardIt { get { return SpawnController.Instance.previouslyPosted; } }
	bool wasRead = false;


	public void Speak() {		// FIXME:  DRY
		if (spawnIn != null) {
			var punish = fixedSpeech == -1 && PlayerController.Instance.IsJason;
			if (punish) {
				spawnIn = SpawnController.Instance.enemyDemon;
				info = "die, murderer";
				Speech.fontSize = 200;
			}
			if (fixedSpeech == -2 && PlayerController.Instance.friendless) {
				info = "you are friendless\nyou murderer";
				payload = null;
			}
			
			var e = Instantiate(spawnIn);
			e.gameObject.SetActive(true);
			e.transform.position = transform.position + new Vector3(0, 2, 0);
			spawnIn = null;
			if (punish) {
				SpawnController.Instance.AddLevels(e, PlayerController.Instance.level * 4);
				SpawnController.Instance.AddEquipment(e, true);
			}
		}
	
		if (info == " ") {
			PlayerController.Instance.Speak("illegible");
			wasRead = true;
		}
		else {
			Speech.text = info;
			var c = Speech.color;
			c.a = 1;
			Speech.color = c;
			timeToFadeout = 45;
		}
		
		if (fixedSpeech == -3) {
			PlayerController.Instance.GainLevel(PlayerController.Instance.MainClass);
		}
		
		if (payload != null) {
			var e = Instantiate(payload);
			e.gameObject.SetActive(true);
			e.transform.position = transform.position + e.transform.position;
			if (e.attackPower > 0) {
				e.lifetime = 90;
				e.friendlyFireActive = true;
				e.attackActive = true;
			}
			payload = null;
		}
		if (!wasRead) {
			wasRead = true;
			PlayerController.Instance.GainExperience(PlayerController.Instance.level + 1);
		}
	}
	
	protected override void _FixedUpdate ()
	{
		Speech.transform.rotation = Quaternion.identity;		// FIXME:  DRY
		if (timeToFadeout-- <= 0) {
			var fade = new Color(0, 0, 0, 1f/120);
			Speech.color -= fade; 
//			Bubble.color -= fade;
		}
		while (info == "") {
			var sw = fixedSpeech != 0 ? fixedSpeech : Random.Range(0, 29);
			if (HeardIt.Contains(sw) && sw < 28) continue;
			HeardIt.Add(sw);
			switch(sw) {
				case -1:
					info = "left click";
					spawnIn = SpawnController.Instance.enemyCHUD;
					break;
				case -2:
					info = "T gives it to her\nspace gets it\nright click uses it";
					payload = SpawnController.Instance.itemEstusFlask;
					spawnIn = SpawnController.Instance.enemyWoman;
					break;
				case -3:
					info = "reading signs\nis worth it";
					break;
				case 1:
					info = "fire damage\ndoesn't\nregenerate";
					break;
				case 2:
					info = "ghosts cannot\nbe killed\nby damage";
					payload = SpawnController.Instance.itemEstusFlask;
					break;
				case 3:
					info = "can't flee if\ntentacles\ntouch you";
					break;
				case 4:
					info = "seek iron lich\nat depth 27";
					break;
				case 5:
					info = "everything is\ndangerous";
					payload = brick;
					payload.transform.position = new Vector3(0, 9, 0);
					payload.attackPower = PlayerController.Instance.racialBaseHitPoints;
					break;
				case 6:
					info = "the dungeon\nchanges\nbehind you";
					break;
				case 7:
					info = "every room is\ndeeper than\nthe last";
					break;
//				case 8:
//					info = "fear water\nand death";
//					break;
				case 9:
					info = "death damage\nignores\narmor";
					break;
				case 10:
					info = "breaking statues\nlowers depth";
					break;
				case 11:
					info = "anything can\ngain levels";
					break;
//				case 12:
//					info = "exploring\nheals you";
//					break;
				case 13:
					info = "heavy armor\nslows movement";
					break;
				case 14:
					if (PlayerController.Instance.friendless) {
						info = "die, you\nmurderer";
						payload = SpellGenerator.Instance().Pillar(WeaponController.DMG_DEATH);
						payload.attackPower = 10;
					}
					else {
						info = "stay strong";
						payload = SpellGenerator.Instance().Heal();
					}
					break;
				case 15:
					info = "beware of a\nghoul's touch";
					break;
				case 16:
					info = "moving away from\nattacks is faster\nthan normal";
					break;
				case 17:
					info = "escape is easy\nwhile allies fight";
					break;
				case 18:
					info = "canny explorers\ncan repeat a room";
					break;
				case 19:
					info = "at the start\nboth directions\nare the same";
					break;
				case 20:
					info = "blocking an\nattack will\nstagger your foe";
					break;
				case 21:
					info = "it's difficult\nto stagger\nan ogre";
					break;
				case 22:
					info = "only physical\ndamage will\ncause stagger";
					break;
				case 23:
					info = "katak cannot find\nhis back shoe";
					break;
				case 24:
					info = "katak cannot find\namulet of trapfinding";
					break;
				case 26:
					info = "you're doomed";
					break;
				case 27:
					info = "treants fear\nonly death";
					break;
				default:
					info = " ";
					break;		// player says "illegible"
			}
		}
		
		base._FixedUpdate ();
	}
}
