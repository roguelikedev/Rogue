using UnityEngine;
using System.Collections;
using Utilities.Geometry;

public class OrbitController : MonoBehaviour {
	public float xAcceleration, yRotation, zAcceleration;
	public float maxVelocity;
	public WeaponController weapon;
	bool goDown;
	bool goRight;
	float zVelocity = 0;
	float xVelocity = -1f;
	Vec offset;
	
	void Update () {
		if (PlayerController.Instance == null) return;
		if (goDown) {
			zVelocity -= zAcceleration;
			if (zVelocity <= -maxVelocity) {
				goDown = false;
				weapon.attackVictims.Clear();
			}
		}
		else {
			zVelocity += zAcceleration;
			if (zVelocity >= maxVelocity) {
				goDown = true;
				weapon.attackVictims.Clear();
			}
		}
		if (goRight) {
			xVelocity -= xAcceleration;
			if (xVelocity <= -maxVelocity) {
				goRight = false;
				weapon.attackVictims.Clear();
			}
		}
		else {
			xVelocity += xAcceleration;
			if (xVelocity >= maxVelocity) {
				goRight = true;
				weapon.attackVictims.Clear();
			}
		}
		
		offset += Vec.New(xVelocity, 0, zVelocity);
		transform.position = offset + PlayerController.Instance.transform.localPosition - Vec.New(0,2,6);
		
		GetComponentInChildren<WeaponController>().GetComponent<SpriteRenderer>().sortingOrder =
					PlayerController.Instance.head.GetComponent<SpriteRenderer>().sortingOrder;
	}
}
