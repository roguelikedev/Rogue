using UnityEngine;
using System.Collections;

public class GhostController : EnemyController {
	public override void TakeDamage (float quantity, int type)
	{
		switch(type) {
			case WeaponController.DMG_HEAL:
				base.TakeDamage(quantity / GLOBAL_DMG_SCALING, WeaponController.DMG_DEATH);
				return;
			case WeaponController.DMG_RAISE:
				base.TakeDamage(MaxHitPoints / GLOBAL_DMG_SCALING, WeaponController.DMG_DEATH);
				return;
			case WeaponController.DMG_PARA:
				base.TakeDamage(quantity, type);
				break;
			default: break;
		}
	}
	public override void EnterTerrainCallback(string terrType) {
		if (terrType == "water") TakeDamage(1, WeaponController.DMG_HEAL);
		base.EnterTerrainCallback(terrType);
	}
}
