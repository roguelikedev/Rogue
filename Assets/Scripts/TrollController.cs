using UnityEngine;
using System.Collections;

public class TrollController : EnemyController {
	public float regenRate;
	
	void Start() {
		BeginRegenerate(regenRate);
	}
	
	protected override bool _FixedUpdate ()
	{
		if (hitPoints < racialBaseHitPoints / 2) fleeDistance = 4;
		if (hitPoints >= racialBaseHitPoints) fleeDistance = 0;
		return base._FixedUpdate ();
	}
}
