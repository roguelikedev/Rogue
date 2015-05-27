using UnityEngine;
using System.Collections;

public class GrapplerController : AggroController {
	Acter grapplee;

	protected override void WillThreaten (Acter other)
	{
		if (Parent.State == Acter.ST_DEAD) {
			other.grappledBy.Remove(Parent);
			Parent.grappling = null;
			return;
		}
		base.WillThreaten (other);
		if (grapplee != null) return;
		other.grappledBy.Add(Parent);
		Parent.grappling = other;
		grapplee = other;
	}
}
