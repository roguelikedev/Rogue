using UnityEngine;
using System.Collections;


public class SpellGenerator : MonoBehaviour {
	public ParticleSystem fireboltParticles;
	public ParticleSystem necroBeamParticles;
	public ParticleSystem necroPuffParticles;
	public ParticleSystem healingParticles;
	public ParticleSystem purpleParticles;
	public Animator expand;
	public Sprite arrow;
	public Sprite explosionSprite;
	public Sprite book1;
	public Sprite book2;
	public Sprite book3;
//	public Sprite wand;
	public AudioClip explosionSound;
	public AudioClip rippingSound;
	public AudioClip moanSound;
	public AudioClip fireSound;
	public AudioClip chimeSound;
	public WeaponController blankBook;
	public WeaponController blankSpell;
	public WeaponController blankWand;
	public FireballController fireball;
	public static SpellGenerator Instance() {
		return GameObject.FindObjectOfType<SpellGenerator>();
	}
	
	#region components
	WeaponController Defaults(int damageType, float radius, float height) {
		var rval = Instantiate(blankSpell);
		var cc = rval.GetComponent<CapsuleCollider>();
		cc.radius = radius;
		cc.height = height;
		
		ParticleSystem emitter;
		switch(damageType) {
			case WeaponController.DMG_FIRE:
				emitter = fireboltParticles;
				rval.name = "fire";
				rval.attackPower *= 1f;
				rval.constantNoise = fireSound;
				break;
			case WeaponController.DMG_DEATH:
				emitter = necroBeamParticles;
				rval.name = "death";
				rval.attackPower *= .1f;
				rval.firedNoise = moanSound;
				break;
			case WeaponController.DMG_RAISE:
				emitter = necroPuffParticles;
				rval.name = "raise";
				break;
			case WeaponController.DMG_PARA:
				emitter = purpleParticles;
				rval.damageType = WeaponController.DMG_PARA;
				rval.attackPower = 1;
				rval.name = "paralysis";
				break;
			default:
				Debug.LogError("fell through case statement");
				emitter = purpleParticles;
				break;
		}
		
		rval.damageType = damageType;
		emitter = Instantiate(emitter);
		emitter.emissionRate *= cc.height;
		emitter.transform.localScale = new Vector3(cc.height, 1, 1);
		emitter.transform.parent = rval.transform;
		rval.depth = 1;
		return rval;
	}
	
