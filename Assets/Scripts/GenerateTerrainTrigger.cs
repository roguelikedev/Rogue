using UnityEngine;
using System.Collections;

public class GenerateTerrainTrigger : MonoBehaviour {
	public TerrainController terrainController;
	public int index;
	public object room;
//	bool waitingOnDestroy = false;
	

	void OnTriggerEnter(Collider other) {
//		if (other.GetComponentInParent<PlayerController>() == null || waitingOnDestroy) return;
		if (other.GetComponentInParent<PlayerController>() == null) return;
//		GameObject.FindObjectOfType<TerrainController>() .GenerateTerrainAtIndexCallback(index, room);
		terrainController.GenerateTerrainAtIndexCallback(index, room);
//		waitingOnDestroy = true;
//		Destroy(gameObject);
	}
}
