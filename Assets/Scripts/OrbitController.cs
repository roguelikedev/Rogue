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
		
//		transform.localPosition = Vec.Zero;
//		transform.Rotate(xWorld, yWorld, zWorld, Space.World);// *= Vec.New(1, 1, 1f, 1).ToQ;
//		transform.Rotate(xWorld, yWorld, zWorld, Space.World);// *= Vec.New(1, 1, 1f, 1).ToQ;
//		transform.Rotate(xWorld, yWorld, zWorld, Space.World);// *= Vec.New(1, 1, 1f, 1).ToQ;
//		transform.Rotate(xWorld, yWorld, zWorld, Space.Self);// *= Vec.New(1, 1, 1f, 1).ToQ;
//		print ("wpn " + GetComponentInChildren<WeaponController>());
//		print (" spr " + GetComponentInChildren<WeaponController>().GetComponent<SpriteRenderer>());
//		print ("head " + PlayerController.Instance.head);
//		print ("head spr " + PlayerController.Instance.head.GetComponent<SpriteRenderer>());
		GetComponentInChildren<WeaponController>().GetComponent<SpriteRenderer>().sortingOrder =
					PlayerController.Instance.head.GetComponent<SpriteRenderer>().sortingOrder;
//		transform.RotateAround(PlayerController.Instance.transform.position, Vec.New(xSelf, ySelf, zSelf), 1);// *= Vec.New(1, 1, 1f, 1).ToQ;
	}
}
