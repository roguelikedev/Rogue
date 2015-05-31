using UnityEngine;
using System.Collections;

public class OgreController : EnemyController {
	void Start () {
		var weapon = Instantiate(SpawnController.Instance.itemBarMace);
		if (WantsToEquip(weapon)) Equip(weapon);
		else Destroy(weapon.gameObject);
	}

	protected override bool _FixedUpdate ()
	{
		poiseBreakCounter = 0;
		return base._FixedUpdate ();
	}
}
