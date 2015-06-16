using UnityEngine;
using System.Collections;

public class EnchanterStatue : Breakable {
	public float depth;

	public override bool Break (WeaponController byWhat)
	{
		if (byWhat.IsProjectile) return false;
		if (base.Break(byWhat)) {
			var hax = TerrainController.Instance.statuesDestroyed;
			SpawnController.Instance.EnchantEquipment(byWhat, depth);
			CameraController.Instance.NoteText("enchanted " + byWhat.Description);
			TerrainController.Instance.statuesDestroyed = hax;
			CameraController.Instance.AnnounceText("empowered " + byWhat.Name);
			return true;
		}
		else return false;
	}
}
