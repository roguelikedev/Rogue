using UnityEngine;
using System.Collections;

public class PullController : BroomController {
	protected override float PushDirection {
		get {
			return -1;
		}
	}
	protected override Vector3 Center {
		get {
			return transform.position;
		}
	}
}
