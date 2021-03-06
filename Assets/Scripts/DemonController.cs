﻿using UnityEngine;
using System.Collections;

public class DemonController : EnemyController {
	protected WeaponController damageAura;
	public int auraRefreshRate;
	protected int auraRefreshCountdown;
	public float auraDamage;
	
	public static WeaponController CreateAura(int damageType, float power) {
		var rval = SpellGenerator.Instance.Pillar(damageType);
		rval.lifetime = -1;
		rval.attackPower *= power;
		rval.transform.localPosition = Vector3.zero;
		var ctr = rval.GetComponent<CapsuleCollider>().center;
		ctr.x = 0;
		rval.GetComponent<CapsuleCollider>().center = ctr;
		rval.gameObject.SetActive(true);
		rval.attackActive = true;
		return rval;
	}
	
	protected void InitializeAura(int damageType, float power) {
		damageAura = SpellGenerator.Instance.Pillar(damageType);
		damageAura.lifetime = -1;
		damageAura.attackPower += power;
		damageAura.transform.parent = transform;
		damageAura.transform.localPosition = Vector3.zero;
		var ctr = damageAura.GetComponent<CapsuleCollider>().center;
		ctr.x = 0;
		damageAura.GetComponent<CapsuleCollider>().center = ctr;
		damageAura.gameObject.SetActive(true);
		damageAura.attackActive = true;
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
