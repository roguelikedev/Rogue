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
		GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Character";
//		print (GetComponentInChildren<SpriteRenderer>().sortingOrder);
	}
	public System.Action<Acter> OnPickup = a => {} ;

	public bool IsEquipped {
		get {
			return GetComponentsInParent<Acter>().Length != 0;
		}
	}
	
	public void UpdateSortingOrder () {
//		if (IsEquipped) {
//			Debug.LogError(Name + " shouldn't be updating sort order!");
//		}
		if (hasSprite || !hasCheckedSprite) {
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
						else mySpr.sortingOrder += 2;
						//						if (GetComponent<WeaponController>().IsArmor) mySpr.sortingOrder++;
						//						else mySpr.sortingOrder--;
					}
					if (GetComponent<TrinketController>() != null) {
//						print (transform.parent + " order " + spr.sortingOrder);
						mySpr.sortingOrder++;
//						print ("my order " + mySpr.sortingOrder);
					}
				}
			}
		}
	}
	
	bool hasSprite;
	bool hasCheckedSprite;
	protected virtual void _FixedUpdate() {
		if (transform.position.y < -2) {
			Destroy(gameObject);
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
