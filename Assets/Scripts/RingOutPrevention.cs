using UnityEngine;
using System.Collections;

public class RingOutPrevention : MonoBehaviour {
	public Vector3 shoveDirection;

	void OnTriggerStay (Collider collider) {
		var act = collider.GetComponent<Acter>();
		if (act) {
			act.transform.position = act.transform.position + shoveDirection;
		}
	}
}
