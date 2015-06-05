using UnityEngine;
using System.Collections;

public class TerrainEffectTrap : TerrainEffectFloor {
	WeaponController payload;
	public WeaponController brick;
	bool hasExploded = false;
	bool isSafe = true;
	public bool IsExploding { get { return hasExploded && !isSafe; } }
	public Texture burntEarth;
	public int severity;
	public override bool ContainsTrap { get { return !hasExploded; } }
	public float damageStaggering;
	
	void Awake() {
		if (Random.Range(0, 4) == 0) payload = SpellGenerator.Instance().Explosion();
		else if (Random.Range(0, 4) == 0) {
			payload = SpellGenerator.Instance().Pillar(WeaponController.DMG_PARA);
			damageStaggering = 0;	
		}
		else {
			payload = brick;
			burntEarth = null;
		}
	}
	
	IEnumerator MultipleTrigger () {
		var npcs = Acter.LivingActers.FindAll(a => a is EnemyController).ConvertAll(a => a as EnemyController);
		foreach (var npc in npcs) {
			npc.TrapIsExploding(transform.position, true);
		}
		for (float lcv = 0; lcv < severity; ) {
			var e = Instantiate(payload);
			e.gameObject.SetActive(true);
			e.transform.position = transform.position + new Vector3(Random.Range(-2f, 2f), 3, Random.Range(-2f, 2f));
			if (payload == brick) {
				var tmp = e.transform.position;
				tmp.y = 9;
				e.transform.position = tmp;
				e.friendlyFireActive = true;
			}
			e.attackActive = true;
			if (damageStaggering == 0) {
				e.attackPower = severity;
			}
			else e.attackPower = Random.Range(1, severity / damageStaggering);
			lcv += e.attackPower;
			
			yield return new WaitForSeconds(damageStaggering / 10);
		}
		yield return new WaitForSeconds(payload.lifetime / 60f);
		isSafe = true;
		foreach (var npc in npcs) {
			npc.TrapIsExploding(transform.position, false);
		}
	}

	protected override void _OnTriggerEnter(Collider other) {
		base._OnTriggerEnter(other);
		if (hasExploded) return;
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		if (burntEarth != null) {
			var rend = GetComponent<MeshRenderer>();
			if (rend == null) rend = GetComponentInChildren<MeshRenderer>();
			rend.material.SetTexture("_MainTex", burntEarth);
		}
		hasExploded = true;
		isSafe = false;
		danger.gameObject.SetActive(false);
		if (payload.damageType == WeaponController.DMG_PARA) {
			acter.TakeDamage(payload.attackPower, payload.damageType);
		}
		else StartCoroutine(MultipleTrigger());
	}
}