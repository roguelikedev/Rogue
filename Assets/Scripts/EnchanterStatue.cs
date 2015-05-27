using UnityEngine;
using System.Collections;

public class EnchanterStatue : Breakable {
	public float depth;

	public override void Break (WeaponController byWhat)
	{
		if (byWhat.IsProjectile) return;
		SpawnController.Instance.EnchantEquipment(byWhat, depth);
		base.Break (byWhat);
//		GameObject.FindObjectOfType<TerrainController>().statuesDestroyed++;		// FIXME: hack
		
		CameraController.Instance.NoteText("enchanted " + byWhat.Description);
	}

}
