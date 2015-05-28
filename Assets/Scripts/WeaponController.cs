using UnityEngine;
using System.Collections;
using System.Collections.Generic;




public class WeaponController : ItemController {
	#region variables and constants
	public Acter thrownBy = null;
	public bool IsProjectile { get { return thrownBy != null; } }
	public bool IsArmor { get { return armorClass != 0; } }
	public bool IsMeleeWeapon { get { return attackPower != 0 && !IsProjectile; } }
	public override int Depth { get { return base.Depth + (payload == null ? 0 : payload.Depth); } }
	public bool deleteOnHitting = false;
	public bool deleteOnLanding = false;
	public int lifetime = -1;
	public int bodySlot;
	public const int DMG_NOT = 0, DMG_PHYS = 1, DMG_FIRE = 2, DMG_HEAL = 3, DMG_RAISE = 4, DMG_PARA = 5, DMG_DEATH = 6
					, DMG_GRAP = 7;
	public int damageType = DMG_NOT;
	public const int EQ_WEAPON = 0, EQ_ARMOR = 1, EQ_SKIRT = 2, EQ_HELM = 3, EQ_SHOULDER = 4, EQ_SHIN = 5;
	public string SlotAffinity {
		get {
			switch(bodySlot) {
				case EQ_ARMOR: return "torso";
				case EQ_SKIRT: return "pelvis";
				case EQ_HELM: return "Head";
				case EQ_SHOULDER: return "backArm";
				case EQ_SHIN : return "backLeg";
				default: return null;
			}	
		}
	}
	public WeaponController payload;
	public List<WeaponController> multiPayload = new List<WeaponController>();
	public bool isSpellbook;
	public bool firePayloadOnTimeout = false;
	public float thrownHorizontalMultiplier = 0;
	public float thrownVerticalMultiplier = 0;
	public bool friendlyFireActive = false;
	public List<Acter> attackVictims = new List<Acter>();
	public bool attackActive = false;
	public float armorClass;
	public float attackPower;
	public AudioClip impactNoise;
	public AudioClip firedNoise;
	public AudioClip constantNoise;
	public AudioSource audioSource;
	public int charges = -1;
	public float speedCoefficient = 1;		// attack speed for weapons, movement speed for armours
	#endregion
	#region life cycle
//	void Awake() { multiPayload.Clear(); }
	
	void Start() {
		if (firedNoise != null) {
			AudioSource.PlayClipAtPoint(firedNoise, transform.position, CameraController.Instance.Volume);
		}
		if (!IsProjectile) return;
		if (thrownHorizontalMultiplier == 0) {
			var offset = new Vector3(GetComponent<CapsuleCollider>().height / 2, 0);
			if (thrownBy.FacingRight) offset *= -1;
			transform.position -= offset;
		}
		
		if (thrownBy.FacingRight) {
			var rot = transform.rotation;
			var cc = GetComponent<CapsuleCollider>();
			if (cc.direction == 2) {		// pillars
				rot.x = -2/3f;
				transform.rotation = rot;
				var tmp = cc.center;
				tmp.x *= -1;
				cc.center = tmp;
				GetComponentInChildren<ParticleSystem>().transform.localPosition = tmp;
			}
			else if (transform.rotation.y == 0) {	// arrows
				rot.y += 180;
				transform.rotation = rot;
			}
		}
	}
	void OnDestroy() {		
//		Destroy(payload);		FIXME: this destoys the payload of the base instance
		payload = null;
	}
	#endregion
	#region update/trigger
	bool wtf = false;
	protected override void _FixedUpdate() {
		var emitter = GetComponentInChildren<ParticleSystem>();
		if (emitter != null) {
			var tmp = Quaternion.identity;
			tmp.x = -1;
			emitter.transform.rotation = tmp;
			if (!wtf) {
				wtf = true;
				var holyShitWhy = Instantiate(emitter);
				Destroy(holyShitWhy.gameObject);
			}
		}
		
		if (charges == 0 && lifetime == -1) {
			lifetime = 90;
		}
		
		if (lifetime != -1) --lifetime;
		if (lifetime == 0) {
			if (attackActive && firePayloadOnTimeout) FirePayload(GetComponent<CapsuleCollider>());
			Destroy(gameObject);
		}
		
		if (thrownVerticalMultiplier > 1) {
			GetComponent<Rigidbody>().AddForce(new Vector3 (0, Physics.gravity.y * (4 * thrownVerticalMultiplier)));
			if (deleteOnLanding && GetComponent<Rigidbody>().velocity.y <= 0) {
				deleteOnHitting = true;
			}
		}
		if (constantNoise != null) {
			if (audioSource == null) {
				Debug.LogError("constant noise requires AudioSource child object");
			}
			else if (!attackActive) audioSource.Stop();
			else if (!audioSource.isPlaying && audioSource.isActiveAndEnabled) {
				audioSource.volume = CameraController.Instance.Volume;
				audioSource.clip = constantNoise;
				audioSource.Play();
			}
			
		}
		base._FixedUpdate();
	}
	
