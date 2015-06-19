using UnityEngine;
using System.Collections;

public class FireWalkerController : PlayerController {
	float fireballBaseDamage = -1;
	FireballController startingFireball;

	public override void HasExploredNewRoom ()
	{
		var book = EquippedSecondaryWeapon;
		print (book != null);
		print ("pl " + (book.payload != null));
		print ("fb " + book.payload.payload + (book.payload.payload is FireballController));
		if (book != null && book.payload != null && book.payload.payload != null && book.payload.payload is FireballController) {
			print ("fuck me!!!");
			if (startingFireball == null) {
				startingFireball = book.payload.payload as FireballController;
				fireballBaseDamage = book.payload.payload.attackPower;
			}
			var prev = book.Description;
			book.payload.payload.attackPower = fireballBaseDamage * Mathf.Max(1, TerrainController.Instance.Depth);
			if (prev != book.Description) announcer.NoteText(prev + " became " + book.Description);
		}
		
		base.HasExploredNewRoom ();
	}

	public override void TakeDamage (float quantity, int type)
	{
		if (type == WeaponController.DMG_FIRE) {
			return;
		}
		base.TakeDamage (quantity, type);
	}

}
