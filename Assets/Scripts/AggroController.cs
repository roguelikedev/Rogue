using UnityEngine;
using System.Collections;


public class AggroController : MonoBehaviour {
	int dir; float rad, h; Vector3 ctr;
	protected EnemyController Parent { get { return GetComponentInParent<EnemyController>(); } }
	
	void Awake() {
		dir = GetComponent<CapsuleCollider>().direction;
		rad = GetComponent<CapsuleCollider>().radius;
		h = GetComponent<CapsuleCollider>().height;
		ctr = GetComponent<CapsuleCollider>().center;
//		originalSize = Instantiate(GetComponent<CapsuleCollider>());		for unknown reasons, this causes stack overflow
	}
	
	public void Reinitialize() {
		GetComponent<CapsuleCollider>().direction = dir;
		GetComponent<CapsuleCollider>().radius = rad;
		GetComponent<CapsuleCollider>().height = h;
		GetComponent<CapsuleCollider>().center = ctr;
	}

	protected virtual void WillThreaten(Acter other) {
		Parent.Threatens(other);
	}
	
	void OnTriggerStay(Collider other) {
		if (other.name != "torso") return;
		var enemy = other.GetComponentInParent<Acter>();
		if (enemy == null) return;
		if (enemy.friendly == Parent.friendly) return;
		if (enemy.State == Acter.ST_DEAD) return;
		
		WillThreaten(enemy);
	}
}

