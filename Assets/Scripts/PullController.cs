using UnityEngine;
using System.Collections;

public class PullController : BroomController {
	protected override Vector3 Center {
		get {
			return transform.position;
		}
	}
}
