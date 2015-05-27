using UnityEngine;
using System.Collections;

public class HiltlessController : WeaponController {
	bool hasExplained = false;

	protected override void OnHit (Acter victim, Acter attacker) {
		attacker.TakeDamage(1, DMG_DEATH);
		if (!hasExplained && attacker.GetComponent<PlayerController>() != null) {
			PlayerController.Instance.Speak("i cut myself!");
			hasExplained = true;
		}
		base.OnHit(victim, attacker);
	}
}
