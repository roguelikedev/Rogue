using UnityEngine;
using System.Collections;

public class PlayerHealthBarController : HealthBarController {
	public SpriteRenderer frontHand;
	public SpriteRenderer backHand;
	public SpriteRenderer frontLock;
	public SpriteRenderer backLock;
	public TextMesh itemCharges;
	public SpriteRenderer wandIcon;
	public SpriteRenderer potionIcon;
	
	public void ShowLock (bool active, bool back) {
		var which = back ? backLock : frontLock;
		which.gameObject.SetActive(active);
	}
}