	public WeaponController Bolt(int damageType) { 
		var rval = Defaults(damageType, .5f, 3);
		
		rval.GetComponent<SpriteRenderer>().sprite = Instantiate(arrow);
		
		rval.thrownHorizontalMultiplier = 1;
		rval.lifetime = 20;
		rval.impactNoise = rippingSound;
		rval.name += "bolt";
		rval.attackPower *= 1.5f;
		rval.deleteOnHitting = true;
		
		return rval;
	}
	public WeaponController Beam(int damageType) { 
		var rval = Defaults(damageType, .9f, 10);
		
		rval.lifetime = 25;
		rval.firedNoise = moanSound;
		rval.name += "beam";
		
		return rval;
	}
	public WeaponController Wave(int damageType) {
		var radius = 2f;
		var rval = Defaults(damageType, radius, 2 * radius);
		var cc = rval.GetComponent<CapsuleCollider>();
		cc.center = cc.center + new Vector3(0, radius/-2);
		rval.thrownHorizontalMultiplier = 0.25f;
		rval.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		var floorhugger = rval.gameObject.AddComponent<BoxCollider>();
		floorhugger.size = new Vector3(0.1f, 0.1f, 0.1f);
		floorhugger.center = new Vector3(0, radius / -2);
		floorhugger.transform.parent = rval.transform;
		var grease = new PhysicMaterial();
		floorhugger.material = grease;
		grease.dynamicFriction = grease.staticFriction = 0;
		grease.frictionCombine = PhysicMaterialCombine.Minimum;
		rval.lifetime = 90;
		rval.name += "wave";
		return rval;
	}
	// FIXME: why can't i fucking well get the emitter out of Defaults() !!!!!!!!!!!!!!
	public WeaponController Pillar(int damageType) {
		var rval = Instantiate(blankSpell);
		
		var cc = rval.GetComponent<CapsuleCollider>();
		cc.transform.localPosition = Vector3.zero;
		cc.direction = 2;
		cc.radius = 2f;
		cc.height = 11;
		cc.center = cc.center + new Vector3(5, 0, 3);		// no idea why the X is necessary
		
		ParticleSystem emitter;
		switch(damageType) {
			case WeaponController.DMG_FIRE:
				emitter = fireboltParticles;
				rval.name = "fire";
				rval.attackPower *= 1;
				rval.constantNoise = fireSound;
				break;
			case WeaponController.DMG_DEATH:
				emitter = necroBeamParticles;
				rval.name = "death";
				rval.attackPower *= .1f;
				rval.constantNoise = moanSound;
				break;
			case WeaponController.DMG_RAISE:
				emitter = necroPuffParticles;
				rval.name = "raise";
				rval.attackPower = 0;
				break;
			case WeaponController.DMG_PARA:
				emitter = purpleParticles;
				rval.damageType = WeaponController.DMG_PARA;
				rval.attackPower = 1;
				rval.name = "paralysis";
				break;
			default:
				Debug.LogError("fell through case statement");
				emitter = purpleParticles;
				break;
		}
		
		emitter = Instantiate(emitter);
		emitter.transform.parent = rval.transform;
		emitter.transform.localScale = new Vector3(cc.radius * 2, cc.radius * 2, cc.height);
		emitter.emissionRate *= cc.height;
		emitter.transform.localPosition = cc.center;
		
		rval.lifetime = 25;
		rval.name += "pillar";
		rval.damageType = damageType;
		rval.depth = 1;
		
		return rval;
	}
	public WeaponController RaiseDead() { 
		var rval = Defaults(WeaponController.DMG_RAISE, 1, 1);  // set radius manually so that particle system looks right

		var cc = rval.GetComponent<CapsuleCollider>();
		cc.radius = 4f;
		
		rval.thrownHorizontalMultiplier = 0;
		rval.lifetime = 25;
		rval.firePayloadOnTimeout = true;
		rval.firedNoise = moanSound;
		rval.depth = 1;
		rval.attackPower = 0;
		
		rval.name += " dead";
		return rval;
	}
	public WeaponController Heal () {
		var rval = Instantiate(blankSpell);
		
		var cc = rval.GetComponent<CapsuleCollider>();
		cc.direction = 1;
		cc.radius = 6f;
		
		var emitter = Instantiate(healingParticles);
		emitter.transform.parent = rval.transform;
		emitter.emissionRate *= cc.radius;
		emitter.transform.localScale *= cc.radius;
		
		rval.lifetime = 20;
		rval.damageType = WeaponController.DMG_HEAL;
		
		rval.firedNoise = chimeSound;
//		rval.attackPower = .1f;
		rval.friendlyFireActive = true;
		rval.name = "healing";
		return rval;
	}
	public WeaponController Mortar(int damageType) {
		var mortar = Defaults(damageType, 1, 1);
		mortar.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		mortar.GetComponent<Rigidbody>().mass = 2.5f;
		mortar.attackPower *= 4f;
		mortar.thrownHorizontalMultiplier = .6f;
		mortar.thrownVerticalMultiplier = 10;
		mortar.name += "mortar";
		mortar.deleteOnLanding = true;
		return mortar;
	}
	public WeaponController Explosion () {
		var explosion = Instantiate(fireball);
		explosion.name = "fireball";		// fucking GUI doesn't support lower case
		
//		explosion.GetComponent<Animator>().runtimeAnimatorController = Instantiate(expand.runtimeAnimatorController);
//		explosion.GetComponent<SpriteRenderer>().sprite = explosionSprite;
//		
//		explosion.friendlyFireActive = true;
//		explosion.attackPower *= 2;
//		explosion.firePayloadOnTimeout = true;
//		explosion.firedNoise = explosionSound;
//		explosion.transform.rotation = Quaternion.identity;
//		explosion.damageType = WeaponController.DMG_FIRE;
		
		return explosion as WeaponController;
	}
	/// <summary> Split the payload of rval and apply velocity on Z axis. </summary>
	public WeaponController Fan (WeaponController rval, int count) {
		if (rval.payload == null) return rval;
		var baseOffset = 1f;
		for (int lcv = 0; lcv < count; ++lcv) {
			var clone = Instantiate(rval.payload);
			var offset = baseOffset;
			if (lcv % 2 == 0) offset *= -1;
			offset *= (1 + lcv / 2);
//			clone.transform.position = new Vector3(0, 0, offset);
			clone.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
			clone.thrownParralaxModifier = offset;
//			clone.GetComponent<Rigidbody>().velocity = clone.GetComponent<Rigidbody>().velocity + new Vector3 (0,0,offset);
			rval.multiPayload.Add(clone);
		}
		rval.payload.name = "fan(" + count + ")" + rval.payload.name;
		
		return rval;
	}
	/// <summary> Split the payload of rval. </summary>
	public WeaponController Split (WeaponController rval, int count) {
		if (rval.payload == null) return rval;
		var baseOffset = rval.GetComponent<CapsuleCollider>().radius * 2;
		for (int lcv = 0; lcv < count; ++lcv) {
			var clone = Instantiate(rval.payload);
			var offset = baseOffset;
			if (lcv % 2 == 0) offset *= -1;
			offset *= (1 + lcv / 2);
			clone.transform.position = new Vector3(0, 0, offset);
			rval.multiPayload.Add(clone);
		}
		rval.payload.name = "split(" + count + ")" + rval.payload.name;
		
		return rval;
	}
	#endregion
	
