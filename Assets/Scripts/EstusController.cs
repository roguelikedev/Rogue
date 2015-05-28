using UnityEngine;
using System.Collections;

public class EstusController : WeaponController {

	protected override void _FixedUpdate ()
	{
		if (payload == null) {
			payload = SpellGenerator.Instance().Heal();
			payload.attackPower = 1;
		}
		base._FixedUpdate ();
	}
}
