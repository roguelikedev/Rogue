using UnityEngine;
using System.Collections;

public class WandController : WeaponController {
	public int maxCharges;
	
	public override string Description {
		get {
			var baseD = base.Description;
			return baseD.Replace(" charges", "/" + maxCharges + " charges");
		}
	}
	
	public void HasChangedRoom () {
		charges = Mathf.Min(charges + 1, maxCharges);
	}
}
