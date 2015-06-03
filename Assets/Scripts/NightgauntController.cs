using UnityEngine;
using System.Collections;
using Utilities.Geometry;

public class NightgauntController : EnemyController {
	protected override void ResetAggro ()
	{
		;
	}

	void Start () {
		var book = Instantiate(SpellGenerator.Instance().blankBook);
		book.payload = SpellGenerator.Instance().Pillar(WeaponController.DMG_FIRE);
		book.payload.transform.position = ((Vec)book.payload.transform.position) + Vec.New(0, -7, 0);
		book.payload.transform.position = ((Vec)book.payload.transform.position).UnitY;
		book.payload.depth = 1;
		book.payload.attackPower *= 5;
		Equip(book);
		armorClass += 3;
	}

	protected override bool _FixedUpdate ()
	{
		GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
		return base._FixedUpdate ();
	}
}
