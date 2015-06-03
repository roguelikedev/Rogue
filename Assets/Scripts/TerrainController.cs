using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utilities.Geometry;

public class TerrainController : MonoBehaviour {
	#region variables, properties
	public Rigidbody tilePrefab;
	public Rigidbody tileRampLeft;
	public Rigidbody tileRampRight;
	public Rigidbody tileWater;
	public Rigidbody tileFarWall;
	public Rigidbody tileNearWall;
	public Rigidbody tileRoof;
	public Rigidbody tileThorns;
	public Rigidbody tileTrapped;
	public TerrainEffectFloor trogAltar;
	public BoxCollider terrainTransition;
	public CameraController announcer;
	public SpawnController spawnController;
	SpellGenerator SpellGenerator { get { return GetComponentInParent<SpellGenerator>(); } }
	public Texture stone;
	public Texture skulls;
	public Material forest;
	public Material cathedral;
	public int trapRarity;
	bool showTraps;
	public EnchanterStatue enchanterStatue;
	public static TerrainController Instance { get { return GameObject.FindObjectOfType<TerrainController>(); } }
	public bool ShowTraps {
		get {
			return showTraps;
		}
		set {
			showTraps = value;
			if (showTraps) {
				foreach(var r in rooms) {
					foreach (var t in r.tiles) {
						var ft = t.GetComponent<TerrainEffectFloor>();
						if (ft == null) continue;
						if (ft.ContainsTrap) ft.ShowDanger();
					}
				}
			}
		}
	}
	bool hasSpawnedEmpty = false;
	public int Depth
	{
		get {
			if (!hasSpawnedEmpty) return 0;
			if (statuesDestroyed <= -2) return 27;
			var rval = generatedCount / (2f + statuesDestroyed);
//			rval = Mathf.Pow(rval, 1.3f);
			return (int)Mathf.Max(1, Mathf.Min(27, rval));
		}
	}
	public int statuesDestroyed = 0;
	int generatedCount = 0;
	int previousAreaType = 0;
	public bool fuckTheWaterLevel;
	List<Room> rooms = new List<Room>();
	#endregion
	#region constants
	const int TILE_SZ = 5;
	const int TILES_PER_ROOM = 5;
	const int MAX_ROOMS = 3;
	public const int D_FOREST = 0, D_WATER = 1, D_CAVE = 2, D_THORNS = 3, D_TOMB = 4, D_CHRISTMAS = 5, D_ARMORY = 6
					, D_MERCY = 7, D_TROG = 8, D_ENCHANT = 9, D_TROVE = 10;
	List<int> visitedSpecialRooms = new List<int>();
	const int D_FINAL = 9;		// this is +1'd before use
	#endregion	
	#region Room	
	class Room {
		public List<GameObject> tiles = new List<GameObject>();
		public int xIndex;
		public int terrainType;
		public int nextRoomType;
	}
	Room LeftmostRoom {
		get {
			Room rval = rooms[0];
			rooms.ForEach(r => { if(r.xIndex < rval.xIndex) rval = r; });
			return rval;
		}
	}
	Room RightmostRoom {
		get {
			Room rval = rooms[0];
			rooms.ForEach(r => { if(r.xIndex > rval.xIndex) rval = r; });
			return rval;
		}
	}
	#endregion
	#region life cycle
	void Start ()
	{
		rooms.Add(GenerateAtIndex(0, tilePrefab, stone, D_CAVE));
		announcer.AnnounceText("Entered Wimps Cave");
	}
	
	int nightgauntCooldown = 0;
	void FixedUpdate () {
		if (Depth < 25) return;
		if (Random.Range(0, nightgauntCooldown) == 0) {
			nightgauntCooldown += 100 * Acter.livingActers.FindAll(e => !e.friendly).Count / (Depth - 24);
			var loc = Random.Range(0, 2) == 0 ? LeftmostRoom.tiles[0].transform.position.x - TILE_SZ * 6
					: RightmostRoom.tiles[RightmostRoom.tiles.Count - 1].transform.position.x + TILE_SZ * 6;
			var ng = Instantiate(spawnController.enemyNightgaunt);
			ng.transform.position = new Vector3(loc, 0) + ng.transform.position;
		}
		if (nightgauntCooldown > 1) nightgauntCooldown--;
	}
	
