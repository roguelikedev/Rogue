using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public interface IDepthSelectable {
	int Depth { get; }
	float Commonness { get; }
}

public class SpawnController : MonoBehaviour {
	#region instance variables
	public bool preventSpawn = false;
	public bool itemPinata = false;
	public float stinginess;
	
	public EnemyController enemyOrc;
	public EnemyController enemyGoblin;
	public EnemyController enemySnake;
	public EnemyController enemyOgre;
	public EnemyController enemyElf;
	public EnemyController enemyTentacleMonster;
	public EnemyController enemyCHUD;
	public EnemyController enemyWoman;
	public EnemyController enemyTroll;
	public EnemyController enemyDemon;
	public EnemyController enemySuccubus;
	public EnemyController enemyGhoul;
	public EnemyController enemyTreant;
	public EnemyController enemyIronLich;
	public EnemyController enemyShieldGolem;
	public EnemyController enemyNightgaunt;
	
	public WeaponController itemBrick;
	public WeaponController itemBow;
	public WeaponController itemBroom;
	public WeaponController itemShillalegh;
	public WeaponController itemKnife;
	public WeaponController itemMachete;
	public WeaponController itemBarMace;
	public WeaponController itemExecutionerSword;
	public WeaponController itemHiltless;
	public WeaponController itemHandCrossbow;
	public EstusController itemEstusFlask;
	public WeaponController itemLightArmor;
	public WeaponController itemArmor;
	public WeaponController itemHeavyArmor;
	public WeaponController itemLightSkirt;
	public WeaponController itemSkirt;
	public WeaponController itemHeavySkirt;
	public WeaponController itemLightHelmet;
	public WeaponController itemHelmet;
	public WeaponController itemHeavyHelmet;
	public WeaponController itemLightPauldrons;
	public WeaponController itemPauldrons;
	public WeaponController itemHeavyPauldrons;
	public WeaponController itemLightGreaves;
	public WeaponController itemGreaves;
	public WeaponController itemHeavyGreaves;
	public WeaponController itemShield;
	
	public TrinketController amulet;
	public TrinketController boot;
	public Breakable barrel;
	public Breakable statue;
	public TreasureChest treasureChest;
	public TreasureChest cardboardBox;
	public SignpostController signPost;
	public List<int> previouslyPosted = new List<int>();
	SpellGenerator SpellGenerator { get { return GetComponentInParent<SpellGenerator>(); } }
	#endregion
	#region fetch
	public static SpawnController Instance { get { return GameObject.FindObjectOfType<SpawnController>(); } }
	