	void Awake() {
		blankBook.multiPayload.Clear();			// HOW DID THIS GET SO MANY SPLITS
	}
	
	public void Generate(int depth, WeaponController parent) {
		print (parent.Description + " passed into generate");
		bool initialSpellRanged = false;
		bool hasParalyze = false;
		bool hasRaiseDead = false;
		System.Func<WeaponController> RandomSpell = () => {
			var dmgType = WeaponController.DMG_NOT;
			while (dmgType == WeaponController.DMG_NOT) {
				switch(Random.Range(0, 3)) {
					case 0:
						dmgType = WeaponController.DMG_DEATH;
						break;
					case 1:
						dmgType = WeaponController.DMG_FIRE;
						break;
					case 2:
						if (hasParalyze) continue;
						hasParalyze = true;
						dmgType = WeaponController.DMG_PARA;
						break;
					default:
						Debug.LogError("broken switch statement in Generate() picking damage type");
						break;
				}
			}
			while (true) {
				switch(Random.Range(0, 8)) {
					case 0:
						initialSpellRanged = true;
						return Beam(dmgType);
					case 1:
						initialSpellRanged = true;
						return Bolt (dmgType);
					case 2:
						return Pillar (dmgType);
					case 3:
						return Explosion();
					case 4:
						if (hasRaiseDead) continue;
						hasRaiseDead = true;
						return RaiseDead();
					case 5:
						initialSpellRanged = true;
						return Mortar(dmgType);
					case 6:
						initialSpellRanged = true;
						return Wave(dmgType);
					case 7:
						return Heal();
					default:
						Debug.LogError("broken switch statement in Generate#RandomSpell!");
						return null;
				}
			}
		};
		
		var leaf = RandomSpell();	// FIXME: is this garbage collected?
		if (!initialSpellRanged) {
			leaf = RandomSpell();
		}
		parent.payload = leaf;
		
		if (leaf.damageType != WeaponController.DMG_RAISE) {
			var powerMultiplier = Random.Range(1, depth + 1);
			leaf.attackPower *= powerMultiplier;
			leaf.depth += powerMultiplier;
		}
		else leaf.depth++;
		
		if (Random.Range(0, 4) == 0) {
			var splits = Random.Range(1, depth);
			if (Random.Range(0,2) == 0) Split(parent, splits);
			else Fan(parent, splits);
			leaf.depth *= splits;
		}
		for (int lcv = leaf.depth; lcv < depth; ) {
			if (leaf.damageType != WeaponController.DMG_RAISE) {
				var powerMultiplier = Random.Range(1, depth + 1);
				leaf.attackPower *= powerMultiplier;
				leaf.depth += powerMultiplier;
			}
			else leaf.depth++;
			
			leaf.payload = RandomSpell();
			if (lcv < depth / 2 && Random.Range(0, 4) == 0) {
				var splits = Random.Range(1, depth / lcv);
				if (Random.Range(0,2) == 0) Split(leaf, splits);
				else Fan(leaf, splits);
				leaf.depth *= splits;
			}
			lcv += leaf.depth;			
			leaf = leaf.payload;
		}
	}
}
