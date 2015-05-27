using UnityEngine;
using System.Collections;

public class TerrainEffectWater : TerrainEffectFloor {
	public ParticleSystem splashes;
	
	protected override void _OnTriggerEnter(Collider other) {
		base._OnTriggerEnter(other);
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		acter.EnterTerrainCallback("water");
	}
	void OnTriggerStay(Collider other) {
		if (other.GetComponentInParent<Acter>() == null) return;
		splashes.Play();
	}
	
	void OnTriggerExit(Collider other) {
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		acter.ExitTerrainCallback("water");
	}
}