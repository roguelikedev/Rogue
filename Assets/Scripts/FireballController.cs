﻿using UnityEngine;
using System.Collections;

public class FireballController : BroomController {
	float originalAttackPower = -1;
	
	protected override Vector3 Center {
		get {
			return transform.position;
		}
	}

	protected override void OnHit (Acter victim, Acter attacker)
	{
		if (originalAttackPower == -1) originalAttackPower = attackPower;
		var distance = victim.transform.position - transform.position;
		attackPower = originalAttackPower / Mathf.Max(1, Mathf.Pow(distance.magnitude, 2f));
		victim.TakeDamage(attackPower, DMG_POISE);
		base.OnHit (victim, attacker);
	}
}
