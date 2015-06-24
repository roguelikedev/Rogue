using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Utilities.Geometry;

public abstract class Acter : MonoBehaviour {
	#region munchkin stats
	public float speed = 200;
	protected virtual float Speed { get { return speed == SPEED_WHEN_PARALYZED ? speed :
												 friendly ? speed :
												 speed * CameraController.Instance.npcSpeedModifier; } }
	public float racialBaseHitPoints;
	protected int xpToLevel;
	protected readonly int baseXPToLevel = 5;
	float hpFromLevels;
	public float MaxHitPoints { get { return racialBaseHitPoints + hpFromLevels; }
							set { hpFromLevels = value - racialBaseHitPoints; } }
	protected float hitPoints;
	public float HitPoints { get { return hitPoints; } }
	protected float fireDamageTaken;
	protected float armorClass = 1;
	public float EffectiveCurrentHP { get { return HitPoints * armorClass / GLOBAL_DMG_SCALING; } }
	public float meleeMultiplier = 1;
	protected float poiseBreakCounter = 0;
	public float Poise { get { return (armorClass + racialBaseHitPoints) / 2.5f; } }
	public float spellpower = 1;
	public int level = 0;
	public const string C_FIGHT = "fighter", C_WIZARD = "wizard", C_ROGUE = "rogue", C_BRUTE = "brute", C_GESTALT = "hybrid";
	public bool isAquatic = false;
	public bool freeAction = false;
	float paralyzeScaling = 1;
	int CLVL_SOFT_CAP = 9;
	public const float GLOBAL_DMG_SCALING = 0.5f;
	
	public void RewardExperience (int qty) {
		xpToLevel -= qty;
		if (xpToLevel <= 0) {
			GainLevel(MainClass);
		}
	}
	public void GainLevel(string whichClass) {
		switch (whichClass) {
			case C_BRUTE: goto case C_FIGHT;
			case C_FIGHT:
				if (hpFromLevels < racialBaseHitPoints * 2) {
					hpFromLevels++;
				}
				if (level < CLVL_SOFT_CAP) {
					meleeMultiplier++;
				}
				break;
			case C_ROGUE:
				if (hpFromLevels < racialBaseHitPoints) {
					hpFromLevels++;
				}
				if (level < CLVL_SOFT_CAP) {
					meleeMultiplier += .75f;
					spellpower += 0.75f;
				}
				speed += 90 / (level + 1);
				break;
			case C_WIZARD:
				if (hpFromLevels < racialBaseHitPoints) {
					hpFromLevels += 0.5f;
				}
				if (level < CLVL_SOFT_CAP) {
					spellpower += .75f;
					speed += 90 / (level + 3);
					meleeMultiplier += .5f;
				}
				spellpower += .75f;
				break;
			case C_GESTALT:
				GainLevel(C_ROGUE);
				GainLevel(C_WIZARD);
				GainLevel(C_FIGHT);
				level -= 3;
				break;
			default:
				Debug.LogError("no such class: " + whichClass);
				break;
		}
		hitPoints = MaxHitPoints;		// don't use Heal() to avoid killing ghosts/ghouls
		fireDamageTaken = 0;
		++level;
		xpToLevel = (int)Math.Pow(baseXPToLevel, level * .75f);
		damageAnnouncer.AnnounceText("reached level " + level);
		if (this is PlayerController) {
			PlayerController.Instance.cameraController.ExpToLevelChanged(xpToLevel, level + 1);
		}
	}
	public virtual string MainClass
	{
		get
		{
			if (spellpower > 0) return C_WIZARD;
			if (speed >= racialBaseHitPoints * 100) return C_ROGUE;
			if (huge || (meleeMultiplier > 1 && Speed < 400)) return C_BRUTE;
			return C_FIGHT;
		}
	}
	protected void BeginRegenerate(float qtyPerSecond) {
		OnFixedUpdate += () => {
			if (State != ST_DEAD) {
				hitPoints += (qtyPerSecond / 60) * GLOBAL_DMG_SCALING;
				if (hitPoints > MaxHitPoints - fireDamageTaken) hitPoints = MaxHitPoints - fireDamageTaken;
			}
		};
	}
	public void Grow(float hugeness) {
		huge = true;
		var tmp = transform.localScale;
		tmp.x *= hugeness; tmp.y *= hugeness; tmp.z *= hugeness;
		transform.position = transform.position + new Vector3(0, 3, 0);
		transform.localScale = tmp;
		racialBaseHitPoints += (int) hugeness;
		meleeMultiplier = Mathf.Max(meleeMultiplier * hugeness, meleeMultiplier + hugeness);
	}
	#endregion
	#region instance variables
	public List<Acter> grappledBy = new List<Acter>();
	public Acter grappling;
	public ParticleSystem blood;
	public Color skinColor;
	public HealthBarController healthBar;
	public Animator Anim { get { return GetComponent<Animator>(); } }
	public CapsuleCollider weapon;
	public Transform torso;
	public Transform head;
	public Transform pelvis;
	string state;
	public string State { get { return state; } }
	protected bool shouldUseMainHand = false;
	protected bool shouldUseOffhand = false;
	protected bool isScrambling = false;
	public virtual bool ShouldScramble { set { isScrambling = value; } }
	protected bool shouldPickUpItem = false;
	public void ShouldPickUpItem () { shouldPickUpItem = true; }
	protected Dictionary<string, int> terrainCollisions = new Dictionary<string, int>();
	protected bool facingRight = false;
	public bool FacingRight { get { return facingRight; } }
	protected List<ItemController> eligiblePickups = new List<ItemController>();
	bool largeWeapon = false;
	public WeaponController bareHands;
	public DamageAnnouncer damageAnnouncer;
	CapsuleCollider fistSize;
	Dictionary<Transform, WeaponController> equippedArmor = new Dictionary<Transform, WeaponController>();
	List<WeaponController> equipASAP = new List<WeaponController>();
	public bool friendly = false;
	Coroutine pendingSpell;
	Coroutine attackFinishGuarantee;
	Coroutine decay;
	public Action<Acter> OnHitEffects = other => {};
	public Action OnFixedUpdate = () => {};
	Dictionary<SpriteRenderer, int> bodyParts = new Dictionary<SpriteRenderer, int>();
	bool isBlocking;
	public bool huge;
	public string Name { get { return name.Replace("(Clone)", ""); } }
	#endregion
	#region life cycle
	public static List<Acter> LivingActers { get { return CameraController.Instance.livingActers; } }
	void Awake() {
		healthBar = Instantiate(healthBar);
		healthBar.player = this;
		hitPoints = MaxHitPoints;
		ChangeSkinColor();
		if (fistSize != null) {
			Debug.LogError(name + " started without fists?!");
		}
		fistSize = Instantiate(weapon);
		fistSize.gameObject.SetActive(false);
		fistSize.name = "fist size";
		bareHands = Instantiate(bareHands);
		bareHands.GetComponent<CapsuleCollider>().height = fistSize.height;
		bareHands.GetComponent<CapsuleCollider>().radius = fistSize.radius;
		bareHands.GetComponent<CapsuleCollider>().center = fistSize.center;
		Equip(bareHands);
		damageAnnouncer = Instantiate(damageAnnouncer);
		damageAnnouncer.acter = this;
		damageAnnouncer.transform.parent = transform;
		LivingActers.Add(this);
		foreach (SpriteRenderer spr in GetComponentsInChildren<SpriteRenderer>())
		{
			if (!bodyParts.ContainsKey(spr)) bodyParts.Add(spr, spr.sortingOrder);
		}
		if (friendly && PlayerController.Instance != null && this != PlayerController.Instance) {
			bool danger = false;
			if (PlayerController.Instance.MainClass == C_WIZARD && PlayerController.Instance.EquippedSecondaryWeapon != null) {
				PlayerController.Instance.EquippedSecondaryWeapon.MapChildren(w => {
					if (w.name.Contains("fireball")) danger = true;
				});
			}
			if (!PlayerController.Instance.IsTerrifying) {
				PlayerController.Instance.Speak(danger ? "if i blow you up\ni'm sorry" : "nice to meet you");
			}
		}
	}
	public void ChangeSkinColor() {
		foreach (SpriteRenderer spr in GetComponentsInChildren<SpriteRenderer>())
		{
			if (spr.GetComponentInParent<WeaponController>() != null) continue;
			if (spr.tag == "SkinColored") continue;
			spr.color = skinColor;
		}
	}
	IEnumerator Decay () {
		yield return new WaitForSeconds(2);
		var enemy = GetComponent<EnemyController>();
		if (enemy != null) {
			foreach (var c in GetComponentsInChildren<SphereCollider>()) {
				if (!c.isTrigger) c.gameObject.SetActive(false);
			}
			PlayerController.Instance.ShouldScramble = false;
		}
//		head.parent = null;
//		head.transform.position = transform.position;
//		head.gameObject.AddComponent<SphereCollider>();
//		var r = head.gameObject.AddComponent<Rigidbody>();
//		r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
	}
	void OnDestroy () {
		if (bareHands != null) Destroy(bareHands.gameObject);
		if (fistSize != null) {
			Destroy(fistSize.gameObject);
		}
		if (healthBar != null) Destroy(healthBar.gameObject);
		LivingActers.Remove(this);
	}
	#endregion
	#region state and animation
	
