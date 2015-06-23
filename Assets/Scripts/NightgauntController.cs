using UnityEngine;
using System.Collections;
using Utilities.Geometry;

public class NightgauntController : EnemyController {
	protected override void ResetAggro ()
	{
		;
	}

	void Start () {
		var book = Instantiate(SpellGenerator.Instance.blankBook);
		book.GetComponent<SpriteRenderer>().color = Color.clear;
		book.payload = SpellGenerator.Instance.Pillar(WeaponController.DMG_FIRE);
		book.payload.transform.position = ((Vec)book.payload.transform.position) + Vec.New(0, -7, 0);
		book.payload.transform.position = ((Vec)book.payload.transform.position).UnitY;
		book.payload.depth = 1;
		book.payload.attackPower *= 5;
		Equip(book);
		hasBook = true;
		armorClass += 3;
	}

	bool hasBook = false;
	protected override bool _FixedUpdate ()
	{
		GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
//		if (State == ST_DEAD && hasBook) {
//			hasBook = false;
//			Destroy(EquippedSecondaryWeapon.gameObject);
//		}
		return base._FixedUpdate ();
	}
}