	const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
	List<EnemyController> AllEnemies {
		get {
			var rval = new List<EnemyController>();
			foreach (FieldInfo fieldInfo in GetType().GetFields(flags))
			{
				if (fieldInfo.FieldType == enemyOrc.GetType()) rval.Add(fieldInfo.GetValue(this) as EnemyController);
			}
			return rval;
		}
	}
	List<WeaponController> AllEquipment {
		get {
			var rval = new List<WeaponController>();
			foreach (FieldInfo fieldInfo in GetType().GetFields(flags))
			{
				if (fieldInfo.FieldType == itemBarMace.GetType()) rval.Add(fieldInfo.GetValue(this) as WeaponController);
			}
			return rval;
		}
	}
	List<WeaponController> AllPrimaryAndSecondaryWeapons {
		get {
			var rval = AllEquipment;
			rval.RemoveAll(i => i.armorClass > 0);
			rval.FindAll(i => i.attackPower == 0 && i.payload == null).ForEach(i => Debug.LogError(i + " should not exist"));
			return rval;
		}
	}
	public List<IDepthSelectable> ChooseByDepth (List<IDepthSelectable> enemiesOrItems, float depth) {
		return ChooseByDepth (enemiesOrItems, depth, 3);
	}
	public List<IDepthSelectable> ChooseByDepth (List<IDepthSelectable> enemiesOrItems, float depth, int minChoices) {
		float iterations = 0;
		System.Func<int, List<IDepthSelectable>> FindRange = d => {
			var rval = new List<IDepthSelectable>();
			foreach (var e in enemiesOrItems) {
				if (Random.Range(0,11) > e.Commonness) continue;
				
				var depthToUse = e.Depth;
				var fuckInterfacesAllTheTime = e as WeaponController;
				if (fuckInterfacesAllTheTime != null && fuckInterfacesAllTheTime.payload != null) {
					depthToUse = fuckInterfacesAllTheTime.depth;		// don't consider the ammunition's depth
				}
				if (depthToUse == -1 || Mathf.Abs(depth - depthToUse) <= d * (1 + iterations * 0.1f)) rval.Add(e);
			}
			return rval;
		};
		var depthDelta = (int)Mathf.Max(1, Random.Range(0f, depth));
		var bestMatches = new List<IDepthSelectable>();
		
		while (bestMatches.Count < minChoices) {
			bestMatches.AddRange(FindRange(depthDelta).FindAll(e => !bestMatches.Contains(e)));
//			depthDelta += (int)Mathf.Max(0, (Random.Range(0f, depth) - Random.Range(0f, depth)));
			++iterations;
//			depthDelta += (int)Random.Range(0f, depth);
			//
			//			AllEnemies.FindAll(e => e.depth == currDepth + Random.Range(-depth, depth + 1));
		}
		
//		bestMatches.ForEach(i => { if (i.Depth != -1) print (i.Depth - depth + " out of depth " + i);});
	
		return bestMatches;
	}
	#endregion
	#region enemy	
	EnemyController ChooseMob(int depth, int areaType) {
		var bestMatches = new List<EnemyController>();
		while (bestMatches.Count == 0) {
			bestMatches = ChooseByDepth(AllEnemies.ConvertAll(e => e as IDepthSelectable), depth)
							.ConvertAll(i => i as EnemyController);
			if (depth < 27) bestMatches.Remove(enemyIronLich);
			switch(areaType) {
				case TerrainController.D_WATER:
					bestMatches.Add(enemyTentacleMonster);
					bestMatches.Remove(enemySuccubus);
					break;
				case TerrainController.D_CAVE:
					bestMatches.Add(enemyGoblin);
					break;
				case TerrainController.D_THORNS:
					bestMatches.RemoveAll(e => e.racialBaseHitPoints == 0);
					if (bestMatches.Count == 0) bestMatches.Add(enemyOrc);
					break;
				case TerrainController.D_TOMB:
					bestMatches.Add(enemySuccubus);
					bestMatches.Add(enemyGhoul);
					break;
				case TerrainController.D_FOREST:
					bestMatches.Add(enemyTreant);
					break;
				default:
					break;
			}
			if (Acter.LivingActers.FindAll(a => a.name.Contains("Woman")).Count > 2) bestMatches.Remove(enemyWoman);
			bestMatches.Remove(enemyNightgaunt);
		}
		
		var whichMob = bestMatches[Random.Range(0, bestMatches.Count)];
		whichMob = Instantiate(whichMob);
		
		return whichMob;
	}
	
