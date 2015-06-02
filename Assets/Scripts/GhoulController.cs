using UnityEngine;
using System.Collections;

public class GhoulController : EnemyController {
	public float paralyzeDuration;
	
	void Start() {
		OnHitEffects += a => a.Paralyze(paralyzeDuration);
	}
	
	public override void TakeDamage (float quantity, int type)
	{
		switch(type) {
			case WeaponController.DMG_HEAL:
				print (quantity + " healing");
				base.TakeDamage(quantity / GLOBAL_DMG_SCALING, WeaponController.DMG_DEATH);
				return;
			case WeaponController.DMG_RAISE:
				print ("raised from dead");
				base.TakeDamage(MaxHitPoints / GLOBAL_DMG_SCALING, WeaponController.DMG_DEATH);
				return;
			default: break;
		}
		base.TakeDamage(quantity, type);
	}
}
