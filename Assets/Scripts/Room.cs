using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour {
	public List<GameObject> tiles = new List<GameObject>();
	public int xIndex;
	public int terrainType;
	public int nextRoomType;
}