	// FIXME: checking wantstoequip() here is all that prevents grapplecontrollers from grappling half a screen away
	// FIXME: it doesn't
	public void AddEquipment (EnemyController whichMob, bool christmas) {
		var equipmentLevel = 0;
		
		if (christmas) {
			var poss = new List<WeaponController>();
			System.Func<string> WhySoDissatisfied = () => {
				var rval = "";
				poss.ForEach (p => rval = rval + p.Name + " ");
				return rval;
			};
			for (int safety = 0; safety < 1000 && poss.Count == 0; ++safety) {
				poss.AddRange(ChooseByDepth(AllPrimaryAndSecondaryWeapons.ConvertAll(i => i as IDepthSelectable)
					, whichMob.ChallengeRating)
					.ConvertAll(i => i as WeaponController));
				if (safety > 998) print (whichMob + " sees " + WhySoDissatisfied());
				poss.RemoveAll(i => !whichMob.WantsToEquip(i));
				if (safety > 998) print (whichMob + " likes " + WhySoDissatisfied());
				poss.RemoveAll(i => i.name.Contains("wand"));
				if (whichMob.MainClass == Acter.C_ROGUE) poss.RemoveAll(i => i.IsMeleeWeapon);
			}
			var what = poss[Random.Range(0, poss.Count)];
			whichMob.Equip(EnchantEquipment(Instantiate(what), whichMob.level));
//			equipmentLevel += what.depth;
		}
		
		if (whichMob.MainClass == Acter.C_WIZARD) {
			var book = Instantiate(SpellGenerator.blankBook);
			var min = Mathf.Min(whichMob.ChallengeRating, (int)whichMob.spellpower);
			var max = Mathf.Max(whichMob.ChallengeRating, (int)whichMob.spellpower);
			var depth = Random.Range(min, max);
			SpellGenerator.Generate(depth, book);
			whichMob.Equip(book);
		}
		else if (Random.Range(0, 6) == 0) {
			var wand = Instantiate(SpellGenerator.blankWand);
			var dept = Mathf.Max(whichMob.level + 1, TerrainController.Instance.Depth);
			SpellGenerator.Generate(Random.Range(1, dept), wand);
			var leaf = wand.payload;
			
			while (leaf != null) {
				leaf.tag = "Untagged";
				leaf = leaf.payload;
			}
			wand.charges = Random.Range(1, dept) + Random.Range(1, dept);
			whichMob.Equip(wand);
			equipmentLevel += wand.Depth;
		}
		
		while (whichMob.ChallengeRating > equipmentLevel)
		{
			var poss = ChooseByDepth(AllEquipment.ConvertAll(i => i as IDepthSelectable), TerrainController.Instance.Depth)
					.ConvertAll(i => i as WeaponController);
			poss.RemoveAll(i => !whichMob.WantsToEquip(i));
			if (poss.Count == 0) {
				equipmentLevel++;	// avoid infinite
				continue;
			}
			var prefab = poss[Random.Range(0, poss.Count)] as WeaponController;
			if (whichMob.HasSlotEquipped(prefab.bodySlot)) {
				equipmentLevel++;	// avoid infinite
				continue;
			}
			var what = Instantiate(prefab);
			equipmentLevel += what.Depth;
			whichMob.Equip(what);
		}
	}
	
	public void AddLevels (EnemyController whichMob, int quantity) {
		for (int lcv = 0; lcv < quantity; ++lcv) {
			whichMob.GainLevel(whichMob.MainClass);
		}
	}
	#endregion
	#region loot
	public WeaponController EnchantEquipment (WeaponController what, float depth) {
		if (what.isSpellbook) {
			Debug.LogError("didn't expect to enchant a " + what);
		}
	
		if (what.bodySlot == WeaponController.EQ_WEAPON) {
			what.depth += (int) depth;
			
			WeaponController boom;
			if (what.payload != null && !what.IsMeleeWeapon) {		// replace previous enchantments on an arrow
				what.payload = Instantiate(what.payload);			// or i guess all but the first spell on a book
				SpellGenerator.Generate((int)depth, what.payload);
				boom = what.payload.payload;
			}
			else {		// replace previous enchantments on a melee weapon
				SpellGenerator.Generate((int)depth, what);
				boom = what.payload;
			}
			
//			boom.MapChildren(wc => wc.friendlyFireActive = false);
			
//			var leaf = boom;
//			var control = 0;
//			while (leaf != null) {
//				if (leaf.damageType != WeaponController.DMG_HEAL) leaf.friendlyFireActive = false;
//				leaf = leaf.payload;
//				if (control > 100) {
//					Debug.LogError("infinite loop");
//					break;
//				}
//			}
			var possibleEmitters = boom.GetComponentsInChildren<ParticleSystem>(true);
//			foreach(var emitter in possibleEmitters) {
			ParticleSystem emitter;// boom.GetComponentInChildren<ParticleSystem>();
			if (possibleEmitters.Length == 0) emitter = SpellGenerator.purpleParticles;
			else emitter = possibleEmitters[0];
			
			emitter = Instantiate(emitter);
			var prev = what.GetComponentInChildren<ParticleSystem>();
			if (prev != null) {
				prev.transform.parent = null;
				Destroy(prev.gameObject);
			}
			
			emitter.transform.parent = what.transform;
			var box = what.GetComponent<BoxCollider>();
			emitter.transform.localPosition = box.center;
			emitter.transform.localScale = new Vector3(box.size.magnitude, 1, 1);
//			emitter.startSize *= (box.size.magnitude / 27);
//			emitter.emissionRate *= box.size.magnitude;
			var c = emitter.startColor;
			c.a *= .25f + what.Depth / 20;
			emitter.startColor = c;
		}
		
		return what;
	}
	
