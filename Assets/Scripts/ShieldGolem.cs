using UnityEngine;
using System.Collections;

public class ShieldGolem : EnemyController {
	public int baseActionDelay;
	int actionDelay;
	public int baseMovementFrames;
	int movementFrames;
	Vector3 targetDirection;
	public AudioClip clang;
	bool isDoingNothing = false;
	System.Action DoNothing = null;
	
	void Start () {
		var self = this;
		DoNothing = () =>
		{
			self.shouldUseMainHand = self.shouldUseOffhand = false; 
			print(self.shouldUseMainHand + " " + self.shouldUseOffhand);
		};
	}
	
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
			stopRunningSlowly = 0;
			targetDirection = base.DirectionToTarget();
			movementFrames = baseMovementFrames;
		}
		return targetDirection;
	}
	
	public override void AttackActiveFramesDidFinish ()
	{
		OnFixedUpdate += DoNothing;
		isDoingNothing = true;
		actionDelay = baseActionDelay;
		base.AttackActiveFramesDidFinish ();
	}
	
	protected override bool _FixedUpdate ()
	{
		if (actionDelay <= 0 && isDoingNothing && DoNothing != null) {
			isDoingNothing = false;
			OnFixedUpdate -= DoNothing;
		}
		if (State != ST_WALK) targetDirection = Vector3.zero;
		return base._FixedUpdate ();
	}
}
