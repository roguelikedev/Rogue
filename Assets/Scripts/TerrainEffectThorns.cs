using UnityEngine;
using System.Collections;

public class TerrainEffectThorns : TerrainEffectFloor {
	public float damage;
	protected override void _OnTriggerEnter(Collider other) {
		base._OnTriggerEnter(other);
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<Acter>();
		acter.EnterTerrainCallback("thorns", damage);
	}
}