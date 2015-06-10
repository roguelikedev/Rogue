using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ItemController : MonoBehaviour, IDepthSelectable {
	public int depth;
	public float commonness = 1;
//	public virtual int Depth { get { return depth == -1 ? 27 : depth; } }		FIXME: should i do it like this?
	public virtual int Depth { get { return depth; } }
	/// <summary> only chops (clone), no downcase or etc  </summary>
	public string Name { get { return name.Replace("(Clone)", ""); } }
	
	
	public float Commonness { get { return commonness; } }
	void Start() {
		GetComponentInChildren<SpriteRenderer>().sortingOrder -= (int)(transform.position.z * 10);
//		print (GetComponentInChildren<SpriteRenderer>().sortingOrder);
	}
	public System.Action<Acter> OnPickup = a => {} ;

	public bool IsEquipped {
		get {
			return GetComponentInParent<Acter>() != null;
		}
	}
	
	bool hasSprite;
	bool hasCheckedSprite;
	protected virtual void _FixedUpdate() {
		if (transform.position.y < -2) {
			Destroy(gameObject);
		}
		if (IsEquipped && (hasSprite || !hasCheckedSprite)) {
			hasCheckedSprite = true;
			var spr = transform.parent.GetComponentInParent<SpriteRenderer>();
			if (spr != null) {
				var mySpr = GetComponentInChildren<SpriteRenderer>();
				if (mySpr != null) {
					hasSprite = true;
					mySpr.sortingOrder = spr.sortingOrder;
					var wc = GetComponent<WeaponController>();
					if (wc != null) {
						if (wc.IsOffhand && !wc.name.Contains("Shield")) mySpr.sortingOrder--;
						else mySpr.sortingOrder++;
//						if (GetComponent<WeaponController>().IsArmor) mySpr.sortingOrder++;
//						else mySpr.sortingOrder--;
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
