using UnityEngine;
using System.Collections;

public class WomanController : EnemyController {

	void Start () {
		armorClass += 2;
	}

	public override string MainClass {
		get {
			return C_GESTALT;
		}
	}
}
