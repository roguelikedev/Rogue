using UnityEngine;
using System.Collections;

public class EstusController : WeaponController {

	void Start () {
		charges = Mathf.Max(5, Random.Range(0, TerrainController.Instance.Depth));
	}

	protected override void _FixedUpdate ()
	{
		if (payload == null) {
			payload = SpellGenerator.Instance().Heal();
			payload.attackPower = 1;
		}
		
		if (charges == 0 && lifetime == -1) {
			lifetime = 90;
		}
		
		base._FixedUpdate ();
	}
}
