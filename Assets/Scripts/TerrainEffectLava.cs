using UnityEngine;
using System.Collections;

public class TerrainEffectLava : TerrainEffectFloor {
//	public ParticleSystem splashes;		FIXME: this would be pretty cool
	public float damagePerSecond;
	public float destroyChancePerSecond;
	
	void OnTriggerStay(Collider other) {
		base._OnTriggerEnter(other);
		var item = other.GetComponent<ItemController>();
//		if (item != null && Random.Range(0, 60 * destroyChancePerSecond) < 1) {
//			var box = item.GetComponent<BoxCollider>();
//			if (box != null) box.gameObject.SetActive(false);
//			else if (!item.IsEquipped) Destroy (item.gameObject);
//		}
		var barrel = other.GetComponent<Breakable>();
		if (barrel != null && Random.Range(0, 60 * destroyChancePerSecond) < 1) {
			barrel.Break(null);
		}
		
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<Acter>();
		acter.EnterTerrainCallback("lava", damagePerSecond / 60, WeaponController.DMG_GRAP);
	}
}