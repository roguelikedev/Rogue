using UnityEngine;
using System.Collections;

public class FoodController : ItemController {
	public AudioClip eating;
	public void EatMe (Acter who) {
		OnPickup(who);
		AudioSource.PlayClipAtPoint(eating, transform.position, CameraController.Instance.Volume);
	}
}