	public void AnimationForBlockingStateDidFinish() {
		ExitState();
	}
	// the values are the animation names
	public const string ST_REST = "standing_breathing", ST_ATTACK = "Attack", ST_WALK = "walk", ST_HURT = "Hurt"
						, ST_DEAD = "Die", ST_CAST = "spell_charging", ST_CAKE = "pancake";
	protected void ExitState() {
		if (state == ST_DEAD) return;  // you're not getting out that easy
		GetComponent<Animator>().speed = 1;
		isBlocking = false;
		GetComponent<Rigidbody>().useGravity = true;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		state = ST_REST;
		if (attackFinishGuarantee != null) StopCoroutine(attackFinishGuarantee);
		EnterStateAndAnimation(ST_REST);
	}
	protected bool EnterStateAndAnimation(string what) {
		if (state == ST_DEAD) return false;
		
		switch (what) {
			case ST_ATTACK:
				if (state == ST_ATTACK) return false;
				if (state == ST_HURT) return false;
				if (state == ST_CAKE) return false;
//				attackActive = true;  this happens during animation (see AttackActiveFramesDid[Finish|Begin])
				if (shouldUseMainHand == shouldUseOffhand) {
					Debug.LogError("should " + (shouldUseMainHand ? "" : "not ") + "attack and cast spell at the same time");
				}
				if (Speed == SPEED_WHEN_PARALYZED) return false;
				
				if (shouldUseOffhand) {
					if (EquippedSecondaryWeapon == null) {
						Debug.LogError("shoulduseoffhand while no secondary weapon");
						shouldUseOffhand = false;
						return EnterStateAndAnimation(ST_REST);
					}
					else if (EquippedSecondaryWeapon.tag == "offhandweapon") {
						var wand = EquippedSecondaryWeapon.GetComponent<WandController>();
						if (wand != null && wand.charges < wand.maxCharges) {
							shouldUseOffhand = false;
							return false;
						}
						
						if (EquippedSecondaryWeapon.isSpellbook) {
							pendingSpell = StartCoroutine(CastSpell());
							attackFinishGuarantee = StartCoroutine(LeaveAttackStateGuarantee(5));
						}
						else {
							var animSpeed = Speed * EquippedSecondaryWeapon.speedCoefficient / 400;
							GetComponent<Animator>().speed = animSpeed;
							Anim.Play("shoot_offhand");
						}
					}
				}
				else {
					GetComponent<Animator>().speed = Speed * EquippedWeapon.speedCoefficient / 400;
					if (EquippedWeapon != null && EquippedWeapon.tag == "Throwable Weapon") {
						Anim.Play("throw");
					}
					else if (EquippedWeapon != null && EquippedWeapon.tag == "Shootable Weapon") {
						Anim.Play("shoot");
					}
					else {
						var strengthToSpeed = 1 - EquippedWeapon.speedCoefficient;
						if (strengthToSpeed > 0) {
							strengthToSpeed /= (float) Math.Sqrt(meleeMultiplier);
							GetComponent<Animator>().speed = Speed * (1 - strengthToSpeed) / 400;
						}
//						if (largeWeapon || huge) anim.Play("downward_attack");
						if (largeWeapon || huge || EquippedWeapon.tag == "slashing weapon") Anim.CrossFade("downward_attack", 0.1f);
						else Anim.Play(ST_ATTACK);
					}
				}
				shouldUseMainHand = shouldUseOffhand = false;
				if (attackFinishGuarantee == null) {
				    attackFinishGuarantee = StartCoroutine(LeaveAttackStateGuarantee(2));//.5f / anim.speed));
			  	}
				break;
			case ST_WALK:
				if (state == ST_ATTACK) return false;
				if (state == ST_HURT) return false;
				if (state == ST_CAKE) return false;
				if (Speed == SPEED_WHEN_PARALYZED) return false;
				if (state == ST_WALK) Anim.Play(ST_WALK);
				else Anim.CrossFade (ST_WALK, 0.1f);
				break;
			case ST_REST:
				// HURT and ATTACK are exited after their animations resolve, not here.
				if (state == ST_ATTACK) return false;
				if (state == ST_HURT) return false;
				if (state == ST_CAKE) return false;
				if (state == ST_CAST) return false;
				if (state == ST_REST) Anim.Play (largeWeapon ? "rest_with_large_weapon" : ST_REST);
				else Anim.CrossFade (largeWeapon ? "rest_with_large_weapon" : ST_REST, 0.1f);
				break;
			case ST_HURT:
				if (state == ST_CAKE) return false;
				isBlocking = false;
				GetComponent<Animator>().speed = 1;
				Anim.Play(ST_HURT);
				if (pendingSpell != null) {
					StopCoroutine(pendingSpell);
					pendingSpell = null;
				}
				attackFinishGuarantee = StartCoroutine(LeaveAttackStateGuarantee(1));//.5f / anim.speed));
				break;
			case ST_CAKE:
				isBlocking = false;
				GetComponent<Animator>().speed = 1;
				GetComponent<Rigidbody>().useGravity = false;
				GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
				Anim.Play(ST_CAKE);
				if (pendingSpell != null) {
					StopCoroutine(pendingSpell);
					pendingSpell = null;
				}
				attackFinishGuarantee = StartCoroutine(LeaveAttackStateGuarantee(1.5f));//.5f / anim.speed));
				break;
			case ST_DEAD:
				isBlocking = false;
				Anim.Play(ST_DEAD);
				if (pendingSpell != null) {
					StopCoroutine(pendingSpell);
					pendingSpell = null;
				}
				LivingActers.Remove(this);
				if (grappling != null) grappling.grappledBy.Remove(this);
				damageAnnouncer.AnnounceDeath();
				if (!GetComponent<PlayerController>()) {
					if (EquippedWeapon != bareHands) DropWeapon(EquippedWeapon);
					DropWeapon(EquippedSecondaryWeapon);
					ReleaseEquipment(GetArmor(torso));
					ReleaseEquipment(GetArmor(head));
					ReleaseEquipment(GetArmor(pelvis));
					ReleaseEquipment(GetArmor(GetSlot("frontArm")));
					ReleaseEquipment(GetArmor(GetSlot("frontShin")));
					ReleaseEquipment(GetArmor(GetSlot("backArm")));
					ReleaseEquipment(GetArmor(GetSlot("backShin")));
					foreach (var item in GetSlot("Head").GetComponentsInChildren<TrinketController>()) {
						item.transform.parent = null;
						item.GetComponent<BoxCollider>().enabled = true;
						item.GetComponent<Rigidbody>().isKinematic = false;
						
						var spr = item.GetComponent<SpriteRenderer>();
						if (spr != null) {
							spr.sortingLayerName = "Default";
						}
					}
				}
				equipASAP.Clear();
				GetComponent<Rigidbody>().mass = 0.1f;
				GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;
				GetComponent<Rigidbody>().useGravity = true;
				shouldUseMainHand = shouldPickUpItem = ShouldScramble = shouldUseOffhand = false;
				decay = StartCoroutine(Decay());
				break;
		}
		state = what;
		
		return true;
	}
	#endregion
	#region movement
	public virtual void EnterTerrainCallback(string terrType) { EnterTerrainCallback(terrType, 0); }
	public virtual void EnterTerrainCallback(string terrType, float damage)
	{
		EnterTerrainCallback(terrType, damage, WeaponController.DMG_NOT);
	}
	public virtual void EnterTerrainCallback(string terrType, float damage, int damageType) {
		if (damage > 0) {
			TakeDamage(damage, damageType);
		}
		if (terrainCollisions.ContainsKey(terrType))
		{
			terrainCollisions[terrType]++;
		}
		else
		{
			terrainCollisions[terrType] = 1;
		}
	}
	public void ExitTerrainCallback(string terrType) {
		//		if (terrainCollisions.ContainsKey(terrType)) terrainCollisions[terrType]--;		unreliable
		if (terrainCollisions.ContainsKey(terrType)) terrainCollisions[terrType] = 0;
	}
	