	// attack
	void OnTriggerStay(Collider other) {
		if (!attackActive) return;
		if (other.tag == "breakable" && attackPower > 0) {
			other.GetComponentInParent<Breakable>().Break(this);
			FirePayload(other);
		}
		if (other.name.Contains("Tile") && deleteOnHitting) {
			FirePayload(other);
		}
		
		Acter victim = other.GetComponentInParent<Acter>();
		if (victim == null) return;
		
		var parent = IsProjectile ? thrownBy : GetComponentInParent<Acter>();
		
		var sg = other.GetComponent<ShieldGolem>();
		if (sg != null && !attackVictims.Contains(victim)) {
			if (sg.friendly != parent.friendly || friendlyFireActive) AudioSource.PlayClipAtPoint(sg.clang, transform.position);
			attackVictims.Add(victim);
		}
		
		if (other.name != "torso") return;
		
		if (attackVictims.Contains(victim)) return;
		attackVictims.Add(victim);
		
		if (parent == null) {
			if (friendlyFireActive) {
				victim.TakeDamage(attackPower, damageType);
			}
			return;
		}
		
		if (parent.friendly == victim.friendly && !friendlyFireActive) return;
		
		if (!IsProjectile && IsEquipped) {
			OnHit(victim, parent);
			if (victim != parent || friendlyFireActive) FirePayload(other);
		}
		else if (IsProjectile && (victim != thrownBy || friendlyFireActive)) {
			OnHit(victim, thrownBy);
			if (payload != null || deleteOnHitting) {
				FirePayload(other);
			}
		}
	}
	#endregion
	#region helper
	public void MapChildren (System.Action<WeaponController> Lambda) {
		var leaf = payload;
		while (leaf != null) {
			Lambda(leaf);
			foreach (var mp in leaf.multiPayload) {
				mp.MapChildren(Lambda);
			}
			leaf = leaf.payload;
		}
	}
	public void ApplySpellpower () {
		if (!thrownBy) { Debug.LogError("can't apply spellpower without knowing the caster"); return; }
		if (tag == "spell") attackPower *= Mathf.Pow(thrownBy.spellpower, .5f);
	}
	
	protected virtual void OnHit (Acter victim, Acter attacker) {
		attacker.WeaponDidCollide(victim, this, friendlyFireActive);
	}
	
	public string Description { get {
		var rval = "";
		if (attackPower != 0) {
			rval = "+" + attackPower + " ";
		}
		else if (armorClass != 0) {
			rval = "+" + armorClass + " ";
		}
		rval += name;
		if (payload != null) rval = rval + " of " + payload.Description;
		if (charges != -1) rval = rval + " (" + charges + " charges)";
		return rval.Replace("(Clone)", "");
	}}
	
	public void SpellEventDidFinish() {
		lifetime = 1;
	}
	#endregion
	public void Throw() {
		if (!IsProjectile) {
			Debug.LogError(this + " shouldn't be thrown!");
			return;
		}
		var velocity = new Vector3(50 * thrownHorizontalMultiplier / GetComponent<Rigidbody>().mass, 5f * thrownVerticalMultiplier);
		if (!thrownBy.FacingRight) velocity.x *= -1;
		GetComponent<Rigidbody>().velocity = velocity;
		
		// FIXME: this is how it SHOULD be done, but it doesn't work
//		var force = new Vector3(1500 * thrownHorizontalMultiplier, 500 * thrownVerticalMultiplier, 0);
//		if (!thrownBy.FacingRight) force.x *= -1;
//		GetComponent<Rigidbody>().AddForce(force);
	}
	
	void Fire(Collider impactPoint, WeaponController p) {
		if (impactPoint.name.Contains("Wall")) return;
		if (impactNoise != null) AudioSource.PlayClipAtPoint(impactNoise, transform.position, CameraController.Instance.Volume);
//		if (payload != null) {
//			var p = Instantiate(payload);
			var unburyFireball = p.name.Contains("fireball") && impactPoint.name.Contains("Tile");	// hax i know
			
			if (deleteOnHitting) {
				impactPoint = GetComponent<CapsuleCollider>();
			}
			if (impactPoint == null) {
				p.transform.position = transform.position + p.transform.localPosition;
			}
			else p.transform.position = impactPoint.transform.position + p.transform.localPosition;
			
			var wp = p.GetComponent<WeaponController>();
			if (wp != null) {
				wp.attackActive = true;
				wp.thrownBy = IsProjectile ? thrownBy : GetComponentInParent<Acter>();
				if (wp.thrownHorizontalMultiplier != 0) {
					wp.Throw();
				}
				if (unburyFireball) {
					wp.transform.position = wp.transform.position + new Vector3(0,3,0);
				}
			}
			if (wp.tag == "spell") {
				wp.ApplySpellpower();
			}
			
			p.gameObject.SetActive(true);
//		}
		attackActive = false;
		
		if (deleteOnHitting) {
			Destroy(gameObject);	
		}
	}
	
	
	void FirePayload(Collider impactPoint) {
		if (payload != null) {
			var pl = Instantiate(payload);
	//		pl.multiPayload.Clear();
	//		pl.multiPayload.AddRange(payload.multiPayload);
			
			Fire(impactPoint, pl);
		
	//		if (impactPoint.name.Contains("Wall")) return;
			multiPayload.ForEach(mp => {
				Fire (impactPoint, Instantiate(mp));
	//			mp.FirePayload(impactPoint);
			});
		}
		
		if (deleteOnHitting) {
			Destroy(gameObject);	
		}
	}
}
