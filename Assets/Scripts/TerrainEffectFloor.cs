using UnityEngine;
using System.Collections;

public class TerrainEffectFloor : MonoBehaviour {
	public SpriteRenderer danger;
	public virtual bool ContainsTrap { get { return false; } }
	
	public void ShowDanger () {
		if (GetComponentInChildren<SpriteRenderer>()) return;
		danger = Instantiate(danger);
		danger.transform.position += transform.position;
		danger.transform.parent = transform;
		danger.gameObject.SetActive(true);
	}

	protected virtual void _OnTriggerEnter(Collider other) {
		if (other.tag != "Throwable Weapon") return;
		var item = other.GetComponentInParent<WeaponController>();
		if (item == null) {
			Debug.LogError("mistagged (shouldn't be Throwable Weapon) " + item);
			return;
		}
		if (item.attackActive) { item.attackActive = false; }
	}

	void OnTriggerEnter(Collider other) {
		_OnTriggerEnter(other);
	}
}
