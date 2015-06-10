using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class Breakable : MonoBehaviour {
	public AudioClip smash;
	public ItemController smallMeal;
	public ItemController largeMeal;
	public ItemController hugeMeal;
	public bool broken= false;
	
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
		CameraController.Instance.PlaySound(smash);
		if (smallMeal != null || largeMeal != null || hugeMeal != null) {
			var meals = new List<IDepthSelectable>();
			meals.Add(smallMeal);
			meals.Add(largeMeal);
			meals.Add(hugeMeal);
			meals = SpawnController.Instance.ChooseByDepth(meals, TerrainController.Instance.Depth, 1);
			var meal = meals[Random.Range(0, meals.Count)];
			Instantiate(meal as ItemController, transform.position, Quaternion.identity);
			smallMeal = largeMeal = hugeMeal = null;
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
