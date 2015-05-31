using UnityEngine;
using System.Collections;

public class IronLichController : DemonController {
	void Start() {
		InitializeAura(WeaponController.DMG_DEATH, auraDamage);
		damageAura.GetComponent<CapsuleCollider>().radius *= 2;
		var book = Instantiate(SpellGenerator.Instance().blankBook);
		book.payload = SpellGenerator.Instance().Beam(WeaponController.DMG_FIRE);
		book.payload.depth = 1;	// make this always choose to cast rather than ineffectual melee
		Equip (book);
		book.GetComponent<SpriteRenderer>().color = Color.clear;
		armorClass += 10;
	}
	public override void TakeDamage (float quantity, int type)
	{
		base.TakeDamage (quantity, type);
		if (State == ST_DEAD) {
			CameraController.Instance.AnnounceText("YOU HAVE WON\nrevenge is sweet");
		}
	}
}
