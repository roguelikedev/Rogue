using UnityEngine;
using System.Collections;

public class TrollController : EnemyController {
	public float regenRate;
	public float freeArmor;
	
	void Start() {
		BeginRegenerate (regenRate);
		armorClass += freeArmor;
	}
	
	protected override bool _FixedUpdate ()
	{
		if (hitPoints < MaxHitPoints / 2) fleeDistance = 9;
		else fleeDistance = 0;
		return base._FixedUpdate ();
	}
}
