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
	bool isTerrifying;
	public bool IsTerrifying { get { return isTerrifying; } }
	int lockMainHandCounter;
	bool lockMainHand;
	int lockOffHandCounter;
	bool lockOffHand;
	
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
//				boom.payload.payload = SpellGenerator.Instance.Explosion();
//				boom.payload.payload.attackPower *= 4;
//				boom.payload.payload.tag = "spell";
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
				isTerrifying = true;
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
				isTerrifying = true;
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
		if (infiniteHealth) {
			cameraController.StatsChanged("acters", LivingActers.Count, Time.deltaTime);
		}
	
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Cancel")) {
			StartCoroutine(wtfCmon());
		}
		SpeechBubble.transform.rotation = Quaternion.identity;
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().sortingOrder = -10;
		var fade = new Color(0, 0, 0, 1f/120);
		SpeechBubble.color -= fade; 
		SpeechBubble.GetComponentInChildren<SpriteRenderer>().color -= fade;
		
		if (MainClass == "") return;
		
		if (EquippedSecondaryWeapon != null) {
			var hbar = healthBar.GetComponent<PlayerHealthBarController>();
			var myWand = EquippedSecondaryWeapon.GetComponent<WandController>();
			if (myWand != null) {
				hbar.itemCharges.text = myWand.charges + "/" + myWand.maxCharges;
				hbar.wandIcon.gameObject.SetActive(true);
			}
			else hbar.wandIcon.gameObject.SetActive(false);
			var myPot = EquippedSecondaryWeapon.GetComponent<EstusController>();
			if (myPot != null) {
				print (myPot);
				hbar.itemCharges.text = myPot.charges + "";
				hbar.potionIcon.gameObject.SetActive(true);
			}
			else {
				hbar.itemCharges.text = "";
				hbar.potionIcon.gameObject.SetActive(false);
			}
		}
		
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
		else GetComponent<Rigidbody>().velocity = Vector3.zero;
		
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
