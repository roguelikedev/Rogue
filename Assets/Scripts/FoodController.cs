using UnityEngine;
using System.Collections;

public class FoodController : ItemController {
	public AudioClip eating;
	public float calories;
	
	public void EatMe (Acter who) {
		OnPickup(who);
		CameraController.Instance.PlaySound(eating);
	}
}
