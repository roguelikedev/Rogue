using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BroomController : WeaponController {
	protected override void _FixedUpdate ()
	{
		if (attackActive) {
			foreach (var victim in attackVictims) {
				var parent = GetComponentInParent<Acter>();
				if (parent == null) parent = thrownBy;
				var direction = (victim.transform.position - parent.transform.position).normalized;
				var fuckTheseVectors = 1 / Mathf.Max(1, victim.Poise);
				direction.Scale(new Vector3(fuckTheseVectors, fuckTheseVectors, fuckTheseVectors));
				victim.transform.position = victim.transform.position + direction;
			}
		}
		base._FixedUpdate ();
	}
}
