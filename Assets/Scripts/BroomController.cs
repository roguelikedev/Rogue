using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BroomController : WeaponController {
	protected override void _FixedUpdate ()
	{
		if (attackActive) {
			foreach (var victim in attackVictims) {
				if (victim == null || victim.friendly == Parent.friendly) continue;
				var parent = GetComponentInParent<Acter>();
				if (parent == null) parent = thrownBy;
				var direction = (victim.transform.position - parent.transform.position);
				if (direction.magnitude > 9) {
					attackVictims.Clear();
					return;
				}
				direction.y = 0;
				direction.Normalize();
				var fuckTheseVectors = 1 / Mathf.Max(3f, victim.Poise);
				direction.Scale(new Vector3(fuckTheseVectors, fuckTheseVectors, fuckTheseVectors));
				victim.transform.position = victim.transform.position + direction;
			}
		}
		else attackVictims.Clear();
		
		base._FixedUpdate ();
	}
}
