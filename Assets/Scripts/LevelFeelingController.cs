using UnityEngine;
using System.Collections;

public class LevelFeelingController : MonoBehaviour {
	public GenerateTerrainTrigger parent;
	
	void OnTriggerEnter(Collider other) {
		if (other.GetComponentInParent<PlayerController>() == null) return;
		
		parent.Warn();
		//		GameObject.FindObjectOfType<TerrainController>() .GenerateTerrainAtIndexCallback(index, room);
		//		waitingOnDestroy = true;
		//		Destroy(gameObject);
	}
}
