﻿using UnityEngine;
using System.Collections;

public class DemonController : EnemyController {
	protected WeaponController damageAura;
	public int auraRefreshRate;
	protected int auraRefreshCountdown;
	public float auraDamage;
	public override string MainClass {
		get {
			return C_GESTALT;
		}
	}
	
	protected void InitializeAura(int damageType, float power) {
		damageAura = SpellGenerator.Instance().Pillar(damageType);
		damageAura.lifetime = -1;
		damageAura.attackPower += power;
		damageAura.transform.parent = transform;
		damageAura.transform.localPosition = Vector3.zero;
		var ctr = damageAura.GetComponent<CapsuleCollider>().center;
		ctr.x = 0;
		damageAura.GetComponent<CapsuleCollider>().center = ctr;
		damageAura.gameObject.SetActive(true);
		damageAura.thrownBy = this;
	}
	
	void Start() {
		InitializeAura(WeaponController.DMG_FIRE, auraDamage);
	}
	
	protected override bool _FixedUpdate ()
	{
		if (damageAura) {
			damageAura.attackActive = true;
			damageAura.transform.position = transform.position;
			if (damageAura.GetComponentInChildren<ParticleSystem>() != null) {
				damageAura.GetComponentInChildren<ParticleSystem>().transform.localPosition = Vector3.zero;
			}
			
			if (auraRefreshCountdown-- <= 0) {
				damageAura.attackVictims.Clear();
				auraRefreshCountdown = auraRefreshRate;
			}
		}
		return base._FixedUpdate ();
	}
}
