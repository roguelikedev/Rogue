using UnityEngine;
using System.Collections;

public class ShieldGolem : EnemyController {
	public int baseActionDelay;
	int actionDelay;
	public int baseMovementFrames;
	int movementFrames;
	Vector3 targetDirection;
	public AudioClip clang;
	
	protected override Vector3 DirectionToTarget ()
	{
		if (actionDelay > 0) {
			--actionDelay;
			return Vector3.zero;
		}
		if (movementFrames > 0) {
			movementFrames--;
		}
		else  {
			targetDirection = Vector3.zero;
			actionDelay = baseActionDelay;
		}
		
		if (targetDirection == Vector3.zero) {
			targetDirection = base.DirectionToTarget();
			movementFrames = baseMovementFrames;
		}
		return targetDirection;
	}
	
	protected override bool _FixedUpdate ()
	{
		if (State != ST_WALK) targetDirection = Vector3.zero;
		return base._FixedUpdate ();
	}
}