	void CleanUp(int currentIndex) {
		if (rooms.Count <= MAX_ROOMS) return;
		
		System.Action<List<GameObject>, System.Func<GameObject, bool>> Spotless = (l, pred) => {
			l.RemoveAll(i => i == null);
			for (int lcv = l.Count - 1; lcv >= 0; --lcv) {
				var curr = l[lcv];
				if (pred(curr)) {
					Destroy(curr);
					l.Remove(curr);
				}
			}
		};
		
		int aryIndex = 0;
		int farthest = Mathf.Abs(currentIndex - rooms[0].xIndex);
		for (int lcv = 0; lcv < rooms.Count; ++lcv) {
			var diff = Mathf.Abs(currentIndex - rooms[lcv].xIndex);
			if (diff > farthest) {
				farthest = diff;
				aryIndex = lcv;
			}
		}
		
		var room = rooms[aryIndex];
		
		Spotless(room.tiles, i => true);
//		Debug.Log("destroying " + room.xIndex);
		rooms.Remove(room);
	}
	#endregion
	
	public void GenerateTerrainAtIndexCallback(int x, object _room) {
		if (rooms.Exists(r => r.xIndex == x)) {
//			Debug.Log(x + " exists, ignoring");
			return;
		}
		
		var tile = tilePrefab;
		Texture floorTexture = null;
		var areaName = "Wimps Cave";
		var room = _room as Room;
		previousAreaType = room.terrainType;
		int currentAreaType = room.nextRoomType;
		
		switch(currentAreaType) {
			case D_FOREST:
				areaName = "Underground Forest";
				break;
			case D_WATER:
				if (!fuckTheWaterLevel) {
					tile = tileWater;
				}
				areaName = "Water Level";
				break;
			case D_CAVE:
				floorTexture = stone;
				areaName = "Wimps Cave";
				break;
			case D_THORNS:
				tile = tileThorns;
				areaName = "Thorn Garden";
				break;
			case D_TOMB:
				floorTexture = skulls;
				areaName = "Tomb";
				break;
			case D_CHRISTMAS:
				areaName = "Munchkin Land";
				break;
			case D_ARMORY:
				areaName = "Old Armory";
				break;
			case D_MERCY:
				areaName = "Temple of Mercy";
				break;
			case D_TROG:
				areaName = "Blood Vendor";
				break;
			case D_ENCHANT:
				areaName = "Enchanted Altar";
				break;
			case D_TROVE:
				areaName = "Treasure Trove";
				break;
			default:
				Debug.LogError("generate terrain to right failed in switch statement");
				break;
		}

		if (currentAreaType >= D_CHRISTMAS) {
			foreach (var t in room.tiles) {
				if (t.name.Contains("Slanted") || t.name.Contains("Ramp") || t.name.Contains("Trap")) continue;	// FIXME:  hack
				tile = t.GetComponent<Rigidbody>();
				break;
			}
		}
		var nextRoom = GenerateAtIndex(x, tile, floorTexture, currentAreaType);
		rooms.Add(nextRoom);
		if (showTraps) GameObject.FindObjectOfType<TerrainController>().ShowTraps = true;
		
		if (previousAreaType != currentAreaType) announcer.AnnounceText("Entered " + areaName + "\nDepth: " + Depth);
		if (nextRoom.nextRoomType != nextRoom.terrainType) {
			var speech = "";
			switch(nextRoom.nextRoomType) {
				case D_FOREST:
					speech = "i feel a breeze";
					break;
				case D_WATER:
					speech = "i hear water";
					break;
				case D_CAVE:
					speech = "snoogin tracks";
					break;
				case D_THORNS:
					speech = "looks sharp";
					break;
				case D_TOMB:
					speech = "death lies ahead";
					break;
				case D_CHRISTMAS:
					speech = "i feel lucky";
					break;
				case D_ARMORY:
					speech = "i feel shrewd";
					break;
				case D_MERCY:
					speech = "this isn't so bad";
					break;
				case D_TROG:
					speech = "i smell blood";
					break;
				case D_ENCHANT:
					speech = "i sense power";
					break;
				case D_TROVE:
					speech = "the glint of gold";
					break;
				default:
					Debug.LogError("generate terrain to right failed in switch statement");
					break;
			}
			if (speech != "") {
				if (PlayerController.Instance.MainClass == Acter.C_GESTALT) {
					speech = "   ...";
				}
				PlayerController.Instance.Speak(speech);
			}
		}
		PlayerController.Instance.Heal(0.5f);
		
		previousAreaType = currentAreaType;
		CleanUp(x);
	}
	int ChooseNextRoomType (int areaType) {
		int rval;
		while (true) {
			if (areaType >= D_CHRISTMAS) {
				rval = Random.Range(D_FOREST, D_CHRISTMAS);
			}
			else if (Random.Range(0, 3) > 0) rval = areaType;
			else if (Random.Range(0, SpawnController.Instance.stinginess) != 0) rval = Random.Range(D_FOREST, D_CHRISTMAS);
			else {
				int specialType;
				switch(areaType) {
					case D_CAVE:
						specialType = D_MERCY;
						break;
					case D_THORNS:
						specialType = D_TROG;
						break;
					case D_WATER:
						specialType = D_ARMORY;
						break;
					case D_TOMB:
						specialType = D_TROVE;
						break;
					case D_FOREST:
						specialType = D_ENCHANT;
						break;
					default:
						specialType = D_CHRISTMAS;					
						break;
				}
				var chanceOfMunchkinLand = 2;
				chanceOfMunchkinLand += visitedSpecialRooms.FindAll(t => t == specialType).Count;
				specialType = Random.Range(0, chanceOfMunchkinLand) > 0 ? D_CHRISTMAS : specialType;
				visitedSpecialRooms.Add(specialType);
				rval = specialType;
			}
			if (rval == D_WATER && Depth < 3) continue;
			if (rval == D_FOREST && Depth < 4) continue;
			if (rval == D_TOMB && Depth < 5) continue;
			if (rval == D_THORNS && Depth < 1) continue;
			else break;
		}
		return rval;
	}
	
