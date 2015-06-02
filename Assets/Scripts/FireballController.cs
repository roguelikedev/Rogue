using UnityEngine;
using System.Collections;

public class FireballController : WeaponController {
	float originalAttackPower = -1;

	protected override void OnHit (Acter victim, Acter attacker)
	{
		if (originalAttackPower == -1) originalAttackPower = attackPower;
		var distance = victim.transform.position - transform.position;
		attackPower = originalAttackPower / Mathf.Max(1, Mathf.Pow(distance.magnitude, 2f));
		print(distance.magnitude + " distance reduces " + originalAttackPower + " to " + attackPower);
		
		base.OnHit (victim, attacker);
	}
}
