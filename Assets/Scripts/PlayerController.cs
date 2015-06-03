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
	public bool friendless;
	public WeaponController murderMask;
	int lockMainHandCounter;
	bool lockMainHand;
	int lockOffHandCounter;
	bool lockOffHand;
	
	void Start () {
		xpToLevel = baseXPToLevel;
		announcer.ExpToLevelChanged(xpToLevel, level + 1);
	}
	
	public void SetClass(string which) {
		if (which == "priest") mainClass = C_WIZARD;
		else if (which == "murderer" || which == "wretch") mainClass = C_GESTALT;
		else if (which == "executioner") mainClass = C_FIGHT;
		else mainClass = which;
		GameObject.FindObjectOfType<Canvas>().gameObject.SetActive(false);
		WeaponController book;
		switch (which) {
			case C_ROGUE:
				TerrainController.Instance.ShowTraps = true;
				freeAction = true;
				var bow = Instantiate(GameObject.FindObjectOfType<SpawnController>().itemBow);
				bow.payload.payload = SpellGenerator.Instance().Pillar(WeaponController.DMG_PARA);
				bow.payload.payload.payload = SpellGenerator.Instance().Explosion();
				bow.payload.payload.payload.attackPower /= 2;
				Equip(bow);
				speed += 50;
				break;
			case C_WIZARD:
			    book = Instantiate(SpellGenerator.Instance().blankBook);
//				book.payload = SpellGenerator.Instance().Bolt(WeaponController.DMG_FIRE);
//				book.payload = SpellGenerator.Instance().Mortar(WeaponController.DMG_FIRE);
				book.payload = SpellGenerator.Instance().Beam(WeaponController.DMG_FIRE);
			
				book.payload.attackPower *= 2;
//				book.payload.payload = SpellGenerator.Instance().Explosion();
//				book.payload.payload.attackPower *= 2;
//				SpellGenerator.Instance().Split(book, 4);
				SpellGenerator.Instance().Fan(book, 4);
				Equip(book);
				
				Equip (Instantiate(SpawnController.Instance.itemBroom));
				
				spellpower += 6;
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
				var pet = Instantiate (SpawnController.Instance.enemyTreant);
				pet.friendly = true;
				break;
			case C_BRUTE:
				BeginRegenerate(1 / GLOBAL_DMG_SCALING);
				racialBaseHitPoints++;
				Equip (Instantiate(SpawnController.Instance.itemHiltless));
				transform.position = transform.position + new Vector3(0, 3, 0);
				transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
				huge = true;
				meleeMultiplier++;
				break;
			case C_FIGHT:
				var weapon = Instantiate(SpawnController.Instance.itemBarMace);
				weapon.payload = SpellGenerator.Instance().Heal();
				Equip (weapon);
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemArmor));
				Equip (Instantiate(SpawnController.Instance.itemSkirt));
				Equip (Instantiate(SpawnController.Instance.itemShield));
