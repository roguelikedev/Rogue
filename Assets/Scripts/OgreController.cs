using UnityEngine;
using System.Collections;

public class OgreController : EnemyController {
	protected override bool _FixedUpdate ()
	{
		poiseBreakCounter = 0;
		return base._FixedUpdate ();
	}
}
