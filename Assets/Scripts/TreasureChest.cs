using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class TreasureChest : ItemController {
	public List<ItemController> contents = new List<ItemController>();
	public Sprite openedSprite;
	bool isOpen = false;
	int kickedXDirection;
	
	protected override void _FixedUpdate ()
	{
		if (isOpen) {
			transform.Rotate(0, 0, 3 * kickedXDirection);
		}
		base._FixedUpdate ();
	}
	
	protected override void OnTriggerEnter(Collider other) {
		if (isOpen) return;
		if (other.name != "torso") return;
		var acter = other.GetComponentInParent<PlayerController>();
		if (acter == null) return;
		
		contents.ForEach(c => {
			c.gameObject.SetActive(true);
			c.transform.position = transform.position + new Vector3(0, 3);
		});
		
		GetComponent<SpriteRenderer>().sprite = openedSprite;
		isOpen = true;
		
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
		Vec delta = (transform.position - other.transform.position).normalized;
		delta.y = Mathf.Abs(delta.y);
		delta *= Vec.New(20, 1, 25);
		GetComponent<Rigidbody>().velocity = delta;
		
		kickedXDirection = delta.x > 0 ? 1 : -1;
			
		StartCoroutine(Decay());
	}
	
	IEnumerator Decay () {
		yield return new WaitForSeconds(1);
		GetComponentInChildren<BoxCollider>().gameObject.SetActive(false);
	}
}
