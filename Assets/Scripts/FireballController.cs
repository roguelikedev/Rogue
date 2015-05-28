using UnityEngine;
using System.Collections;

public class FireballController : WeaponController {
	float originalAttackPower = -1;

	protected override void OnHit (Acter victim, Acter attacker)
	{
		if (originalAttackPower == -1) originalAttackPower = attackPower;
		var distance = victim.transform.position - transform.position;
		attackPower = originalAttackPower / Mathf.Max(distance.magnitude, 0.25f);
		print(distance.magnitude + " " + attackPower + "/" + originalAttackPower);
		
		base.OnHit (victim, attacker);
	}
}
