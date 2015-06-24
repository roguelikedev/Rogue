using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrinketController : ItemController {
	public CameraController cameraController;
	public float meleeMultiplier;
	public bool waterWalking;
	public float armorClass;
	public float speed;
	public float spellPower;
	public bool freeAction;
	public float regeneration;
	public bool trapFinding;
	public float npcSlowdown = 1;
	public float hugeness = 1;
	public WeaponController aura;
	bool hasGyppedPlayer = false;
	bool friendless = false;
	bool hasStrangledPlayer = false;
	
	void Start() {
		cameraController = FindObjectOfType<CameraController>();
	}
	
	public TrinketController MakeAmulet (float depth) {
		var rval = Instantiate(this);
		rval.regeneration = -Mathf.Sqrt(depth);
		hasStrangledPlayer = true;
		return rval;
		
		switch (Random.Range (0, 5)) {
			case 0:
				if (!hasGyppedPlayer) {
					hasGyppedPlayer = true;
					return rval;
				}
				break;
			case 1: 
				if (!GameObject.FindObjectOfType<TerrainController>().ShowTraps) {
					rval.trapFinding = true;
					return rval;
				}
				break;
			case 2:
				if (!PlayerController.Instance.friendless) {
					rval.friendless = true;
					rval.OnPickup += a => {
						PlayerController.Instance.friendless = true;
						CameraController.Instance.AnnounceText("you are friendless\nyou murderer");
					};
					return rval;
				}
				break;
			case 3:
				if (!hasStrangledPlayer) {
					rval.regeneration = -Mathf.Sqrt(depth);
					hasStrangledPlayer = true;
					return rval;
				}
				break;
			default: break;
		}
		
		float value = 0;
		while (value < depth) {
			float magnitude = Mathf.Max(2, Random.Range(depth / 2, depth));
			switch(Random.Range(0, 5)) {
				case 0:
					rval.meleeMultiplier += magnitude / (SpawnController.Instance.stinginess * 2);
					break;
				case 1:
					rval.armorClass += magnitude / SpawnController.Instance.stinginess;
					break;
				case 2:
					rval.speed += magnitude * 30 / SpawnController.Instance.stinginess;
					break;
				case 3:
					rval.spellPower += magnitude / SpawnController.Instance.stinginess;
					break;
				case 4:
					rval.regeneration = Mathf.Sqrt(magnitude / SpawnController.Instance.stinginess);
					break;
				default:
					Debug.LogError("fell through switch statement");
					break;
			}
			value += magnitude;
		}
		return rval;
	}
	
	public void OnIdentify () {
		print (this);
		System.Func<float, string> Abbreviate = f => {
			var rval = f.ToString();
			return rval.Substring(0, Mathf.Min(3, rval.Length));
		};
		List<string> properties = new List<string>();
		if (meleeMultiplier != 0) properties.Add(" strength+" + Abbreviate(meleeMultiplier));
		if (armorClass != 0) properties.Add(" armor+" + Abbreviate(armorClass));
		if (speed != 0) properties.Add(" speed+" + Abbreviate(speed));
		if (spellPower != 0) properties.Add(" intelligence+" + Abbreviate(spellPower));
		if (waterWalking) properties.Add(" water walking");
		if (freeAction) properties.Add(" free action");
		if (trapFinding) properties.Add(" trap finding");
		if (npcSlowdown != 1) properties.Add(" bullet time x" + Abbreviate(npcSlowdown));
		if (hugeness != 1) properties.Add(" hugeness x" + Abbreviate(hugeness));
		if (properties.Count > 1) properties.Insert(properties.Count - 1, " and");
		if (friendless) properties.Add (" hatred");
		if (regeneration > 0) properties.Add(" regeneration+" + Abbreviate(regeneration));
		if (regeneration < 0) properties.Add(" strangulation" + Abbreviate(regeneration));
		
		if (properties.Count == 0) {
			properties.Add(" adornment");
		}
		string _properties = "";
		properties.ForEach(s => _properties += s);
		
		var noun = "amulet";
		if (freeAction || waterWalking) noun = "boot";
		if (npcSlowdown != 1 || hugeness != 1) noun = "glove";
		var aura = GetComponentInChildren<WeaponController>();
		if (aura) {
			noun = "belt";
			_properties = " " + aura.Description;
		}
		if (friendless || regeneration < 0) noun = "cursed " + noun;
		
		cameraController.NoteText("identified " + noun + " of" + _properties);
	}
}
