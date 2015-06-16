using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class BroomController : WeaponController {
	protected override void _FixedUpdate ()
	{
		if (attackActive) {
			foreach (var victim in attackVictims) {
				if (victim == null) continue;
				
				Vec direction;
				if (Parent != null) {
				 	direction = (victim.transform.position - Parent.transform.position);
					if (victim.friendly == Parent.friendly) continue;
				}
				else direction = victim.transform.position - transform.position;		// it's a trap
				direction.z = 0;
				direction.y = Mathf.Abs(direction.y);
				direction = direction.Normalish;
				if (direction.Len3 > 9) {
					attackVictims.Clear();
					return;
				}
				direction /= Mathf.Max(3f, Mathf.Sqrt(victim.Poise));
				victim.transform.position = victim.transform.position + direction;
			}
		}
		else attackVictims.Clear();
		
		base._FixedUpdate ();
	}
}