	const int Z_MIN = -1, Z_MAX = 1;
	Room GenerateAtIndex(int index, Rigidbody floorType, Texture texture, int areaType)
	{
		if (rooms.Exists(r => r.xIndex == index)) return null;
		print ("make index " + index + " depth " + Depth);
		if (texture == null) {
			var tmp = floorType.GetComponent<MeshRenderer>();
			if (tmp == null) tmp = floorType.GetComponentInChildren<MeshRenderer>();
			texture = tmp.sharedMaterial.GetTexture("_MainTex");
		}
		
		var room = new Room();
		room.nextRoomType = ChooseNextRoomType(areaType);
										
		room.terrainType = areaType;
		room.xIndex = index;
		int xmin = index * TILES_PER_ROOM;
		int xmax = xmin + TILES_PER_ROOM;
		bool hasRamp = Random.Range(0,3) == 0 && areaType < D_CHRISTMAS && hasSpawnedEmpty;
		int leftRampIndex = Random.Range(xmin, xmax - 1);
		int rightRampIndex = Random.Range(leftRampIndex + 1, xmax);
		var trapSeed = Random.Range(0, trapRarity);
		for (int x = xmin; x < xmax; ++x) {
			for (int z = Z_MIN; z <= Z_MAX; ++z) {
				Vector3 orig = tilePrefab.transform.position + new Vector3(x * TILE_SZ, 0, z * TILE_SZ);
				Rigidbody _;
				Texture _texture = texture;
				
				Rigidbody _floorType = floorType;
				
				if (trapSeed == 0) {
					_floorType = tileTrapped;
					_floorType.GetComponent<TerrainEffectTrap>().severity = Random.Range(1, Mathf.Max(Depth, 1)) * trapRarity;
				}
				if (areaType < D_CHRISTMAS)	trapSeed = Random.Range(0, trapRarity);
				
				if (hasRamp) {
					if (areaType == D_WATER) _texture = stone;
					if (x == leftRampIndex) {
						_= Instantiate(tileRampLeft, orig, floorType.transform.rotation) as Rigidbody;
					}
					else if (x == rightRampIndex) {
						_= Instantiate(tileRampRight, orig, floorType.transform.rotation) as Rigidbody;
					}
					else if (x < rightRampIndex && x > leftRampIndex) {
						_= Instantiate(_floorType, orig, floorType.transform.rotation) as Rigidbody;
						_.transform.position += new Vector3(0, 2.9f);
					}
					else {
						_= Instantiate(_floorType, orig, floorType.transform.rotation) as Rigidbody;
						if (areaType == D_WATER) _texture = texture;
					}
				}
				else _= Instantiate(_floorType, orig, floorType.transform.rotation) as Rigidbody;
				room.tiles.Add(_.gameObject);
				
				var rend = _.GetComponent<MeshRenderer>();
				if (rend == null) rend = _.GetComponentInChildren<MeshRenderer>();
				rend.material.SetTexture("_MainTex", _texture);
			}
		}
		// order of operations is important here. 
		Vector3 where = terrainTransition.transform.position;
		where.x = room.tiles[0].transform.position.x;
		var tt = Instantiate(terrainTransition, where, terrainTransition.transform.rotation) as BoxCollider;
		var transitionLeft = tt.GetComponent<GenerateTerrainTrigger>();
		transitionLeft.terrainController = this;
		transitionLeft.index = room.xIndex - 1;
		transitionLeft.room = room;
		where.x = room.tiles[room.tiles.Count - 1].transform.position.x;
		tt = Instantiate(terrainTransition, where, terrainTransition.transform.rotation) as BoxCollider;
		var transitionRight = tt.GetComponent<GenerateTerrainTrigger>();
		transitionRight.terrainController = this;
		transitionRight.index = room.xIndex + 1;
		transitionRight.room = room;
		room.tiles.Add(transitionLeft.gameObject);
		room.tiles.Add(transitionRight.gameObject);
		
		
		System.Func<Rigidbody, Rigidbody> Make = (tileType) => {
			var tile = Instantiate(tileType) as Rigidbody;
			tile.transform.position += new Vector3(xmin * TILE_SZ, 0, 0);
			room.tiles.Add(tile.gameObject);
			return tile;
		};
		Make(tileNearWall);
		Make(tileRoof);
		
		var farWall = Make(tileFarWall);
		farWall.GetComponentInChildren<TextMesh>().text = "" + Depth;
		switch (areaType) {
			case D_CAVE:
				farWall.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", stone);
				break;
			case D_WATER:
				farWall.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", stone);
				break;
			case D_FOREST:
				farWall.GetComponent<MeshRenderer>().material = forest;
				break;
			case D_THORNS:
				farWall.GetComponent<MeshRenderer>().material = forest;
				break;
			case D_TOMB:
				farWall.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", skulls);
				break;
			default: break;
		}
		if (areaType >= D_CHRISTMAS) farWall.GetComponent<MeshRenderer>().material = cathedral;
		
		// note that D_CHRISTMAS itself is incongruously handled by spawncontroller
		if (areaType == D_ARMORY) {
			#region FIXME: WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
			var items = new List<ItemController>();
			for (int lcv = 0; lcv < 3; ) {
				var item = spawnController.MakeSpecialItem();
				if (items.Find(i => i.name == item.name) != null) {
					continue;
				}
				else {
					item.transform.position = new Vector3((xmin + 1 + lcv) * TILE_SZ, 6, 0);
					items.Add(item);
					++lcv;
				}
			}
			items[0].OnPickup += a => {
				Destroy(items[1].gameObject);
				Destroy(items[2].gameObject);
				items[0].OnPickup = _ => {};
			};
			items[1].OnPickup += a => {
				Destroy(items[0].gameObject);
				Destroy(items[2].gameObject);
				items[1].OnPickup = _ => {};
			};
			items[2].OnPickup += a => {
				Destroy(items[0].gameObject);
				Destroy(items[1].gameObject);
				items[2].OnPickup = _ => {};
			};
			
//			foreach(var i in items) {			// FIXME:  WHYYYYYYYYYYYYYYYYYYYYYYYYYYYYY
//				i.OnPickUp += () => {
//					for (int lcv = items.Count - 1; lcv >= 0; --lcv) {
//						print (lcv);
//						if (items[lcv] != i) {
//							print ("destroying " + items[lcv]);
//							Destroy(items[lcv].gameObject);
//						}
//						else print ("ignoring " + items[lcv]);
//					}
//				};
//			}
			#endregion
		}
		else if (areaType == D_MERCY) {
			Instantiate(spawnController.statue, new Vector3((xmin + TILES_PER_ROOM / 2) * TILE_SZ, 0), Quaternion.identity);
		}
		else if (areaType == D_TROG) {
			var item = spawnController.MakeSpecialItem();
			item.transform.position = new Vector3((xmin + TILES_PER_ROOM / 2) * TILE_SZ, 4, 0);
			item.OnPickup += a => {
				WeaponController punishment = null;
				switch(Random.Range(0, 3)) {
					case 0:
						punishment = SpellGenerator.Instance().Pillar(WeaponController.DMG_FIRE);
						punishment.attackPower = a.MaxHitPoints / Acter.GLOBAL_DMG_SCALING;
						punishment.attackPower *= 2;	// overcome doubling of intrinsic 1 AC
//						item.name = "Burning " + item.name;
						break;
					case 1:
						punishment = SpellGenerator.Instance().Pillar(WeaponController.DMG_DEATH);
						punishment.attackPower = (a.HitPoints - 0.1f) / Acter.GLOBAL_DMG_SCALING;
//						item.name = "Death's Door " + item.name;
						break;
					case 2:
						a.MaxHitPoints--;
						var pc = a.GetComponent<PlayerController>();
						if (pc != null) pc.Speak("my heart!");
//						item.name = "Life Draining " + item.name;
						break;
					default: Debug.LogError("fell through case statement"); break;
				}
				if (punishment != null) {
					punishment.friendlyFireActive = punishment.attackActive = true;
					punishment.transform.position = a.transform.position;
					punishment.GetComponent<CapsuleCollider>().center = Vec.Zero;
					punishment.gameObject.SetActive(true);
					punishment.GetComponentInChildren<ParticleSystem>().transform.localPosition = Vec.Zero;
				}
			};
			item.name = "Cursed " + item.name;
			Instantiate(trogAltar).transform.position = new Vector3((xmin + TILES_PER_ROOM / 2) * TILE_SZ, 1.6f, 0);
		}
		else if (areaType == D_ENCHANT) {
			var altar = Instantiate(enchanterStatue);
			altar.transform.position = new Vector3((xmin + TILES_PER_ROOM / 2) * TILE_SZ, 1.6f, 0);
			altar.depth = Depth;
		}
		else if (Depth > 0) spawnController.Spawn(new Vector3(xmin, 0, Z_MIN) * TILE_SZ, new Vector3(xmax - 1, 0, Z_MAX) * TILE_SZ
							, Depth, areaType);
		hasSpawnedEmpty = true;
		generatedCount++;
		return room;
	}
}
