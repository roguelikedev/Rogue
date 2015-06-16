using UnityEngine;
using System.Collections;

public class WandController : WeaponController {
	public int maxCharges;
	
	public void HasChangedRoom () {
		charges = Mathf.Min(charges + 1, maxCharges);
	}
}
