using UnityEngine;
using System.Collections;

public class EnchanterStatue : Breakable {
	public float depth;

	public override bool Break (WeaponController byWhat)
	{
		if (byWhat.IsProjectile) return false;
		if (base.Break(byWhat)) {
			SpawnController.Instance.EnchantEquipment(byWhat, depth);
			CameraController.Instance.NoteText("enchanted " + byWhat.Description);
			return true;
		}
		else return false;
	}
}
