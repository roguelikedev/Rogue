using UnityEngine;
using System.Collections;

public class FireWalkerController : PlayerController {
	float fireballBaseDamage = -1;
	FireballController startingFireball;

	public override void HasExploredNewRoom ()
	{
		var book = EquippedSecondaryWeapon;
		if (book != null && book.payload != null && book.payload is FireballController) {
			if (startingFireball == null) {
				startingFireball = book.payload as FireballController;
				fireballBaseDamage = book.payload.attackPower;
			}
			if (book.payload == startingFireball) {
				book.payload.attackPower = fireballBaseDamage * Mathf.Max(1, TerrainController.Instance.Depth);
			}
		}
		
		base.HasExploredNewRoom ();
	}

	public override void TakeDamage (float quantity, int type)
	{
		if (type == WeaponController.DMG_FIRE) return;
		base.TakeDamage (quantity, type);
	}

}
