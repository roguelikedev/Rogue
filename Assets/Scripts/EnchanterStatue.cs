using UnityEngine;
using System.Collections;

public class EnchanterStatue : Breakable {
	public float depth;

	public override bool Break (WeaponController byWhat)
	{
		if (byWhat.IsProjectile) return false;
		if (base.Break(byWhat)) {
			var hax = TerrainController.Instance.statuesDestroyed;
			var moreHax = SpawnController.Instance.stinginess;
			SpawnController.Instance.EnchantEquipment(byWhat, depth);
			CameraController.Instance.NoteText("enchanted " + byWhat.Description);
			TerrainController.Instance.statuesDestroyed = hax;
			SpawnController.Instance.stinginess = moreHax;
			CameraController.Instance.AnnounceText("empowered " + byWhat.Name);
			return true;
		}
		else return false;
	}
}
