using UnityEngine;
using System.Collections;

public class GhoulController : EnemyController {
	public float paralyzeDuration;
	
	void Start() {
		OnHitEffects += a => a.Paralyze(paralyzeDuration);
	}
}
