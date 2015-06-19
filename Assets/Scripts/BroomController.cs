﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class BroomController : WeaponController {
	protected virtual float PushDirection { get { return 1; } }
	protected virtual Vector3 Center {
		get {
			if (Parent != null) return Parent.transform.position;
			else return transform.position;
		}
	}
	
	protected override void _FixedUpdate ()
	{
		if (attackActive) {
			foreach (var victim in attackVictims) {
				if (victim == null) continue;
				if (victim.State == Acter.ST_CAKE) continue;
				
				Vec direction;
				if (Parent != null) {
					if (victim.friendly == Parent.friendly && !friendlyFireActive) continue;
				 	direction = (victim.transform.position - Center);
				}
				else direction = victim.transform.position - Center;		// it's a trap
				direction.y = 0;
				direction *= PushDirection;
				
//				direction.z = Mathf.Abs(direction.z);
				if (direction.Len3 > 9) {
					continue;
				}
				direction = direction.Normalish;
				direction /= Mathf.Sqrt(victim.Poise);
				victim.transform.position = victim.transform.position + direction;
			}
		}
		
		
		base._FixedUpdate ();
	}
//	protected override bool _OnTriggerStay (Collider other)
//	{
////		print ("stay");
//		if (base._OnTriggerStay (other)) {
//			if (attackActive) {
//				foreach (var victim in attackVictims) {
//					if (victim == null) continue;
//					
//					Vec direction;
//					if (Parent != null) {
//						direction = (victim.transform.position - Parent.transform.position);
//						if (victim.friendly == Parent.friendly) continue;
//					}
//					else direction = victim.transform.position - transform.position;		// it's a trap
//					direction.z = 0;
//					direction.y = Mathf.Abs(direction.y);
//					direction = direction.Normalish;
//					if (direction.Len3 < 9) {
//						direction /= Mathf.Max(3f, Mathf.Sqrt(victim.Poise));
//						victim.transform.position = victim.transform.position + direction;
//					}
//				}
//				
//			}
//			return true;
////			else attackVictims.Clear();
//		}
//		return false;
//	}
}
