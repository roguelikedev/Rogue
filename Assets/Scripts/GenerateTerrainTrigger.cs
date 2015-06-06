using UnityEngine;
using System.Collections;

public class GenerateTerrainTrigger : MonoBehaviour {
	public TerrainController terrainController;
	public int index;
	public object room;
	int NextRoomType { get { return (room as TerrainController.Room).nextRoomType; } }
	public SpriteRenderer dangerSign;
	public Sprite leftArrow;
	public Sprite rightArrow;
	public SpriteRenderer arrow;
	public TextMesh nextRoomName;
	public bool shouldNotWarn = false;
//	bool waitingOnDestroy = false;
	
//	void FixedUpdate () {
//		if (
//	
//	}
	
	public void Warn () {
		if (shouldNotWarn) return;
		
		var pc = PlayerController.Instance;
		pc.Speak(terrainController.LevelFeeling(NextRoomType));
		
		if (NextRoomType == TerrainController.D_TOMB) {
			System.Action<WeaponController> CheckTurnGhosts = w => {
				if (w.damageType == WeaponController.DMG_HEAL || w.damageType == WeaponController.DMG_RAISE) shouldNotWarn = true;
			};
			pc.EquippedWeapon.MapChildren(CheckTurnGhosts);
			pc.EquippedSecondaryWeapon.MapChildren(CheckTurnGhosts);
		}
		else if (NextRoomType == TerrainController.D_FOREST) {
			System.Action<WeaponController> CheckKillTreants = w => {
				if (w.damageType == WeaponController.DMG_DEATH) shouldNotWarn = true;
			};
			pc.EquippedWeapon.MapChildren(CheckKillTreants);
			pc.EquippedSecondaryWeapon.MapChildren(CheckKillTreants);
		}
		else if (NextRoomType == TerrainController.D_THORNS) {
			if (pc.EffectiveCurrentHP > 7 || pc.MainClass == Acter.C_BRUTE) shouldNotWarn = true;
		}
		else if (NextRoomType == TerrainController.D_WATER) shouldNotWarn = pc.level > 3;
		else shouldNotWarn = true;
		
		if (shouldNotWarn) return;
		dangerSign.gameObject.SetActive(true);
		arrow.gameObject.SetActive(true);
	}

	void OnTriggerEnter(Collider other) {
//		if (other.GetComponentInParent<PlayerController>() == null || waitingOnDestroy) return;
		if (other.GetComponentInParent<PlayerController>() == null) return;
		
		terrainController.GenerateTerrainAtIndexCallback(index, room);
		nextRoomName.text = "";
		dangerSign.gameObject.SetActive(false);
		arrow.gameObject.SetActive(false);
		
//		GameObject.FindObjectOfType<TerrainController>() .GenerateTerrainAtIndexCallback(index, room);
//		waitingOnDestroy = true;
//		Destroy(gameObject);
	}
}
