using UnityEngine;
using System.Collections;


public class DamageAnnouncer : MonoBehaviour {
	public Acter acter;
	TextMesh damageText;
	TextMesh announcementText;
	SpriteRenderer statusIconRenderer;
	SpriteRenderer friendlyIconRenderer;
	const int baseDelayBeforeFadeOut = 30;
	int delayBeforeFadeOut = baseDelayBeforeFadeOut;
	Vector3 offset = new Vector3();
	public AudioClip clank;
	public AudioClip splatter;
	public AudioClip playerDeath;
	public AudioClip thump;
	public Sprite friendlyIcon;
	public Sprite eliteIcon;
	public Sprite paralyzedIcon;
	float damageCount = 0;
	public AudioClip chimeSound;
	
	public void AnnounceDeath() {
		var mob = acter as EnemyController;
		if (mob != null) AnnounceText("+"+mob.ChallengeRating+" XP");
		var pc = acter as PlayerController;
		if (pc != null) {
			pc.announcer.AnnounceText("YOU DIED...\npress escape to restart");
			CameraController.Instance.PlaySound(playerDeath);
		}
		CameraController.Instance.PlaySound(splatter);
	}
	
	public void AnnounceDamage(float quantity, int damageType) {
		damageCount += quantity;
		quantity = damageCount;
		var str = quantity.ToString();
		str = str.TrimStart(new char['0']);
		if (str.Length > 3 && str.Contains(".")) str = str.Remove(3);
		damageText.text = str;
		delayBeforeFadeOut = baseDelayBeforeFadeOut;
		damageText.transform.localPosition = Vector3.zero;
		var clip = thump;
		if (damageType == WeaponController.DMG_NOT) {		// thorns floor
			clip = SpellGenerator.Instance().rippingSound;
		}
		else if (damageType == WeaponController.DMG_FIRE) {
			clip = SpellGenerator.Instance().fireSound;
		}
		if (damageType != WeaponController.DMG_GRAP) {
			CameraController.Instance.PlaySound(clip);
		}
	}
	
	public void AnnounceText(string text) {
		announcementText.text = text;
		delayBeforeFadeOut = baseDelayBeforeFadeOut * 2;
		announcementText.transform.localPosition = Vector3.zero;
	}
	
	public void SetFriendly(bool friendly) {
		if (friendlyIconRenderer.sprite == eliteIcon) return;
		if (friendly) friendlyIconRenderer.sprite = friendlyIcon;
		else friendlyIconRenderer.sprite = null;
	}
	
	public void SetElite() {
		friendlyIconRenderer.sprite = eliteIcon;
	}
	
	public void SetParalyzed(bool paralyzed) {
		if (paralyzed) statusIconRenderer.sprite = paralyzedIcon;
		else statusIconRenderer.sprite = null;
	}

	void Awake () {
		damageText = GetComponentsInChildren<TextMesh>()[0];
		announcementText = GetComponentsInChildren<TextMesh>()[1];
		if (damageText == null) {
			return;
		}
		GetComponentInChildren<MeshRenderer>().sortingLayerName = "UI";
		damageText.fontSize = 0;
		damageText.characterSize = 0.5f;
		damageText.text = "";
		statusIconRenderer = GetComponentsInChildren<SpriteRenderer>()[0];
		statusIconRenderer.sprite = null;
		statusIconRenderer.sortingLayerName = "UI";
		friendlyIconRenderer = GetComponentsInChildren<SpriteRenderer>()[1];
		friendlyIconRenderer.sprite = null;
		friendlyIconRenderer.sortingLayerName = "UI";
	}
	
	void LateUpdate () {
		if (acter == null) return;
		transform.position = acter.transform.position + offset;
		transform.rotation = Quaternion.identity;
		
		delayBeforeFadeOut--;
		damageText.transform.position += new Vector3(0, 0.025f);
		damageText.transform.rotation = Quaternion.identity;
		announcementText.transform.position += new Vector3(0, 0.025f);
		announcementText.transform.rotation = Quaternion.identity;
		
		if (delayBeforeFadeOut <= 0) {
			damageText.text = "";
			announcementText.text = "";
			damageCount = 0;
		}
	}
}

