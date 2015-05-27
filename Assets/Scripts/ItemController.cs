using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ItemController : MonoBehaviour, IDepthSelectable {
	public int depth;
	public virtual int Depth { get { return depth; } }
	public float Commonness { get { return 1; } }
	void Start() {
		GetComponentInChildren<SpriteRenderer>().sortingOrder -= (int)(transform.position.z * 10);
	}
	public System.Action<Acter> OnPickup = a => {} ;

	public bool IsEquipped {
		get {
			return GetComponentInParent<Acter>() != null;
		}
	}
	
	protected virtual void _FixedUpdate() {
		if (transform.position.y < -2) {
			Destroy(gameObject);
		}
		if (IsEquipped) {
			var spr = transform.parent.GetComponentInParent<SpriteRenderer>();
			if (spr != null) {
				var mySpr = GetComponentInChildren<SpriteRenderer>();
				if (mySpr != null) {
					mySpr.sortingOrder = spr.sortingOrder;
					if (GetComponent<WeaponController>() != null) {
						if (GetComponent<WeaponController>().IsArmor) mySpr.sortingOrder++;
						else mySpr.sortingOrder--;
					}
					if (GetComponent<TrinketController>() != null) mySpr.sortingOrder++;
				}
			}
		}
	}
	
	void FixedUpdate() {
		_FixedUpdate();
	}
	
	// item pickup
	protected virtual void OnTriggerEnter(Collider other) {
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		if (GetComponent<BoxCollider>() == null) return;
		if (name.Contains("Bare Hands")) return;
		if (!IsEquipped) acter.PickupIsEligible(this);
	}
	protected virtual void OnTriggerExit(Collider other) {
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<Acter>();
		if (acter == null) return;
		if (GetComponent<BoxCollider>() == null) return;
		if (name.Contains("Bare Hands")) return;
		acter.PickupIsIneligible(this);
	}
}
