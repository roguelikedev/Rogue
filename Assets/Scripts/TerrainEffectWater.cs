using UnityEngine;
using System.Collections;

public class TerrainEffectWater : TerrainEffectFloor {
	public ParticleSystem splashes;
	
	protected override void _OnTriggerEnter(Collider other) {
		base._OnTriggerEnter(other);
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		acter.EnterTerrainCallback("water");
		splashes.Play();		// used to be in defunct triggerstay()
	}
//	void OnTriggerStay(Collider other) {
//		if (other.GetComponentInParent<Acter>() == null) return;
//	}
	
	void OnTriggerExit(Collider other) {
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		acter.ExitTerrainCallback("water");
	}
}