//				meleeMultiplier++;
				break;
			case "murderer":
				freeAction = friendless = isAquatic = true;
				var murderWeapon = Instantiate(GameObject.FindObjectOfType<SpawnController>().itemMachete);
				murderWeapon.payload = SpellGenerator.Instance().Bolt(WeaponController.DMG_DEATH);
				murderWeapon.payload.payload = SpellGenerator.Instance().RaiseDead();
				murderWeapon.payload.payload.firedNoise = SpellGenerator.Instance().rippingSound;
				lockMainHand = true;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(lockMainHand, true);
				murderWeapon.depth = 999;
				Equip(murderWeapon);
				Equip(Instantiate(murderMask));
				speed += 200;
				SpawnController.Instance.stinginess++;
				TerrainController.Instance.statuesDestroyed--;
				break;
			case "wretch":
				CameraController.Instance.AnnounceText ("are you really\nup to this\nchallenge?");
				TerrainController.Instance.statuesDestroyed = -27;
				break;
			case "executioner":
				Equip (Instantiate(SpawnController.Instance.itemExecutionerSword));
				var wand = Instantiate(SpellGenerator.Instance().blankWand);
				wand.payload = SpellGenerator.Instance().Beam(WeaponController.DMG_DEATH);
				wand.payload.attackPower = 100 / GLOBAL_DMG_SCALING;
				wand.payload.payload = SpellGenerator.Instance().Heal();
				wand.payload.payload.attackPower = 100 / GLOBAL_DMG_SCALING;
				SpellGenerator.Instance().Split(wand, 6);
				Equip (wand);
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
	
	public override bool WantsToEquip (WeaponController w)
	{
		if (w.bodySlot == WeaponController.EQ_WEAPON) {
			if (w.IsOffhand && lockOffHand) return false;
			if (!w.IsOffhand) if (lockMainHand) return false;
		}
		return base.WantsToEquip (w);
	}

	void Update () {
		if (infiniteHealth) {
			announcer.StatsChanged("acters", livingActers.Count, Time.deltaTime);
		}
	
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel")) {
			Application.LoadLevel("DefaultScene");
		}
		SpeechBubble.transform.rotation = Quaternion.identity;
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().sortingOrder = -10;
		var fade = new Color(0, 0, 0, 1f/120);
		SpeechBubble.color -= fade; 
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().color -= fade;
		
		if (MainClass == "") return;
		
		if (!shouldUseOffhand && (Input.GetKeyDown ("e") || Input.GetKeyDown(KeyCode.Mouse0) || Input.GetButtonDown("Fire1"))) 
		{
			shouldUseMainHand = true;
		}
		if (Input.GetKey ("e") || Input.GetKey(KeyCode.Mouse0) || Input.GetButton("Fire1")) {
			var jasonMask = GetArmor(GetSlot("Head"));
			if (jasonMask != null && jasonMask.name.Contains("Mask")) lockMainHandCounter = int.MinValue;
		
			lockMainHandCounter++;
			if (lockMainHandCounter > 60 && EquippedWeapon != bareHands) {
				lockMainHand = !lockMainHand;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(lockMainHand, true);
				lockMainHandCounter = int.MinValue;
			}
		}
		else lockMainHandCounter = 0;
		
		if (!shouldUseMainHand && EquippedSecondaryWeapon != null && (Input.GetKeyDown("r") || Input.GetKeyDown(KeyCode.Mouse1)
                                                                                            || Input.GetButtonDown("Fire2"))) {
			shouldUseOffhand = true;
		}
		if (Input.GetKey ("r") || Input.GetKey(KeyCode.Mouse1) || Input.GetButton("Fire2")) {
			lockOffHandCounter++;
			print (lockOffHandCounter);
			if (lockOffHandCounter > 60 && EquippedSecondaryWeapon != null) {
				lockOffHand = !lockOffHand;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(lockOffHand, false);
				lockOffHandCounter = int.MinValue;
			}
		}
		else lockOffHandCounter = 0;
		
		if (Input.GetKeyDown (KeyCode.Space) || Input.GetButtonDown("Fire3")) {
			shouldPickUpItem = true;
			foreach(var sign in eligiblePickups.FindAll(s => s.name.Contains("signpost"))) {
				sign.GetComponent<SignpostController>().Speak();
				eligiblePickups.Remove(sign);
			}
		}
		if (Input.GetKeyDown (KeyCode.T) || Input.GetButtonDown("Jump")) {
			foreach (var ally in livingActers.FindAll(p => p.friendly && p != this)) {
				ally.ShouldPickUpItem();
			}
		}
		if (Input.GetKeyDown ("tab"))
		{
			infiniteHealth = !infiniteHealth;
			spellpower *= infiniteHealth ? 10 : 0.1f;
			speed *= infiniteHealth ? 4 : 0.25f;
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
		if (infiniteHealth) {
			Speak("nope, invulnerable (" + quantity +")");
		}
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
		
		if (infiniteHealth) {
			;		// see Update()
		}
		else if (MainClass == C_WIZARD) {
			announcer.StatsChanged("intelligence", spellpower, armorClass);
		}
		else {
			announcer.StatsChanged("strength", meleeMultiplier, armorClass);
		}
		if (MainClass == C_GESTALT) poiseBreakCounter = 0;
		
		
		if (!_FixedUpdate()) return;
//		print ("should " + (shouldUseMainHand ? " attack " : "") + (shouldUseOffhand ? " shoot " : ""));
		if (MainClass == "") return;

		Vector3 v = Vector3.zero;
		v.x = Input.GetAxis ("Horizontal");
		v.z = Input.GetAxis ("Vertical");
//		if (v.magnitude <= 0.3f) v = Vector3.zero;
						
		if (v.x != 0 || v.z != 0) {
			if (EnterStateAndAnimation(ST_WALK)) Move(v);
		}
		else GetComponent<Rigidbody>().velocity = Vector3.zero;
		
		if (v == Vector3.zero && State != ST_CAST) {
			EnterStateAndAnimation(ST_REST);
		}
	}
	
	public override void WeaponDidCollide (Acter other, WeaponController weaponController, bool friendlyFireOK)
	{
		base.WeaponDidCollide(other, weaponController, friendlyFireOK);
		announcer.ExpToLevelChanged(xpToLevel, level + 1);
	}
}
