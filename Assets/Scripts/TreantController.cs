using UnityEngine;
using System.Collections;

public class TreantController : DemonController {
	public override string MainClass { get { return C_BRUTE; } }

	public float naturalArmor;
	void Start() {
		armorClass += naturalArmor;
	}
	
	public override void TakeDamage (float quantity, int type)
	{
		if (type == WeaponController.DMG_FIRE) {
			if (!damageAura) {
				InitializeAura(WeaponController.DMG_FIRE, auraDamage);
			}
			else damageAura.attackPower += auraDamage;
			damageAura.friendlyFireActive = true;
			damageAura.damageType = WeaponController.DMG_PHYS; 		// don't let it proc itself
			damageAura.lifetime += (int)(quantity * 5);	// remember AC doubles vs fire
		}
		base.TakeDamage (quantity, type);
	}
	
	protected override bool _FixedUpdate ()
	{
		if (damageAura && damageAura.attackVictims.Count == 0) {
			damageAura.attackPower -= auraDamage;
			if (damageAura.attackPower <= 0) damageAura.lifetime = 1;
		}
		return base._FixedUpdate ();
	}
}
