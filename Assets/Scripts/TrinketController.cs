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
	bool hasGyppedPlayer = false;
	
	void Start() {
		cameraController = FindObjectOfType<CameraController>();
	}
	
	public TrinketController MakeAmulet (float depth) {
		var rval = Instantiate(this);
		
		if (Random.Range(0, 2) == 1) {
			if (!hasGyppedPlayer) {
				hasGyppedPlayer = true;
				return rval;
			}
			if (!GameObject.FindObjectOfType<TerrainController>().ShowTraps) {
				rval.trapFinding = true;
				return rval;
			}
		}
		
		float value = 0;
		while (value < depth) {
			float magnitude = Mathf.Max(2, Random.Range(depth / 2, depth));
			switch(Random.Range(0, 5)) {
			case 0:
				rval.meleeMultiplier += magnitude / 4;
				break;
			case 1:
				rval.armorClass += magnitude / 3;
				break;
			case 2:
				rval.speed += magnitude * 10;
				break;
			case 3:
				rval.spellPower += magnitude / 2;
				break;
			case 4:
				rval.regeneration = Mathf.Sqrt(magnitude);
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
		System.Func<float, string> Abbreviate = f => {
			var rval = f.ToString();
			return rval.Substring(0, Mathf.Min(3, rval.Length));
		};
		List<string> properties = new List<string>();
		if (meleeMultiplier != 0) properties.Add(" strength+" + Abbreviate(meleeMultiplier));
		if (armorClass != 0) properties.Add(" armor+" + Abbreviate(armorClass));
		if (speed != 0) properties.Add(" speed+" + Abbreviate(speed));
		if (spellPower != 0) properties.Add(" intelligence+" + Abbreviate(spellPower));
		if (regeneration != 0) properties.Add(" regeneration+" + Abbreviate(regeneration));
		if (waterWalking) properties.Add(" water walking");
		if (freeAction) properties.Add(" free action");
		if (trapFinding) properties.Add(" trap finding");
		if (npcSlowdown != 1) properties.Add(" bullet time x" + Abbreviate(npcSlowdown));
		if (hugeness != 1) properties.Add(" hugeness x" + Abbreviate(hugeness));
		if (properties.Count > 1) properties.Insert(properties.Count - 1, " and");
		if (properties.Count == 0) properties.Add(" adornment");
		string _properties = "";
		properties.ForEach(s => _properties += s);
		
		var noun = "amulet";
		if (freeAction || waterWalking) noun = "boot";
		if (npcSlowdown != 1 || hugeness != 1) noun = "glove";
		
		cameraController.NoteText("identified " + noun + " of" + _properties);
	}
}
