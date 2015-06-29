using UnityEngine;
using System.Collections;

public class MummyController : GhoulController {
	public override void AwardExpForMyDeath ()
	{
		base.AwardExpForMyDeath ();
		PlayerController.Instance.InflictCurse(Random.Range(PlayerController.CURSE_HATE, PlayerController.CURSE_LAST_PLUS_ONE));
	}
}
