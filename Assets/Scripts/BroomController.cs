using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class BroomController : WeaponController {
	protected override void _FixedUpdate ()
	{
		if (attackActive) {
			foreach (var victim in attackVictims) {
				if (victim == null || victim.friendly == Parent.friendly) continue;
				var parent = GetComponentInParent<Acter>();
				if (parent == null) parent = thrownBy;
				Vec direction = (victim.transform.position - parent.transform.position);
				direction.z = 0;
				direction.y = Mathf.Abs(direction.y);
				direction = direction.Normalish;
				if (direction.Len3 > 9) {
					attackVictims.Clear();
					return;
				}
				direction *= 1 / Mathf.Max(3f, victim.Poise);
				victim.transform.position = victim.transform.position + direction;
			}
		}
		else attackVictims.Clear();
		
		base._FixedUpdate ();
	}
}
