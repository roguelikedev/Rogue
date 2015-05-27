using UnityEngine;
using System.Collections;

public class Breakable : MonoBehaviour {
	public AudioClip smash;
	public ItemController smallMeal;
	public ItemController largeMeal;
	
	void Start() {
		GetComponentInChildren<SpriteRenderer>().sortingOrder -= (int)(transform.position.z * 10);
	}

	void FixedUpdate() {
		if (transform.position.y < -2) {
			Destroy(gameObject);
		}
	}

	public virtual void Break(WeaponController byWhat) {
		AudioSource.PlayClipAtPoint(smash, transform.position, CameraController.Instance.Volume);
		if (smallMeal != null || largeMeal != null) {
			Instantiate(Random.Range(0,4) == 0? largeMeal : smallMeal, transform.position, Quaternion.identity);
		}
		else { 	// statue
			GameObject.FindObjectOfType<TerrainController>().statuesDestroyed++;
		}
		Destroy(gameObject);
	}
}