	public ItemController MakeSpecialItem () {
		var poss = AllEquipment.ConvertAll(i => i as ItemController);
		poss.RemoveAll(i => i.name.Contains("wand") || i.name.Contains("book") || i.GetComponent<WeaponController>().IsArmor);
		poss = ChooseByDepth(poss.ConvertAll(i => i as IDepthSelectable), 27).ConvertAll(i => i as ItemController);
		poss.Add(amulet);
		var rval = poss[Random.Range(0, poss.Count)];
		
		if (rval == amulet) {
			rval = MakeTrinket(27);
		}
		else {
			rval = EnchantEquipment(Instantiate(rval as WeaponController), 27);
		}
		
		return rval;
	}
	
	TrinketController MakeTrinket (float depth) {
		switch(Random.Range(0,3)) {
			case 0:
				if (PlayerController.Instance.isAquatic) goto case 1;
				return Instantiate(boot);
			case 1:
				if (PlayerController.Instance.freeAction) goto case 2;
				var _boot = Instantiate(boot);
				_boot.waterWalking = false;
				_boot.freeAction = true;
				return _boot;
			case 2:
				return amulet.MakeAmulet(depth);
			default:
				Debug.LogError("broken switch");
				return null;
		}
	}
	
	TreasureChest MakeTreasureChest (float depth) {
		var rval = Instantiate(treasureChest);
		var possibilities = ChooseByDepth(AllEquipment.ConvertAll(i => i as IDepthSelectable), depth).ConvertAll(i => i as WeaponController);
		possibilities.Add(itemEstusFlask as WeaponController);
		if (PlayerController.Instance.spellpower > 1) {
			possibilities.Add(SpellGenerator.Instance().blankBook);
		}
		for (int lcv = 0; lcv <= depth;) {
			if (possibilities.Count == 0) break;
			var item = possibilities[Random.Range(0, possibilities.Count)];
			possibilities.Remove(item);
			
			var itemInstance = Instantiate(item);
			if (itemInstance.name.Contains("book")) {
				SpellGenerator.Generate((int)depth + 1, itemInstance);
//				print (itemInstance.Description);
			}
			else if (itemInstance.GetComponent<EstusController>() == null)  EnchantEquipment(itemInstance, depth);
			rval.contents.Add(itemInstance);
			lcv += itemInstance.Depth;
		}
		if (Random.Range(0,3) != 0) rval.contents.Add(MakeTrinket(depth));
		
		rval.contents.ForEach(i => {
			i.gameObject.SetActive(false);
		});
		return rval;
	}
	
	TreasureChest MakeEquipmentBox (float depth) {
		var rval = Instantiate(cardboardBox);
		var contents = ChooseByDepth(AllEquipment.ConvertAll(i => i as IDepthSelectable), depth).ConvertAll(i => i as WeaponController);
		contents.Add(itemEstusFlask as WeaponController);
		
		var itemInstance = Instantiate(contents[Random.Range(0, contents.Count)]);
		var enchantDepth = Random.Range(0, Mathf.Sqrt(depth)) - Random.Range(1, 10);
		if (enchantDepth > 0 && itemInstance.GetComponent<EstusController>() == null) {
			EnchantEquipment(itemInstance, enchantDepth);
		}
		rval.contents.Add(itemInstance);
		rval.contents.ForEach(i => i.gameObject.SetActive(false));
		return rval;
	}
	#endregion
	
