using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class ScoreController : MonoBehaviour {
	static PlayerController PC { get { return PlayerController.Instance; } }
	public static ScoreController Instance { get { return GameObject.FindObjectOfType<ScoreController>(); } }
	[SerializeField] int scoreAmount;
	int lastScoreAmount;
	[SerializeField] bool didWin;
	[SerializeField] string killedBy;
	bool announcedHighScore = false;
	public string playerName = "";
	
	Dictionary <string, int> Scores { get {
		var reader = new StreamReader("test.txt");
		var rval = new Dictionary <string, int> ();
		while (reader.Peek() >= 0) 
		{
			var line = reader.ReadLine();
			rval[line.Split(':')[1]] = int.Parse(line.Split(':')[2]);
		}
		reader.Close();
		return rval;
	} }
//	void WriteToFile () {
//		var UID = SystemInfo.deviceUniqueIdentifier;
//		string line = UID + ":" + playerName + ":" + scoreAmount;
//		var d = Scores;
//		var file = new StreamWriter("test.txt");
//		if (!d.ContainsKey(playerName)) file.WriteLine(line);
//		foreach (var kvp in d) {
//			var currName = kvp.Key;
//			if (currName == playerName) file.WriteLine(line);
//			else file.WriteLine (kvp.Key + ":" + kvp.Value);
//		}
//		file.Close();
//	}
	
	List <string> LScores { get {
		var reader = new StreamReader("test.txt");
		var rval = new List <string> ();
		while (reader.Peek() >= 0) 
		{
			rval.Add(reader.ReadLine());
		}
		reader.Close();
		return rval;
	} }
	
	void LWriteToFile () {
		if (playerName == "") return;
		string currentEntry = SystemInfo.deviceUniqueIdentifier + ":" + playerName + ":" + scoreAmount;
		var d = LScores;
		var file = new StreamWriter("test.txt");
		if (!d.Contains(playerName)) file.WriteLine(currentEntry);
		else foreach (var line in d) {
			if (line.Contains(playerName)) file.WriteLine(currentEntry);
			else file.WriteLine (line);
		}
		file.Close();
	}

	void UpdateScore () {
		if (scoreAmount == lastScoreAmount) return;
		lastScoreAmount = scoreAmount;
		CameraController.Instance.UpdateScore(scoreAmount);
//		if (!Scores.ContainsKey(playerName) || Scores[playerName] < scoreAmount) {
//		if (!Scores.ContainsKey(playerName) || Scores[playerName] < scoreAmount) {
//			WriteToFile();
			LWriteToFile();
			if (!announcedHighScore && Scores[playerName] < scoreAmount) {
				CameraController.Instance.AnnounceText("new high\nscore!");
				announcedHighScore = true;
			}
//		}
	}
	
	IEnumerator DecreaseScoreByTime () {
		while (true) {
			yield return new WaitForSeconds(1);
			scoreAmount--;
		}
	}
	
	public void InitPlayerInfo (string name) {
		playerName = name;
		lastScoreAmount = -1;
		UpdateScore();
	}
	
	void Start () {
		if (!File.Exists("test.txt")) {
			File.CreateText("test.txt").Close();
		}
		var unknown = true;
		LScores.ForEach(l => {
			if (l.Contains(SystemInfo.deviceUniqueIdentifier.ToString())) {
				unknown = false;
				playerName = l.Split(':')[1];
			}
		} );
		CameraController.Instance.playerNameUnknown = unknown;
	}
	
	public void PlayerGainedLevel () {
		scoreAmount += (int)Mathf.Pow(PC.level * 3, 2);
	}
	
	public void PlayerEnteredRoom () {
		scoreAmount += (int) (TerrainController.Instance.Depth * 4);// / TerrainController.Instance.statuesDestroyed);
	}
	
	bool assignedWinPoints = false;
	public void PlayerHasWon () {
		if (!assignedWinPoints) scoreAmount *= 2;
		assignedWinPoints = true;
	}
	
	void FixedUpdate () {
		UpdateScore();
	}
	
	public void PlayerDied (string _killedBy) {
		killedBy = _killedBy;
	}
	
	public string ScoreList () {
		var rval = "";
		var d = Scores;
		var _d = new SortedList<int,string>();
		foreach (var kvp in d) {
			var name = kvp.Key.PadRight(13);
			var score = kvp.Value;//.ToString().PadLeft(9);
			_d.Add(score, name);
			rval += kvp.Key.PadRight(13) + kvp.Value.ToString().PadLeft(9) + "\n";
		}
		rval = "";
		foreach (var hax in _d) {
			rval = hax.Value.PadRight(13) + hax.Key.ToString().PadLeft(9) + "\n" + rval;
		}
		return rval;
	}
}
