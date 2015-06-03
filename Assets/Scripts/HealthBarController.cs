using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HealthBarController : MonoBehaviour {
	public Acter player;
	protected Vector3 offset;
	public SpriteRenderer heart;
	List<SpriteRenderer> buddies = new List<SpriteRenderer>();
	
	
	// Use this for initialization
	void Awake () {
		offset = transform.position;
	}
	
	static int destCount = 0;
	void OnDestroy () {
		DestroyMyBuddies();
		++destCount;
	}
	bool mutex = false;
	void DestroyMyBuddies () {
		if (mutex) return;
		mutex = true;
		buddies.RemoveAll(b => b == null);
		while (buddies.Count > 0) {
			var b = buddies[0];
			buddies.Remove(b);
			Destroy(b.gameObject);
		}
		mutex = false;
	}
	
	float prevHP = -1;
	public void SetCurrentHP (float hp, float hpPerColor) {
		transform.position = player.transform.position + offset;
		
		if (hp != prevHP) {
			prevHP = hp;
			DestroyMyBuddies();
			buddies.Clear();
			
			bool first = true;
			for (float lcv = hp; lcv > 0; --lcv) {
				var spr = Instantiate(heart);
				buddies.Add(spr);
				if (first) {
					var remainder = hp - (int)hp;
					if (remainder > 0) {
						spr.transform.localScale = heart.transform.localScale * remainder;
					}
					first = false;
				}
				if (lcv <= hpPerColor) spr.color = Color.red;
				else if (lcv <= hpPerColor * 2) spr.color = Color.blue;
				else spr.color = Color.gray;
			}
			buddies.Reverse();
		}
		
		// follow the acter here instead of in a LateUpdate() to avoid dangling pointers -- they SetCurrentHP every frame anyway
		for (int lcv = 0; lcv < buddies.Count; ++lcv) {
			var xOffset = (lcv % (int)hpPerColor) * heart.bounds.extents.x * 2;// + lcv * 0.05f;
//			print (xOffset + " before scaling");
//			xOffset *= sprite.bounds.extents.x - .24f);
//			print (xOffset + " post scaling");
			buddies[lcv].transform.position = transform.position + new Vector3(xOffset, 0);//lcv * 0.5f, 0);
			buddies[lcv].sortingOrder += lcv;
		}
	}
}
