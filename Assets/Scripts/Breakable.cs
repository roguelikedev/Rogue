using UnityEngine;
using System.Collections;
using Utilities.Geometry;

public class Breakable : MonoBehaviour {
	public AudioClip smash;
	public ItemController smallMeal;
	public ItemController largeMeal;
	public bool broken;
	
	void Start() {
		GetComponentInChildren<SpriteRenderer>().sortingOrder -= (int)(transform.position.z * 10);
	}

	void FixedUpdate() {
		if (transform.position.y < -2) {
			Destroy(gameObject);
		}
	}
	
	IEnumerator Decay () {
		yield return new WaitForSeconds(1);
		GetComponent<CapsuleCollider>().gameObject.SetActive(false);
	}

	public virtual bool Break(WeaponController byWhat) {
		if (broken) return false;
		broken = true;	
		AudioSource.PlayClipAtPoint(smash, transform.position, CameraController.Instance.Volume);
		if (smallMeal != null || largeMeal != null) {
			Instantiate(Random.Range(0,4) == 0? largeMeal : smallMeal, transform.position, Quaternion.identity);
			smallMeal = largeMeal = null;
		}
		else { 	// statue
			if (PlayerController.Instance.IsJason) {
				CameraController.Instance.AnnounceText("NO MERCY,\nMURDERER");
				TerrainController.Instance.statuesDestroyed = -27;
			}
			else {
				GameObject.FindObjectOfType<TerrainController>().statuesDestroyed++;
				CameraController.Instance.AnnounceText("things are\neasier");
			}
		}
		if (byWhat != null) {
			GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
			Vec delta = (transform.position - byWhat.transform.position).normalized;
			delta.y = Mathf.Abs(delta.y);
			delta *= Vec.New(20, 1, 25);
			GetComponent<Rigidbody>().velocity = delta; //AddForce(delta);
		}
		StartCoroutine(Decay());
		return true;
	}
}
