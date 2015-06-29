using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : Acter {
	#region variables and properties
	public CameraController cameraController;
	public bool infiniteHealth = false;
	string mainClass = "";
	public override string MainClass { get { return mainClass; } }
	TextMesh SpeechBubble { get { return GetComponentInChildren<TextMesh>(); } }
	public static PlayerController Instance { get { return GameObject.FindObjectOfType<PlayerController>(); } }
	protected override float Speed {
		get {
			return speed;		// ignore NPC slowdown modifier
		}
	}
	public bool friendless;
	public WeaponController murderMask;
	public WeaponController executionerCowl;
	bool isSilent;
	public bool IsSilent { get { return isSilent; } }
	int lockMainHandCounter;
	bool lockMainHand;
	int lockOffHandCounter;
	bool lockOffHand;
	public bool starving;
	
	public bool IsJason { get {
		var jasonMask = GetArmor(GetSlot("Head"));
		return (jasonMask != null && jasonMask.name.Contains("Mask")); 
	} }
	#endregion
	
	void Start () {
		xpToLevel = baseXPToLevel;
		cameraController.ExpToLevelChanged(xpToLevel, level + 1);
	}
	
	public virtual void HasExploredNewRoom () {
		LivingActers.FindAll(a => a.friendly).ForEach(ally => {
//			ally.Heal(0.5f);
			if (ally.EquippedSecondaryWeapon != null) {
				var wand = ally.EquippedSecondaryWeapon.GetComponent<WandController>();
				if (wand != null) wand.HasChangedRoom();
			}
		});
	}
	
	public const int CURSE_HATE = 0, CURSE_LEPROSY = 1, CURSE_STARVE = 2, CURSE_DRAIN_CHARGE = 3, CURSE_TRAPS = 4
						, CURSE_STINGY = 5, CURSE_DEPTH = 6, CURSE_SILENT = 7, CURSE_NIGHTGAUNT = 8, CURSE_MAX_ROOMS = 9
						, CURSE_LEVEL = 10
						, CURSE_LAST_PLUS_ONE = 11;
	public void InflictCurse (int curseType) {
		switch (curseType) {
			case CURSE_HATE:
				friendless = true;
				CameraController.Instance.AnnounceText("you are friendless\nyou murderer");
				break;
			case CURSE_LEPROSY:
				healthBar.GetComponent<PlayerHealthBarController>().neverShowHealth = true;
				healthBar.SetCurrentHP(0, 0);
				CameraController.Instance.AnnounceText("you go numb");
				break;
			case CURSE_STARVE:
				starving = true;
				CameraController.Instance.AnnounceText("you're starving");
				break;
			case CURSE_DRAIN_CHARGE:
				if (EquippedSecondaryWeapon && EquippedSecondaryWeapon.charges != -1) {
					EquippedSecondaryWeapon.charges = 0;
					CameraController.Instance.NoteText("charges drained from " + EquippedSecondaryWeapon.Name);
				}
				break;
			case CURSE_TRAPS:
				TerrainController.Instance.trapRarity = Mathf.Max(1, TerrainController.Instance.trapRarity - 1);
				CameraController.Instance.AnnounceText("traps have multiplied");
				break;
			case CURSE_STINGY:
				SpawnController.Instance.stinginess += 0.5f;
				CameraController.Instance.AnnounceText("treasures flee\nfrom you");
				break;
			case CURSE_DEPTH:
				TerrainController.Instance.statuesDestroyed -= .25f;
				CameraController.Instance.AnnounceText("things are\nharder");
				break;
			case CURSE_SILENT:
				isSilent = true;
				CameraController.Instance.AnnounceText("you are struck dumb");
				break;
			case CURSE_NIGHTGAUNT:
				TerrainController.Instance.baseNightgauntRate *= 0.75f;
				CameraController.Instance.AnnounceText("the nightgaunts\nfavor you");
				TerrainController.Instance.SpawnNightgaunt();
				break;
			case CURSE_MAX_ROOMS:
				TerrainController.Instance.maxRooms = 2;		// going lower than 2 does nothing
				CameraController.Instance.AnnounceText("you feel claustrophobic");
				break;
			case CURSE_LEVEL:
				GainLevel("mummy curse");
				CameraController.Instance.AnnounceText("your mind fills with\nuseless knowledge");
				break;
		}
	}
	
	#region munchkins
	public void GainExperience (int quantity) {
		xpToLevel -= quantity;
		if (xpToLevel <= 0) GainLevel(MainClass);
		cameraController.ExpToLevelChanged(xpToLevel, level + 1);
	}
	public void SetClass(string which) {
		if (which == "priest") mainClass = C_WIZARD;
		else if (which == "murderer" || which == "wretch") mainClass = C_GESTALT;
		else if (which == "executioner") mainClass = C_FIGHT;
		else if (which == "firewalker") mainClass = C_ROGUE;
		else mainClass = which;
		
		GameObject.FindObjectOfType<Canvas>().gameObject.SetActive(false);
		WeaponController book;
		switch (which) {
			case C_ROGUE:
				TerrainController.Instance.ShowTraps = true;
				freeAction = true;
				var bow = Instantiate(GameObject.FindObjectOfType<SpawnController>().itemBow);
				bow.payload.payload = SpellGenerator.Instance.Pillar(WeaponController.DMG_PARA);
				bow.payload.payload.payload = SpellGenerator.Instance.Pillar(WeaponController.DMG_FIRE);
				bow.payload.payload.payload.attackPower *= 4;
				SpawnController.Instance.ApplyParticles(bow);
				Equip(bow);
				speed += 50;
				break;
			case "firewalker":
				var boom = Instantiate(SpellGenerator.Instance.blankWand);
				boom.payload = SpellGenerator.Instance.Vortex();
				boom.payload.payload = SpellGenerator.Instance.Explosion();
				boom.payload.payload.attackPower *= 4;
				boom.payload.payload.tag = "spell";
				SpellGenerator.Instance.Fan(boom.payload, 2);
				boom.charges = boom.maxCharges = 3;
				Equip(boom);
				speed += 50;
				spellpower += 1;
				break;
			case C_WIZARD:
			    book = Instantiate(SpellGenerator.Instance.blankBook);
				book.payload = SpellGenerator.Instance.Bolt(WeaponController.DMG_FIRE);
				book.payload.attackPower *= 2;
				book.payload.payload = SpellGenerator.Instance.Explosion();
				book.MapChildren(pl => pl.attackPower *= 2);
				book.payload.payload.attackPower *= 2;
				SpellGenerator.Instance.Fan(book, 4);
				Equip(book);
				var broom = Instantiate(SpawnController.Instance.itemBroom);
				broom.payload = SpellGenerator.Instance.Vortex();
				SpawnController.Instance.ApplyParticles(broom);
				Equip (broom);
				
				spellpower += 6;
				break;
			case "priest":
				book = Instantiate(SpellGenerator.Instance.blankBook);
				book.payload = SpellGenerator.Instance.Heal();
				book.payload.attackPower *= 5;
				Equip(book);
				spellpower += 4;
				var pet = Instantiate (SpawnController.Instance.enemyTreant);
				pet.friendly = true;
				var saw = Instantiate(TerrainController.Instance.buzzsaw);
				saw.weapon.payload = SpellGenerator.Instance.RaiseDead();
				saw.weapon.thrownBy = this;
				break;
			case C_BRUTE:
				BeginRegenerate(1 / GLOBAL_DMG_SCALING);
				Equip (Instantiate(SpawnController.Instance.itemHiltless));
				Grow(1.5f);
				break;
			case C_FIGHT:
				var weapon = Instantiate(SpawnController.Instance.itemBarMace);
				weapon.payload = SpellGenerator.Instance.Heal();
				SpawnController.Instance.ApplyParticles(weapon);
				Equip (weapon);
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemGreaves));
				Equip (Instantiate(SpawnController.Instance.itemArmor));
				Equip (Instantiate(SpawnController.Instance.itemSkirt));
				Equip (Instantiate(SpawnController.Instance.itemShield));
				break;
			case "murderer":
				freeAction = friendless = isAquatic = true;
				var murderWeapon = Instantiate(GameObject.FindObjectOfType<SpawnController>().itemMachete);
				murderWeapon.payload = SpellGenerator.Instance.Bolt(WeaponController.DMG_DEATH);
				murderWeapon.payload.payload = SpellGenerator.Instance.RaiseDead();
				murderWeapon.payload.payload.firedNoise = SpellGenerator.Instance.rippingSound;
				SpawnController.Instance.ApplyParticles(murderWeapon);
				lockMainHand = true;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(true, lockMainHand);
				murderWeapon.depth = 999;
				Equip(murderWeapon);
				Equip(Instantiate(murderMask));
				speed += 200;
				skinColor = cameraController.pinkSkin;
				ChangeSkinColor();
				SpawnController.Instance.stinginess++;
				TerrainController.Instance.statuesDestroyed--;
				isSilent = true;
				break;
			case "wretch":
				CameraController.Instance.AnnounceText ("are you really\nup to this\nchallenge?");
				TerrainController.Instance.statuesDestroyed = -27;
				break;
			case "executioner":
				Equip (Instantiate(SpawnController.Instance.itemExecutionerSword));
				var wand = Instantiate(SpellGenerator.Instance.blankWand);
				wand.charges = wand.maxCharges = 18;
				wand.payload = SpellGenerator.Instance.Beam(WeaponController.DMG_DEATH);
				wand.payload.attackPower = 100 / GLOBAL_DMG_SCALING;
				wand.payload.payload = SpellGenerator.Instance.Heal();
				wand.payload.payload.attackPower = 100 / GLOBAL_DMG_SCALING;
				SpellGenerator.Instance.Split(wand, 6);
				lockOffHand = true;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(true, false);
				Equip (wand);
				meleeMultiplier++;
				Equip (Instantiate(executionerCowl));
				skinColor = new Color(.95f, .85f, .9f);
				ChangeSkinColor();
				isSilent = true;
				break;
			default: break;
		}
	}
	#endregion

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

	IEnumerator wtfCmon () {			// this is necessary to stop editor locking up
		yield return new WaitForSeconds(0.1f);
		Application.LoadLevel("DefaultScene");
	}
	void Update () {
		#region bug hax
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel")) {
			StartCoroutine(wtfCmon());
		}
		if (transform.position.y < -10) {
			CameraController.Instance.AnnounceText("you have fallen\nthrough the world");
			transform.position = new Vector3 (transform.position.x, 50, transform.position.z);
			torso.GetComponent<SphereCollider>().isTrigger = true;
		}
		if (transform.position.y < 20) {
			torso.GetComponent<SphereCollider>().isTrigger = false;
		}
		#endregion
		#region HUD
		if (infiniteHealth) {
			cameraController.StatsChanged("acters", LivingActers.Count, Time.deltaTime);
		}
		
		SpeechBubble.transform.rotation = Quaternion.identity;
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().sortingOrder = -10;
		var fade = new Color(0, 0, 0, 1f/120);
		SpeechBubble.color -= fade; 
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().color -= fade;
		
		if (MainClass == "") return;
		
		var hbar = healthBar.GetComponent<PlayerHealthBarController>();
		if (EquippedSecondaryWeapon != null) {
			var myWand = EquippedSecondaryWeapon.GetComponent<WandController>();
			if (myWand != null) {
				hbar.itemCharges.text = myWand.charges + "/" + myWand.maxCharges;
				hbar.wandIcon.gameObject.SetActive(true);
			}
			else hbar.wandIcon.gameObject.SetActive(false);
			var myPot = EquippedSecondaryWeapon.GetComponent<EstusController>();
			if (myPot != null) {
				hbar.itemCharges.text = myPot.charges + "";
				hbar.potionIcon.gameObject.SetActive(true);
			}
			else {
				hbar.potionIcon.gameObject.SetActive(false);
			}
			if (myPot == null && myWand == null) hbar.itemCharges.text = "";
		}
		else {
			hbar.itemCharges.text = "";
			hbar.potionIcon.gameObject.SetActive(false);
			hbar.wandIcon.gameObject.SetActive(false);
		}
		#endregion
		#region left/right click
		if (!shouldUseOffhand && (Input.GetKeyDown ("e") || Input.GetKeyDown(KeyCode.Mouse0) 
		                          || Input.GetKeyDown ("j") || Input.GetButtonDown("Fire1"))) 
		{
			shouldUseMainHand = true;
		}
		if (Input.GetKey ("e") || Input.GetKey(KeyCode.Mouse0)
							    || Input.GetKey ("j") || Input.GetButton("Fire1")) {
			if (IsJason) lockMainHandCounter = int.MinValue;
			lockMainHandCounter++;
			if (lockMainHandCounter > 60 && EquippedWeapon != bareHands) {
				lockMainHand = !lockMainHand;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(lockMainHand, true);
				lockMainHandCounter = int.MinValue;
			}
		}
		else lockMainHandCounter = 0;
		
		if (!shouldUseMainHand && EquippedSecondaryWeapon != null && (Input.GetKeyDown("r")
				 || Input.GetKeyDown("k") || Input.GetKeyDown(KeyCode.Mouse1) || Input.GetButtonDown("Fire2"))) {
			shouldUseOffhand = true;
		}
		if (Input.GetKey ("r") || Input.GetKey(KeyCode.Mouse1)
						    || Input.GetKey ("k") || Input.GetButton("Fire2")) {
			lockOffHandCounter++;
			if (lockOffHandCounter > 60 && EquippedSecondaryWeapon != null) {
				lockOffHand = !lockOffHand;
				healthBar.GetComponent<PlayerHealthBarController>().ShowLock(lockOffHand, false);
				lockOffHandCounter = int.MinValue;
			}
		}
		else lockOffHandCounter = 0;
		#endregion
		
		if (Input.GetKeyDown (KeyCode.Space) || Input.GetButtonDown("Fire3")) {
			shouldPickUpItem = true;
			foreach(var sign in eligiblePickups.FindAll(s => s.name.Contains("signpost"))) {
				sign.GetComponent<SignpostController>().Speak();
				eligiblePickups.Remove(sign);
			}
		}
		if (Input.GetKeyDown (KeyCode.T) || Input.GetButtonDown("Jump")) {
			foreach (var ally in LivingActers.FindAll(p => p.friendly && p != this)) {
				ally.ShouldPickUpItem();
			}
		}
		if (Input.GetKeyDown ("tab"))
		{
			damageAnnouncer.AnnounceDeath();
			Speak("one more try...");		
			infiniteHealth = !infiniteHealth;
			spellpower *= infiniteHealth ? 10 : 0.1f;
			speed *= infiniteHealth ? 4 : 0.25f;
			freeAction = infiniteHealth;
			GetComponent<Rigidbody>().mass = infiniteHealth ? 3 : 1;
		}
