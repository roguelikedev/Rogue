using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Acter {
	public CameraController announcer;
	public bool infiniteHealth = false;
	string mainClass = "";
	public override string MainClass { get { return mainClass; } }
	TextMesh SpeechBubble { get { return GetComponentInChildren<TextMesh>(); } }
	public static PlayerController Instance { get { return GameObject.FindObjectOfType<PlayerController>(); } }
	
	void Start () {
		xpToLevel = baseXPToLevel;
		announcer.ExpToLevelChanged(xpToLevel, level + 1);
	}
	
	public void SetClass(string which) {
		if (which == "priest") mainClass = C_WIZARD;
		else mainClass = which;
		GameObject.FindObjectOfType<Canvas>().gameObject.SetActive(false);
		WeaponController book;
		switch (which) {
			case C_ROGUE:
				GameObject.FindObjectOfType<TerrainController>().ShowTraps = true;
				var bow = Instantiate(GameObject.FindObjectOfType<SpawnController>().itemBow);
				bow.payload.payload = SpellGenerator.Instance().Pillar(WeaponController.DMG_PARA);
				bow.payload.payload.payload = SpellGenerator.Instance().Explosion();
				Equip(bow);
				speed += 50;
				break;
			case C_WIZARD:
			    book = Instantiate(SpellGenerator.Instance().blankBook);
				book.payload = SpellGenerator.Instance().Bolt(WeaponController.DMG_FIRE);
				book.payload.attackPower *= 2;
				book.payload.payload = SpellGenerator.Instance().Explosion();
				SpellGenerator.Instance().Split(book, 2);
				
//				book.payload = SpellGenerator.Instance().Explosion();
				Equip(book);
				spellpower += 4;
				break;
			case "priest":
				book = Instantiate(SpellGenerator.Instance().blankBook);
				book.payload = SpellGenerator.Instance().RaiseDead();
				book.payload.payload = SpellGenerator.Instance().Heal();
				book.payload.payload.attackPower *= 5;
				Equip(book);
				spellpower += 4;
				meleeMultiplier += 0.5f;
				Equip (Instantiate(SpawnController.Instance.itemShillalegh));
				break;
			case C_BRUTE:
				BeginRegenerate(1);
				racialBaseHitPoints++;
				Equip (Instantiate(SpawnController.Instance.itemHiltless));
				transform.position = transform.position + new Vector3(0, 3, 0);
				transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
				huge = true;
				meleeMultiplier++;
				break;
			case C_FIGHT:
				var machet = Instantiate(SpawnController.Instance.itemMachete);
				machet.payload = SpellGenerator.Instance().Heal();
				Equip (machet);
//				Equip (Instantiate(SpawnController.Instance.itemMachete));
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemArmor));
				Equip (Instantiate(SpawnController.Instance.itemSkirt));
				Equip (Instantiate(SpawnController.Instance.itemShield));
				meleeMultiplier++;
				break;
			default: break;
		}
	}

	public void Speak(string what) {
		SpeechBubble.text = what;
		var c = SpeechBubble.color;
		c.a = 1;
		SpeechBubble.color = c;
		c = SpeechBubble.GetComponentInChildren<SpriteRenderer>().color;
		c.a = 1;
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().color = c;
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.LoadLevel("DefaultScene");
		}
		SpeechBubble.transform.rotation = Quaternion.identity;
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().sortingOrder = -10;
		var fade = new Color(0, 0, 0, 1f/120);
		SpeechBubble.color -= fade; 
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().color -= fade;
		
		if (MainClass == "") return;
		if (!shouldUseOffhand && (Input.GetKeyDown ("e") || Input.GetKeyDown(KeyCode.Mouse0))) 
		{
			shouldUseMainHand = true;
		}
		if (!shouldUseMainHand && EquippedSecondaryWeapon != null && (Input.GetKeyDown("r") || Input.GetKeyDown(KeyCode.Mouse1))) {
			shouldUseOffhand = true;
		}
		
		
		if (Input.GetKeyDown (KeyCode.Space)) {
			shouldPickUpItem = true;
			foreach(var sign in eligiblePickups.FindAll(s => s.name.Contains("signpost"))) {
				sign.GetComponent<SignpostController>().Speak();
				eligiblePickups.Remove(sign);
			}
		}
		if (Input.GetKeyDown (KeyCode.T)) {
			foreach (var ally in livingActers.FindAll(p => p.friendly && p != this)) {
				ally.ShouldPickUpItem();
			}
		}
		if (Input.GetKeyDown ("tab"))
		{
			infiniteHealth = !infiniteHealth;
			spellpower *= infiniteHealth ? 10 : 0.1f;
			speed *= infiniteHealth ? 2 : 0.5f;
			freeAction = infiniteHealth;
			GetComponent<Rigidbody>().mass = infiniteHealth ? 3 : 1;
		}
		if (infiniteHealth) grappledBy.ForEach(e => {
			print(e);
			e.TakeDamage(100, WeaponController.DMG_DEATH);
		});
	}

	public override void TakeDamage (float quantity, int type)
	{
		if (infiniteHealth) Speak("nope, invulnerable (" + quantity +")");
		else {
			var dead = State == ST_DEAD;
			base.TakeDamage (quantity, type);
			if (MainClass == C_BRUTE && type == WeaponController.DMG_FIRE) {
				Speak("argh!  fire!");
			}
			if (State == ST_DEAD && !dead) print(quantity + " type " + type + " damage");
		}
	}

	void FixedUpdate () {
		if (!livingActers.Contains(this))livingActers.Add(this);
		
		if (MainClass == C_WIZARD) {
			announcer.StatsChanged("intelligence", spellpower, armorClass);
		}
		else {
			announcer.StatsChanged("strength", meleeMultiplier, armorClass);
		}
		
		if (!_FixedUpdate()) return;
		if (MainClass == "") return;

		Vector3 v = Vector3.zero;
		v.x = Input.GetAxis ("Horizontal");
		v.z = Input.GetAxis ("Vertical");
						

		if (v.x != 0 || v.z != 0) {
			if (EnterStateAndAnimation(ST_WALK)) Move(v);
		}
		else if (v == Vector3.zero && State != ST_CAST) {
			EnterStateAndAnimation(ST_REST);
		}
	}
	
	public override void WeaponDidCollide (Acter other, WeaponController weaponController, bool friendlyFireOK)
	{
		base.WeaponDidCollide(other, weaponController, friendlyFireOK);
		announcer.ExpToLevelChanged(xpToLevel, level + 1);
	}
}
