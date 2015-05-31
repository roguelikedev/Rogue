using UnityEngine;
using System.Collections;

public class TrollController : EnemyController {
	public float regenRate;
	
	void Start() {
		BeginRegenerate(regenRate);
		armorClass += 2;
	}
	
	protected override bool _FixedUpdate ()
	{
		if (hitPoints < racialBaseHitPoints) fleeDistance = 4;
		if (hitPoints >= racialBaseHitPoints) fleeDistance = 0;
		return base._FixedUpdate ();
	}
}