//		if (infiniteHealth) grappledBy.ForEach(e => {
//			e.TakeDamage(100, WeaponController.DMG_DEATH);
//		});
		if (infiniteHealth) grappledBy.Clear();
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
			if (State == ST_DEAD && !dead) {
				print(quantity + " type " + type + " damage");
			}
		}
	}

	void FixedUpdate () {
		if (!LivingActers.Contains(this))LivingActers.Add(this);
		
		if (infiniteHealth) {
			;		// see Update()
		}
		else if (MainClass == C_WIZARD) {
			cameraController.StatsChanged("intelligence", spellpower, armorClass);
		}
		else {
			cameraController.StatsChanged("strength", meleeMultiplier, armorClass);
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
		else {
			Move (Vector3.zero);
		}
		if (State != ST_WALK && State != ST_REST) GetComponent<Rigidbody>().velocity = Vector3.zero;
		
		if (v == Vector3.zero && State != ST_CAST) {
			EnterStateAndAnimation(ST_REST);
		}
	}
	
	public override void WeaponDidCollide (Acter other, WeaponController weaponController, bool friendlyFireOK)
	{
		if (infiniteHealth && weaponController.attackPower > 0) {
			other.TakeDamage(100, WeaponController.DMG_DEATH);
			other.TakeDamage(100, WeaponController.DMG_RAISE);
			return;
		}
			
		base.WeaponDidCollide(other, weaponController, friendlyFireOK);
		cameraController.ExpToLevelChanged(xpToLevel, level + 1);
	}
}