	public void Spawn(Vector3 min, Vector3 max, int depth, int areaType) {
		System.Func<Vector3> RandomLocation = () => {
			var where = Vector3.Lerp(min, max, Random.Range(0f, 1f));
			where.y = 7;
			return where;
		};
		#region pinata
		if (itemPinata) {
//			MakeTreasureChest(6).transform.position = RandomLocation();
//			MakeTreasureChest(6).transform.position = RandomLocation();
//			MakeTreasureChest(6).transform.position = RandomLocation();
//			MakeTreasureChest(6).transform.position = RandomLocation();
//			MakeTreasureChest(1).transform.position = RandomLocation();
//			MakeTreasureChest(9).transform.position = RandomLocation();
//			MakeTreasureChest(27).transform.position = RandomLocation();			
			
			for (int lcv = 0; lcv < 9; ++lcv) {
//				var b = Instantiate(barrel);
//				b.transform.position = RandomLocation();
				var book = Instantiate(SpellGenerator.blankBook);
				print ("pinata'ing " + book.Description);
				SpellGenerator.Generate((lcv + 1) * 3, book);
				book.transform.position = RandomLocation();
			}

//			AllEquipment.ForEach(i => Instantiate(i).transform.position = RandomLocation());
//			AllItems.ForEach(i => Instantiate(i));
			
//			var _ = Instantiate(enemyWoman);
//			AddLevels(_, 100);
//			_.transform.position = RandomLocation();
//			itemPinata = false;
		}
		#endregion
		
		if (areaType == TerrainController.D_CHRISTMAS || areaType == TerrainController.D_TROVE) {
			for (int lcv = 0; lcv < 10 / stinginess; ++lcv) {
				var s = Random.Range(0, stinginess);
				if (s < 1 || areaType == TerrainController.D_TROVE) {
					MakeTreasureChest(depth).transform.position = RandomLocation();
				}
				else MakeEquipmentBox(depth).transform.position = RandomLocation();
			}
			return;
		}
		for (int lcv = 0; lcv < 3; ++lcv) {
			if (Random.Range(0, stinginess) > 1) continue;
			
			var s = Random.Range(0, 12);
			bool hasMadeSign = false;
			if (s == 0) MakeTreasureChest(depth).transform.position = RandomLocation();
			else if (s < 3 && !hasMadeSign) {
				Instantiate(signPost).transform.position = RandomLocation();
				hasMadeSign = true;
			}
			else if (s < 8) Instantiate(barrel).transform.position = RandomLocation();
			else MakeEquipmentBox(depth).transform.position = RandomLocation();
		}
		#region preventSpawn
		if (preventSpawn && depth > 0) {
//			var b = Instantiate(enemyShieldGolem);
//			b.transform.position = RandomLocation();
//			b.meleeMultiplier = 0.01f;
//			b.Equip(Instantiate(itemKnife));
			for (int lcv = 0; lcv < 1; ++lcv) {
//				var b = MakeEquipmentBox(depth);
//				var b = Instantiate(barrel);
				var b = Instantiate(enemyOgre);
				b.transform.position = RandomLocation();
			}
			//			var g = Instantiate(enemyGoblin);
			//			g.Equip(Instantiate(itemBow));
			return;
		}
		#endregion
		
		float encounterLevel = 0;
		bool isCaptain = Random.Range(0, 4) == 0;		// huge enemies are always captains
		while (encounterLevel <= depth) {
			EnemyController whichMob = ChooseMob(depth, areaType);
			if (whichMob == enemyIronLich || whichMob == enemyWoman) isCaptain = true;
			
			var remainingEL = encounterLevel - whichMob.racialLevel;
			if (remainingEL < depth) {
				var hugeness = Random.Range(-1, 1.5f);
				if (hugeness > 1.25f) {
					var tmp = whichMob.transform.localScale;
					tmp.x *= hugeness; tmp.y *= hugeness; tmp.z *= hugeness;
					whichMob.transform.localScale = tmp;
					whichMob.racialBaseHitPoints += hugeness;
//					whichMob.Heal(whichMob.MaxHitPoints);		// this *should* always happen via AddLevels()
					whichMob.meleeMultiplier = Mathf.Max(whichMob.meleeMultiplier * hugeness, whichMob.meleeMultiplier + hugeness);
					whichMob.name = "Huge " + whichMob.name;
					whichMob.racialLevel = (int)(whichMob.racialLevel * hugeness);
					isCaptain = true;
				}
				
				var levels = (int) (isCaptain ? Mathf.Min(remainingEL, whichMob.racialLevel * 2)
											  : Random.Range(0, Mathf.Max(remainingEL, whichMob.ChallengeRating)));
				
				AddLevels(whichMob, levels);
				if (isCaptain) {
					whichMob.GetComponentInChildren<DamageAnnouncer>().SetElite();
					whichMob.name = "Champion " + whichMob.name;
				}
			}
			
			AddEquipment(whichMob, isCaptain);
			isCaptain = false;
			encounterLevel += whichMob.ChallengeRating;
			whichMob.transform.position = RandomLocation();
		}
	}
}






