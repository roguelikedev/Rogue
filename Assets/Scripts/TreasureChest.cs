using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreasureChest : ItemController {
	public List<ItemController> contents = new List<ItemController>();
	public Sprite openedSprite;
	bool isOpen = false;

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
		StartCoroutine(Decay());
	}
	
	IEnumerator Decay () {
		yield return new WaitForSeconds(1);
		GetComponentInChildren<BoxCollider>().gameObject.SetActive(false);
	}
}
