using UnityEngine;
using System.Collections;

public class CarrotTitanController : EnemyController {
	void Start() {
		armorClass += 25;
	}
	
	protected override bool _FixedUpdate ()
	{
		if (!friendly) print (armorClass);
		return base._FixedUpdate ();
	}
}