	protected void Flip ()
	{
		facingRight = !facingRight;
		var rot = transform.rotation;
		rot.y = facingRight ? 180 : 0;
		transform.rotation = rot;
	}
	
	protected void Move(Vector3 direction)
	{
		if (direction.x > 0 && !facingRight) Flip ();
		else if (direction.x < 0 && facingRight) Flip ();
		
		var _speed = Speed;
		if (isScrambling && grappledBy.Count == 0) _speed *= 1.5f;
		if (MainClass != C_GESTALT) {
			foreach (WeaponController armor in equippedArmor.Values) {
				var slowness = 1 - armor.speedCoefficient;
				slowness /= meleeMultiplier;
				_speed *= 1 - slowness;
			}
		}
		if (terrainCollisions.ContainsKey("water") && terrainCollisions["water"] > 0 && !isAquatic) _speed /= 2;
		direction *= _speed * Time.deltaTime;
		direction.y = GetComponent<Rigidbody>().velocity.y;
		
		if (direction.y > 0) direction.y = 0;			// don't skateboard jump off of ramps
		if (direction.y < -1) direction.y = -10;		// don't float off of ramps
		grappledBy.ForEach(e => {
			if (!freeAction) direction /= e.meleeMultiplier;
			if (Vector3.Distance(transform.position, e.transform.position) 
					> Vector3.Distance(transform.position, e.transform.position + direction)) {
				e.GetComponent<Rigidbody>().velocity = direction;
			}
		});
		GetComponent<Rigidbody>().velocity = direction;
	}
	#endregion
	#region equipment
	#region helper
	protected Transform GetSlot(string str) {
		var ch = GetComponentsInChildren<Transform>();
		var _ch = new List<Transform>(ch);
		return _ch.Find(t => t.name == str);
	}
	public void PickupIsEligible(ItemController item) {
		var wc = item as EstusController;
		if (wc != null && wc.charges <= 0) return;
		if (!eligiblePickups.Contains(item)) eligiblePickups.Add(item);
	}
	public void PickupIsIneligible(ItemController item) {
		eligiblePickups.Remove(item);
	}
	protected WeaponController GetArmor(Transform where) {
		return equippedArmor.ContainsKey(where) ? equippedArmor[where] : null;
	}
	#endregion
	#region weapons
	public WeaponController EquippedWeapon
	{
		get
		{
			var rval = weapon.GetComponentInChildren<WeaponController>();
			if (rval != null && !rval.IsEquipped) Debug.LogError(this + " equipped with an unequipped " + rval);
			return rval == null ? bareHands : rval;
		}
	}
	public WeaponController EquippedSecondaryWeapon
	{
		get
		{
			var frontHand = GetSlot("frontForeArm");
			if (frontHand == null) return null;		// sometimes possible when an enemy decays through the floor
			var rval = frontHand.GetComponentInChildren<WeaponController>();
			if (rval != null && !rval.IsEquipped) Debug.LogError(this + " equipped with an unequipped " + rval);
			return rval;
		}
	}
	WeaponController DropWeapon(WeaponController rval) {
		if (rval == null) return null;
		rval.attackActive = false;
		
		if (rval == EquippedWeapon) {
			weapon.radius = fistSize.radius;
			weapon.height = fistSize.height;
			weapon.center = fistSize.center;
			weapon.direction = fistSize.direction;
			largeWeapon = false;
			rval.PhantomRangeActive(true);
		}
		ReleaseEquipment(rval);
		return rval;
	}
	protected void EquipWeapon(WeaponController item) {
		if (item == null) return;	// it's been destroyed
		var isOffhand = item.tag == "offhandweapon";
		var _parent = isOffhand ? GetSlot("frontForeArm") : weapon.transform;
		DropWeapon(_parent.GetComponentInChildren<WeaponController>());
		item.transform.parent = _parent;
		AttachEquipment(item);
		if (isOffhand) {
			item.transform.localPosition = new Vector3(0, -.7f);
		}
		var spr = item.GetComponent<SpriteRenderer>();
		if (spr != null) {
			spr.sortingLayerName = "Character";
//			bodyParts.Add(spr, (isOffhand ? parent.GetComponent<SpriteRenderer>().sortingOrder
//			              : parent.GetComponentInParent<SpriteRenderer>().sortingOrder % 10) - 1);
//
		}
		if (!isOffhand) {
			if (item == bareHands) {
				weapon.radius = fistSize.radius;			// FIXME:  i don't think weapon is ever used
				weapon.height = fistSize.height;
				weapon.center = fistSize.center;
				weapon.direction = fistSize.direction;
			}
			else {
				weapon.radius = item.GetComponent<CapsuleCollider>().radius;
				weapon.direction = item.GetComponent<CapsuleCollider>().direction;
				var box = item.GetComponent<BoxCollider>();
				var maxDim = Mathf.Max(box.size.x, box.size.y, box.size.z);
				weapon.height = maxDim;
				weapon.center = box.center;
				item.PhantomRangeActive(false);
			}
			largeWeapon = item.tag == "Large Weapon";
		}
	}
	#endregion
	WeaponController ReleaseEquipment(WeaponController item) {
		if (item == null) return null;
		if (!item.IsEquipped) {
			Debug.LogError(this + " attempting to release unequipped " + item);
			return null;
		}
		if (equippedArmor.ContainsKey(item.transform.parent)) equippedArmor.Remove(item.transform.parent);
		item.transform.parent = null;
		item.transform.localScale = Vector3.one;
		if (item != bareHands) {
			item.GetComponent<BoxCollider>().enabled = true;
			item.GetComponent<Rigidbody>().isKinematic = false;
			item.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		else {
			item.transform.parent = transform;
		}
		var spr = item.GetComponent<SpriteRenderer>();
		if (spr != null) {
			spr.sortingLayerName = "Default";
//			bodyParts.Remove(spr);
		}
		return item;
	}
	
	void AttachEquipment(WeaponController item) {
		//		item.IsEquipped = true;
		item.transform.localPosition = Vector3.zero;
		item.transform.localRotation = Quaternion.identity;
		item.GetComponent<BoxCollider>().enabled = false;
		item.GetComponent<Rigidbody>().isKinematic = true;
	}
	
	void EquipArmor(WeaponController item, Transform where) {
		var previous = ReleaseEquipment(GetArmor(where));
		if (previous != null) armorClass -= previous.armorClass;
		item.transform.parent = where;
		AttachEquipment(item);
		item.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
//		item.GetComponent<SpriteRenderer>().sortingOrder = where.GetComponent<SpriteRenderer>().sortingOrder + 1;
//		var graphic = item.GetComponent<SpriteRenderer>();
//		if (graphic != null) bodyParts.Add(graphic, (where.GetComponent<SpriteRenderer>().sortingOrder % 10) + 1);
		armorClass += item.armorClass;
		equippedArmor[where] = item;
	}
	
	// avoid problems involving order of operations and Start().
	void _Equip (WeaponController item) {
		switch (item.bodySlot) {
			case WeaponController.EQ_WEAPON:
				EquipWeapon (item);
				break;
			case WeaponController.EQ_ARMOR:
				EquipArmor(item, torso);
				break;
			case WeaponController.EQ_HELM:
				EquipArmor(item, head);
				break;
			case WeaponController.EQ_SKIRT:
				EquipArmor(item, pelvis);
				break;
			case WeaponController.EQ_SHOULDER:
				var whichShoulder = WantsToEquipPauldronOrGreave(item);
				if (whichShoulder == "backArm") {
					item.transform.localScale = Vec.New(-1, 1);
				}
				if (whichShoulder != null) {
					var prevP = GetArmor(GetSlot(whichShoulder));
					EquipArmor(item, GetSlot(whichShoulder));
					var otherSh = prevP == null ? null : WantsToEquipPauldronOrGreave(prevP);
					if (otherSh != null) EquipArmor(prevP, GetSlot(otherSh));
				}
				else Debug.LogError("should want to equip " + item);
				break;
			case WeaponController.EQ_SHIN:
				var whichShin = WantsToEquipPauldronOrGreave(item);
				if (whichShin != null) {
					var prev = GetArmor(GetSlot(whichShin));
					EquipArmor(item, GetSlot(whichShin));
					var otherShin = prev == null ? null : WantsToEquipPauldronOrGreave(prev);
					if (otherShin != null) EquipArmor(prev, GetSlot(otherShin));
				}
				break;
			default:
				Debug.LogError("no such item slot: " + item.bodySlot);
				break;
		}
		equipASAP.Remove(item);
	}
	// avoid problems involving order of operations and Start().
	public void Equip(WeaponController item) {
		if (item == bareHands) {
			_Equip(bareHands);
			return;
		}
		
		if (item != null) {
			item.OnPickup(this);
			equipASAP.Add(item);
		}
		else {
			equipASAP.ForEach(i => _Equip(i));
		}
	}
	
	public bool HasSlotEquipped (int slot) {
		if (equipASAP.Find(e => e.bodySlot == slot) != null) return true;
		switch (slot) {
		case WeaponController.EQ_ARMOR:
			return equippedArmor.ContainsKey(torso);
		case WeaponController.EQ_HELM:
			return equippedArmor.ContainsKey(head);
		case WeaponController.EQ_SKIRT:
			return equippedArmor.ContainsKey(pelvis);
		case WeaponController.EQ_SHIN:
			return equippedArmor.ContainsKey(GetSlot("frontShin"));
		case WeaponController.EQ_SHOULDER:
			return equippedArmor.ContainsKey(GetSlot("frontArm"));
		case WeaponController.EQ_WEAPON:
			return EquippedWeapon != bareHands;
		default:
			Debug.LogError("broken switch in EnemyController.HasSlotEquipped, no key (" + slot + ")");
			return false;
		}
	}
	
//	bool playerHasHugenessHack = false;
	protected void EquipTrinket (TrinketController trinket) {
		if (isAquatic && trinket.waterWalking) return;
		if (freeAction && trinket.freeAction) return;
		if (trinket.npcSlowdown != 1 && CameraController.Instance.npcSpeedModifier != 1) return;
//		if (trinket.hugeness != 1 && playerHasHugenessHack) return;
		if (trinket.hugeness != 1 && GetSlot("backForeArm").GetComponentInChildren<TrinketController>()) return;
		
		trinket.OnPickup(this);
		trinket.GetComponent<BoxCollider>().enabled = false;
		trinket.GetComponent<Rigidbody>().isKinematic = true;
		trinket.transform.parent = head;
		trinket.transform.localPosition = new Vector3(UnityEngine.Random.Range(-.1f, .1f), UnityEngine.Random.Range(-.25f, .25f));
		trinket.transform.localRotation = Quaternion.identity;
		trinket.transform.localScale = Vector3.one;
		trinket.GetComponent<SpriteRenderer>().sortingLayerName = "Character";
		
		if (!isAquatic && trinket.waterWalking) {
			isAquatic = true;
			var foot = GetSlot("backFoot");
			trinket.transform.parent = foot;
			trinket.GetComponent<SpriteRenderer>().sortingOrder = foot.GetComponent<SpriteRenderer>().sortingOrder + 1;
		}
		if (!freeAction && trinket.freeAction) {
			freeAction = true;
			var foot = GetSlot("frontFoot");
			trinket.transform.parent = foot;
			trinket.GetComponent<SpriteRenderer>().sortingOrder = foot.GetComponent<SpriteRenderer>().sortingOrder + 1;
		}
		if (trinket.npcSlowdown != 1 && CameraController.Instance.npcSpeedModifier == 1) {
			CameraController.Instance.npcSpeedModifier *= trinket.npcSlowdown;
			var hand = GetSlot("frontForeArm");
			trinket.transform.parent = hand;
			trinket.GetComponent<SpriteRenderer>().sortingOrder = hand.GetComponent<SpriteRenderer>().sortingOrder + 1;
		}
		if (trinket.hugeness != 1) {
			Grow(trinket.hugeness);
			var hand = GetSlot("backForeArm");
			trinket.transform.parent = hand;
			trinket.GetComponent<SpriteRenderer>().sortingOrder = hand.GetComponent<SpriteRenderer>().sortingOrder + 1;
		}
		if (trinket.GetComponentInChildren<WeaponController>()) {
			var waist = GetSlot("pelvis");
			trinket.transform.parent = waist;
			trinket.GetComponent<SpriteRenderer>().sortingOrder = torso.GetComponent<SpriteRenderer>().sortingOrder + 3;
		}
//		trinket.GetComponent<SpriteRenderer>().sortingOrder = head.GetComponent<SpriteRenderer>().sortingOrder - 1;
		
		
		trinket.transform.localRotation = Quaternion.identity;
		trinket.transform.localPosition = Vector3.zero;
		
		meleeMultiplier += trinket.meleeMultiplier;
		armorClass += trinket.armorClass;
		speed += trinket.speed;
		spellpower += trinket.spellPower;
		CameraController.Instance.npcSpeedModifier *= trinket.npcSlowdown;
		if (trinket.regeneration != 0) BeginRegenerate(trinket.regeneration); 
		if (trinket.trapFinding) GameObject.FindObjectOfType<TerrainController>().ShowTraps = true;
		trinket.OnIdentify();
	}
	
	string WantsToEquipPauldronOrGreave (WeaponController w) {
		var affinity = w.SlotAffinity;
		while (true) {
			if (GetSlot(affinity) == null) return affinity;
			if (GetArmor(GetSlot(affinity)) == null) return affinity;
			var rval = w.Depth > GetArmor(GetSlot(affinity)).Depth;
			if (!rval && (affinity == "backArm" || affinity == "backShin")) {
//				print ("a " + affinity);
				affinity = affinity.Replace("back", "front");
//				print ("b " + affinity);
				//				print (GetArmor(GetSlot(affinity)));
				//				print (w + " vs " + GetArmor(GetSlot(w.SlotAffinity)));
				continue;
			}
			return rval ? affinity : null;
		}
	}
	
	public virtual bool WantsToEquip (WeaponController w) {
		if (w.IsEquipped) return false;
		if (MainClass == C_BRUTE && !w.IsArmor && !w.IsMeleeWeapon && w.GetComponent<EstusController>() == null) return false;
		
		if (!HasSlotEquipped(w.bodySlot)) return true;
		// armor
		var affinity = w.SlotAffinity;
		if (w.armorClass > 0) {
			if (GetSlot(affinity) == null) return true;
			if (GetArmor(GetSlot(affinity)) == null) return true;
			var rval = w.Depth > GetArmor(GetSlot(affinity)).Depth;
			if (!rval && (affinity == "backArm" || affinity == "backShin")) {
				rval = WantsToEquipPauldronOrGreave(w) != null;
			}
			return rval;
		}
		// weapons
		if (w.isSpellbook) return true;
		var comparedWeapon = w.IsOffhand ? EquippedSecondaryWeapon : EquippedWeapon;
		if (comparedWeapon == null) return true;

		if (comparedWeapon.Name != w.Name) {
			return true;
		}
		else if (w.charges > comparedWeapon.charges) return true;
		return w.Depth > comparedWeapon.Depth;
	}
	void TryGetItem () {
		if (shouldPickUpItem && eligiblePickups.Count > 0) {
			foreach(var eligible in eligiblePickups.FindAll(w => !w.IsEquipped)) {
				if (eligible.tag == "NonEquipmentItem") {		// food
					var food = eligible as FoodController;
					bool shouldEat = false;
					if (Heal(food.calories)) shouldEat = true;
					else {
						foreach (var ally in LivingActers.FindAll(a => a.friendly)) {
							if (ally.Heal(food.calories)) {
								shouldEat = true;
								break;
							}
						}
					}
					if (shouldEat) {
						eligible.gameObject.SetActive(false);
						eligiblePickups.Remove(eligible);
						food.EatMe(this);
						Destroy(eligible.gameObject);
						break;
					}
				}
				if (eligible.GetComponent<TrinketController>()) {
					eligiblePickups.Remove(eligible);
					EquipTrinket(eligible.GetComponent<TrinketController>());
					break;
				}
				var weapon = eligible as WeaponController;
				if (weapon == null) break;
				var cc = GameObject.FindObjectOfType<CameraController>();
				var subject = this is PlayerController ? "" : Name + " ";
				if (WantsToEquip(weapon)) {
					Equip(weapon);
					cc.NoteText(subject + "got " + weapon.Description);
					shouldPickUpItem = false;
					break;
				}
				foreach (var ally in LivingActers.FindAll(a => a.friendly && !(a is PlayerController))) {
					if (ally.WantsToEquip(weapon)) {
						ally.Equip(weapon);
						cc.NoteText(subject + "gave " + weapon.Description + " to " + ally.Name);
						shouldPickUpItem = false;
						break;
					}
				}
			}
		}
		if (this is PlayerController) shouldPickUpItem = false;
	}
	#endregion
	#region combat
	
	#region simple callbacks
	IEnumerator LeaveAttackStateGuarantee(float seconds) {
		yield return new WaitForSeconds(seconds);
		if (pendingSpell != null) {
			CameraController.Instance.NoteText(Name + " failed to cast " + EquippedSecondaryWeapon.payload.Description);
			StopCoroutine(pendingSpell);
			pendingSpell = null;
		}
		ExitState();
	}
	
	public virtual void AttackActiveFramesDidFinish() {
		EquippedWeapon.attackActive = false;
	}
	public virtual void AttackActiveFramesDidBegin() {
		if (state != ST_ATTACK) {
			Debug.LogError("expected AttackActiveFramesDidBegin to be canceled due to attack interrupted");
			return;
		}
		EquippedWeapon.attackVictims.Clear ();
		EquippedWeapon.attackActive = true;
	}
	public void FireMissileCallback () {
		if (state != ST_ATTACK) {
			Debug.LogError("expected AttackActiveFramesDidBegin to be canceled due to attack interrupted");
			return;
		}
		EquippedWeapon.attackVictims.Clear();
		if (EquippedWeapon.name.Contains("Brick")) {
			Debug.LogError("main hand bricks are broken!");
			ThrowWeapon(DropWeapon(EquippedWeapon));
			Equip(bareHands);
		}
		else if (EquippedWeapon.tag == "Shootable Weapon") {
			var p = Instantiate(EquippedWeapon.payload);
			p.gameObject.SetActive(true);
			ThrowWeapon(p);
		}
	}
	public void FireOffhandCallback () {
		if (state != ST_ATTACK) {
			Debug.LogError("expected AttackActiveFramesDidBegin to be canceled due to attack interrupted");
			return;
		}
		if (EquippedSecondaryWeapon == null) return;			// no idea
		if (EquippedSecondaryWeapon.thrownHorizontalMultiplier != 0) {
			var brick = DropWeapon(EquippedSecondaryWeapon);
			brick.transform.localPosition = Vector3.zero;		// world coordinate is added in ThrowWeapon
			ThrowWeapon(brick);
		}
		else if (EquippedSecondaryWeapon.payload != null) {
			var p = Instantiate(EquippedSecondaryWeapon.payload);
			p.gameObject.SetActive(true);
			ThrowWeapon(p, EquippedSecondaryWeapon.transform);
			if (EquippedSecondaryWeapon.charges != -1) --EquippedSecondaryWeapon.charges;
		}
		else {
			if (!EquippedSecondaryWeapon.name.Contains("Shield")) Debug.LogError("only shields should fall through to here");
			isBlocking = true;
		}
	}
	#endregion
	#region special effects
	public bool Heal(float qty) {
		if (hitPoints == MaxHitPoints && fireDamageTaken == 0) return false;
		if (State == ST_DEAD) return false;
		TakeDamage(qty, WeaponController.DMG_HEAL);
		return true;
	}
	const int SPEED_WHEN_PARALYZED = 1;
	IEnumerator EndParalyze(float duration, float normalSpeed) {
		yield return new WaitForSeconds(duration);
		speed = normalSpeed;
		damageAnnouncer.SetParalyzed(false);
		GetComponent<Animator>().speed = 1;
		shouldUseMainHand = shouldUseOffhand = false;
	}
	public void Paralyze(float magnitude) {
		if (freeAction) return;
		if (Speed == SPEED_WHEN_PARALYZED) return;
		magnitude = Mathf.Pow(magnitude, 0.33f) / Mathf.Pow(paralyzeScaling, 2);
//		if (magnitude < 0.5f) return;
		GetComponent<Rigidbody>().velocity = Vector3.zero;
		var _speed = speed;
		speed = SPEED_WHEN_PARALYZED;
		GetComponent<Animator>().speed = SPEED_WHEN_PARALYZED;
		ExitState();
		
		shouldUseMainHand = shouldUseOffhand = false;
		poiseBreakCounter = 0;
		damageAnnouncer.SetParalyzed(true);
		StartCoroutine(EndParalyze(magnitude, _speed));
		paralyzeScaling += magnitude;
	}
	bool Zombify() {
		if (State != ST_DEAD) return false;
		var me = GetComponent<EnemyController>();
		if (me != null) {
			if (me.isUnliving) return false;
			me.isUnliving = true;
		}
		else if (GetComponent<PlayerController>() == null) Debug.LogError("shouldn't zombify a " + this);
		
		StopCoroutine(decay);
		hitPoints = MaxHitPoints;
		state = ST_HURT;
		ExitState();
		transform.rotation = Quaternion.identity;
		facingRight = false;
		GetComponent<Rigidbody>().mass = 1;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		//		transform.position += new Vector3(0, 1);
		skinColor = Color.white;
		ChangeSkinColor();
		
		return true;
	}
	#endregion
	#region projectiles and spells
	public void ThrowWeapon(WeaponController projectile) {
		ThrowWeapon(projectile, weapon.transform);
	}
	public void ThrowWeapon(WeaponController projectile, Transform launchPoint) {
		projectile.thrownBy = this;
		projectile.attackActive = true;
		projectile.transform.position = launchPoint.position + projectile.transform.localPosition;
		projectile.Throw();
	}
	IEnumerator CastSpell() {
		if (EquippedSecondaryWeapon.tag != "offhandweapon") {
			Debug.LogError("attempted to cast a spell with " + EquippedSecondaryWeapon);
		}
		if (EquippedSecondaryWeapon.payload == null) {
			Debug.LogError(EquippedSecondaryWeapon + " has no payload");
		}
		else {
			Anim.Play(ST_CAST);
			float castTime = EquippedSecondaryWeapon.Depth / spellpower; //Mathf.Pow(spellpower, 1.5f);
			var wand = EquippedSecondaryWeapon.GetComponent<WandController>();
			var isWand = wand != null;
			if (isWand) {
				if (wand.charges < wand.maxCharges) Debug.LogError(wand.Name + " shouldn't have cast with " + wand.charges + "/" +
													wand.maxCharges + " charges!");
				castTime = 0;
			}
			yield return new WaitForSeconds(castTime);
			
			Anim.CrossFade("spell_complete", 0.5f);
			yield return new WaitForSeconds(0.5f);
			
			if (!isWand && EquippedSecondaryWeapon) {
				EquippedSecondaryWeapon.MapChildren(w => w.tag = "spell");
			}
			Action<WeaponController> Fire = wc => {
				wc = Instantiate(wc);
				wc.gameObject.SetActive(true);
				ThrowWeapon(wc, EquippedSecondaryWeapon.transform);
				if (wc.tag == "spell") {
					wc.ApplySpellpower();
				}
			};
			if (EquippedSecondaryWeapon.payload == null) {
				Debug.LogError("why???");
			} else {
				Fire(EquippedSecondaryWeapon.payload);
				foreach (var mp in EquippedSecondaryWeapon.multiPayload) {
					Fire(mp);
				}
			}
	//		var p = Instantiate(EquippedSecondaryWeapon.payload);
	//		p.gameObject.SetActive(true);
	//		ThrowWeapon(p, EquippedSecondaryWeapon.transform);
	//		if (p.tag == "spell") {
	//			p.ApplySpellpower();
	//		}
			if (isWand) wand.charges = 0;
			ExitState();
			pendingSpell = null;
		}
	}
	#endregion
	public virtual void TakeDamage(float quantity, int type)
	{
		if (type == WeaponController.DMG_PARA) {
			Paralyze(quantity);
			return;
		}
		if (type == WeaponController.DMG_RAISE) {
			return;	// handled in WeaponDidCollide() to avoid tramp variable
		}
//		print ("pre scaling " + quantity);
		
		quantity *= GLOBAL_DMG_SCALING;
		if (type == WeaponController.DMG_HEAL) {
			if (hitPoints == MaxHitPoints && fireDamageTaken == 0) return;
			if (State == ST_DEAD) return;
			
			hitPoints = Mathf.Min(MaxHitPoints, hitPoints + quantity);
			fireDamageTaken = Mathf.Max(fireDamageTaken - quantity, 0);
			return;
		}
		
		blood.Play ();
		
		if (type != WeaponController.DMG_DEATH) {
			quantity /= (armorClass * WeaponController.GLOBAL_ARMOR_SCALING * (type == WeaponController.DMG_FIRE ? 2 : 1));
		}
		if (type != WeaponController.DMG_POISE) {
			hitPoints -= quantity;
			damageAnnouncer.AnnounceDamage(quantity, type);
		}
		if (type == WeaponController.DMG_FIRE) fireDamageTaken += quantity;
//		print ("post scaling " + quantity + ", AC " + armorClass);
		
		
		if (hitPoints > 0 && (type == WeaponController.DMG_PHYS || type == WeaponController.DMG_POISE)
						 && Speed != SPEED_WHEN_PARALYZED) {
			if (state != ST_HURT && State != ST_CAKE) {
				poiseBreakCounter += quantity;
				if (poiseBreakCounter > Poise) {
					EnterStateAndAnimation(ST_HURT);
					poiseBreakCounter = 0;
				}
			}
			else {
				if (type != WeaponController.DMG_POISE) TakeDamage(1, WeaponController.DMG_DEATH);
				EnterStateAndAnimation(ST_CAKE);
			}
		}
		if (hitPoints <= 0 && state != ST_DEAD) {
			EnterStateAndAnimation(ST_DEAD);
		}
	}
	public void WeaponDidCollide(Acter other) { WeaponDidCollide(other, EquippedWeapon); }
	public void WeaponDidCollide(Acter other, WeaponController weaponController) { WeaponDidCollide(other, weaponController, false); }
	public virtual void WeaponDidCollide(Acter other, WeaponController weaponController, bool friendlyFireOK) {
		if (!friendlyFireOK && other.friendly == friendly) {
			return;
		}
		if (weaponController.damageType == WeaponController.DMG_RAISE) {
			var sufficientControl = true;
			if (friendly) {
				var existingAllies = LivingActers.FindAll(a => a.GetComponent<EnemyController>() != null)
											.ConvertAll(a => a as EnemyController);
				existingAllies.RemoveAll(a => !a.isUnliving);
				var controlledCR = 0;
				existingAllies.ForEach(a => controlledCR += a.ChallengeRating);
				if (controlledCR >= spellpower) {
					sufficientControl = false;
					if (other.State == ST_DEAD) {
						CameraController.Instance.NoteText(Name + " couldn't zombify " + other.Name + ", insufficient spellpower");
					}
				}
			}
			if (sufficientControl && other.Zombify()) {
				other.friendly = friendly;
				var enemy = other.GetComponent<EnemyController>();
				if (enemy != null && enemy.friendly) enemy.fleeDistance = 1;
			}
			
			// fall through to allow "raise dead damage" to ghosts
		}
		if (weaponController.damageType == WeaponController.DMG_PHYS && other.isBlocking) {
			if (weaponController.IsMeleeWeapon) {
				poiseBreakCounter += Poise * 2;
				other.WeaponDidCollide(this, other.EquippedSecondaryWeapon);
			}
			return;
		}
		
		var qty = weaponController.attackPower;
		if (!weaponController.IsProjectile || weapon.tag == "Throwable Weapon") {
			qty *= meleeMultiplier;
			if (weaponController.Name.Contains("iltless")) print (qty);
		}
		
		var prevState = other.State;
		other.TakeDamage(qty, weaponController.damageType);
		
		if (other.State == ST_DEAD && prevState != ST_DEAD) {
			
			var player = other.GetComponent<PlayerController>();
			if (player) {
				var str = player == this ? "commited suicide" : "killed by " + Name;
				str += "\nwith a " + weaponController.Root.MultiLineDescription;
				print (str);
				CameraController.Instance.AnnounceDeath(str);
				return;
			}
			
			var mob = other as EnemyController;
			if (mob != null && mob.friendly != friendly) {
				mob.AwardExpForMyDeath();
			}
		}
		
		OnHitEffects(other);
	}
		
	
	#endregion
	

	
	// returns whether to continue child's fixedupdate()
	protected virtual bool _FixedUpdate() {
//		if (state == null || state.Length == 0) state = ST_REST;
//		print (State);
		
		if (hitPoints > MaxHitPoints) hitPoints = MaxHitPoints;	// can happen with a curse
		healthBar.SetCurrentHP(hitPoints, racialBaseHitPoints);
		
		foreach (var kvp in bodyParts)
		{
			if (kvp.Key == null) continue;		// FIXME: why necessary?
			kvp.Key.sortingOrder = kvp.Value - (int)(transform.position.z * 10);
		}
		foreach (var eq in GetComponentsInChildren<ItemController>()) {
			eq.UpdateSortingOrder();
			eq.transform.localScale = new Vector3(1, Mathf.Sign(eq.transform.localScale.y), 1);
		}
		
		if (State == ST_DEAD) {
			return false;
		}
		
		grappledBy.RemoveAll(e => e == null);
		grappledBy.ForEach(e => TakeDamage(e.meleeMultiplier / 60, WeaponController.DMG_GRAP));
		
		if (EquippedSecondaryWeapon != null && EquippedSecondaryWeapon.GetComponent<EstusController>() != null &&
										 EquippedSecondaryWeapon.charges <= 0) {
			DropWeapon(EquippedSecondaryWeapon);
//			var spentFlask = DropWeapon(EquippedSecondaryWeapon);
//			var p = spentFlask.GetComponentInChildren<ParticleSystem>();
//			if (p != null) p.gameObject.SetActive(false);
		}
		
		if (!LivingActers.Contains(this)) LivingActers.Add(this);
		
		eligiblePickups.RemoveAll(shouldntHappenButDoes => shouldntHappenButDoes == null);	// food and stuff
		
		TryGetItem();
		
		Equip(null);
		poiseBreakCounter -= 1 / 60;
		
		if (Speed != SPEED_WHEN_PARALYZED) {
			paralyzeScaling = Mathf.Max (1, paralyzeScaling - 1/60);
		}// else print ("paralyze scaling " + paralyzeScaling);
		
		OnFixedUpdate();
		
		if ((State == ST_HURT || State == ST_ATTACK || State == ST_CAKE) && attackFinishGuarantee == null) {
			Debug.LogError (Name + " recovered from " + State + " coma");
			attackFinishGuarantee = StartCoroutine(LeaveAttackStateGuarantee(1));
		}
		
		if (State == ST_ATTACK) {
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			return false;
		}
		
		if (shouldUseMainHand && EnterStateAndAnimation(ST_ATTACK)) return false;
		if (shouldUseOffhand && EnterStateAndAnimation(ST_ATTACK)) return false;
		
		return true;
	}
	
	void OnTriggerStay (Collider collider) {
//		print (collider);
		if (collider.tag == "Wall") {
			print ("get back in there");
			var zDir = transform.position.z > 5? -1 : 1;
			transform.position = transform.position + new Vector3(0, 0, zDir);
		}
		
//		if (collider.tag == "Wall") {
//			var zDir = transform.position.z > 5? -1 : 1;
//			transform.position = transform.position + new Vector3(0, 0, zDir);
//		}
	}
}
