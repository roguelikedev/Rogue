using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyController : Acter, IDepthSelectable
{
	public int depth;
	public int racialLevel;
	public float commonness = 10f;
	const float playerFollowDistance = 4f;
	public float fleeDistance = 0f;
	public bool isUnliving;
	bool shouldFlee;
	bool isThreateningPlayer;
	protected bool ShouldBlock { get {
		return hitPoints < MaxHitPoints / 2
			    && EquippedSecondaryWeapon != null && EquippedSecondaryWeapon.name.Contains("Shield");
	} }
	
	SphereCollider FootSize { get { 
		return new List<SphereCollider>(GetComponentsInChildren<SphereCollider>()).Find(c => !c.isTrigger);
	} }
	bool OffhandIsSuperior { get {
		if (EquippedSecondaryWeapon == null) return false;
		// note that bows and hand crossbows are depth -1, but their ammunition has depth
		// hiltless is also depth -1, but AI shouldn't be using it except as last resort anyway
		var offhandSuperior = EquippedSecondaryWeapon.Depth > EquippedWeapon.Depth;
		if (EquippedSecondaryWeapon.name.Contains("Shield")) offhandSuperior = ShouldBlock;
		if (EquippedSecondaryWeapon.charges == 0) offhandSuperior = false;
		var wand = EquippedSecondaryWeapon.GetComponent<WandController>();
		if (wand != null && wand.charges < wand.maxCharges) offhandSuperior = false;
		return offhandSuperior;
	} }
	

	#region pathing
	List<Vector3> trapLocations = new List<Vector3>();
	public void TrapIsExploding (Vector3 where, bool whether) {
		if (whether) trapLocations.Add(where);
		else {
			if (trapLocations.Contains(where)) trapLocations.Remove(where);
			else Debug.LogError("no known trap at " + where);
		}
	}
	
	Vector3 DirectionFromTrap() {
		Vector3 closest = Vector3.zero;
		foreach (Vector3 a in trapLocations) {
			var delta = a - transform.position;
			delta.z = transform.position.z;
			if ((closest == Vector3.zero || delta.magnitude < closest.magnitude) && delta.magnitude < 8) {
				closest = delta;
			}
		}
		return transform.position - closest;
	}
	
	Vector3 DirectionToFoe() {
		Vector3 targetLocn = transform.position;
		Vector3 closest = Vector3.zero;
		foreach (Acter a in LivingActers) {
			if (friendly == a.friendly || a.State == ST_DEAD) continue;
			var delta = a.transform.position - transform.position;
			if (closest == Vector3.zero || delta.magnitude < closest.magnitude) {
				closest = delta;
				if (delta.magnitude > fleeDistance * 2f) shouldFlee = false;
				if (delta.magnitude < fleeDistance) shouldFlee = true;
				if (shouldFlee) {
					targetLocn = transform.position - delta;
					targetLocn.z = a.transform.position.z;
				}
				else targetLocn = a.transform.position;
			}
		}
		if (closest != Vector3.zero && !shouldFlee) {
			var offset = .5f + FootSize.radius;
			if (Mathf.Abs(targetLocn.x - transform.position.x) < offset) {
				if (Mathf.Abs(targetLocn.z - transform.position.z) > .1f) {
					var targetTowardLeft = targetLocn.x < transform.position.x;// - .25f;
					if (targetTowardLeft) targetLocn.x += offset;
					else targetLocn.x -= offset;
				}
			}
		}
		return targetLocn;
	}
	
	Vector3 DirectionToFriend() {
		var targetLocn = transform.position;
		Acter a = LivingActers.Find(p => p.GetComponent<PlayerController>() != null);
		if (a != null && a.friendly == friendly) {
			var delta = transform.position - a.transform.position;
			if (delta.magnitude < playerFollowDistance) targetLocn = -a.transform.position;
			else if (delta.magnitude > playerFollowDistance * 3f) targetLocn = a.transform.position;
		}
		return targetLocn;
	}
	
	Vector3 DirectionToItem() {
		var targetLocn = transform.position;
		var closest = Vector3.zero;
		var items = GameObject.FindObjectsOfType<WeaponController>();
		foreach (var item in items) {
			if (!WantsToEquip(item)) continue;
			var delta = item.transform.position - transform.position;
			if (delta.magnitude < 4000 / speed && closest == Vector3.zero || delta.magnitude < closest.magnitude) {
				closest = delta;
				targetLocn = item.transform.position;
			}
		}
		return targetLocn;
	}

	protected int stopRunningSlowly;
	Vector3 lastDirection = Vector3.zero;
	protected virtual Vector3 DirectionToTarget() {
		if (stopRunningSlowly-- > 0) {
			return lastDirection;
		}
		Vector3 targetLocn = transform.position;
		targetLocn = DirectionFromTrap();
		if (targetLocn == transform.position) targetLocn = DirectionToFoe();
		if (targetLocn == transform.position) {
			if (shouldPickUpItem) {
				targetLocn = DirectionToItem();
				if (targetLocn == transform.position) shouldPickUpItem = false;
			}
			else {
				targetLocn = DirectionToFriend();
			}
		}
		
		
		
//		targetLocn = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform> ().position;
		var rval = Vector3.zero;
		var posn = transform.position;

		// the outer test prevents the AI from rapidly and repeatedly flipping when on equal X axis to the player
		if (Mathf.Abs (posn.x - targetLocn.x) > .1f) {
			if (posn.x < targetLocn.x)
				rval.x = 1;
			else
				rval.x = -1;
		}
//		if (Mathf.Abs (posn.z - targetLocn.z) > 1) {
			if (posn.z < targetLocn.z)
				rval.z = 1;
			else if (posn.z > targetLocn.z)
				rval.z = -1;
//		}
		stopRunningSlowly = 15;
		lastDirection = rval;
		return rval;
	}
	#endregion
	#region properties
	bool PrimaryWeaponIsRanged {
		get
		{
			return EquippedWeapon != null && (EquippedWeapon.tag == "Shootable Weapon" || EquippedWeapon.tag == "Throwable Weapon");
		}
	}
	bool SecondaryWeaponIsRanged {
		get
		{
			return (EquippedSecondaryWeapon != null && EquippedSecondaryWeapon.payload != null
				&& (EquippedSecondaryWeapon.payload.GetComponent<CapsuleCollider>().height > 5
			    || EquippedSecondaryWeapon.payload.thrownHorizontalMultiplier > 0));
		}
	}
	public int ChallengeRating { get { return racialLevel + level; } }
	public void AwardExpForMyDeath () {
		LivingActers.FindAll(a => a.friendly != friendly).ForEach(a => {
			a.RewardExperience(ChallengeRating);
		});
	}
	public int Depth { get { return depth; } }
	public float Commonness { get { return commonness; } }
	#endregion
	
	public void Threatens(Acter who) {
		if (shouldUseMainHand || shouldUseOffhand) return;
		if (State == ST_ATTACK) return;
		
		var distance = Vector3.Distance(transform.position, who.transform.position);
		if (distance < fleeDistance) return;
		
		var weaponSize = Mathf.Max(weapon.height, weapon.radius);
		// secondary weapon is a brick or something
		if ((EquippedSecondaryWeapon != null && EquippedWeapon.IsMeleeWeapon && distance > weaponSize
			&& EquippedSecondaryWeapon.charges != 0) || OffhandIsSuperior) {
			shouldUseOffhand = true;
		}
		else {
			shouldUseMainHand = true;
		}
		
		if (EquippedSecondaryWeapon != null && EquippedSecondaryWeapon.name.Contains("Shield")) {
			shouldUseOffhand = ShouldBlock;
			shouldUseMainHand = !shouldUseOffhand;
		}
		
		
		if (EquippedWeapon.IsMeleeWeapon && shouldUseMainHand) {
			who.ShouldScramble = true;
			if (who is PlayerController) isThreateningPlayer = true;
		}
	}
	
	public override void AttackActiveFramesDidFinish ()
	{
		base.AttackActiveFramesDidFinish ();
		PlayerController.Instance.ShouldScramble = false;
		isThreateningPlayer = false;
	}

	protected virtual void ResetAggro () {
		var aggroSize = GetComponentInChildren<AggroController>().GetComponent<CapsuleCollider>();
		if (PrimaryWeaponIsRanged || (SecondaryWeaponIsRanged && !EquippedSecondaryWeapon.name.Contains("wand"))) {
			aggroSize.direction = 0;
			aggroSize.center = new Vector3(-5, 0);
			aggroSize.height = 10;
			fleeDistance = 4f;
		}
		else {
			GetComponentInChildren<AggroController>().Reinitialize();
			if (!friendly) fleeDistance = 0f;
			var rememberMe = "debug";
//			aggroSize.center = aggroSize.center + new Vector3(EquippedWeapon.
		}
	}
	
	void FixedUpdate() {
//		print ("flee " + shouldFlee + " main " + shouldUseMainHand + " off " + shouldUseOffhand);
		if (friendly && PlayerController.Instance.friendless) friendly = false;
		if (isThreateningPlayer && !shouldUseMainHand && State != ST_ATTACK) {
			if (isThreateningPlayer) PlayerController.Instance.ShouldScramble = false;
			isThreateningPlayer = false;
		}
	
		if (transform.position.y < -2) {
			if (State != ST_DEAD) AwardExpForMyDeath();
			Destroy(gameObject);
			return;
		}

		if (EquippedSecondaryWeapon != null) {
			var wand = EquippedSecondaryWeapon.GetComponent<WandController>();
			if (wand != null) {
				shouldUseOffhand = wand.charges == wand.maxCharges;
			}
			if (EquippedSecondaryWeapon.charges == 0) shouldUseOffhand = false;
			else if (EquippedSecondaryWeapon.GetComponent<EstusController>() != null
			         && State != ST_ATTACK) {	// don't want to queue up another use while draining the last charge
				shouldUseOffhand = hitPoints < MaxHitPoints / 2;
			}
		}
		
		if (!_FixedUpdate()) return;
		
		if (stopRunningSlowly <= 1) ResetAggro();
		damageAnnouncer.SetFriendly(friendly);
		
		var dir = DirectionToTarget ();
		if (dir != Vector3.zero) {
			if (EnterStateAndAnimation(ST_WALK)) Move(dir);
		}
		else {
			Move (Vector3.zero);
			EnterStateAndAnimation(ST_REST);
		}
	}
	
	public override bool ShouldScramble {
		set {
			if (value && ShouldBlock) {
				shouldUseMainHand = false;
				shouldUseOffhand = true;
			}
		}
	}
}
