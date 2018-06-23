using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class FoeInfo{
	public int challengeLevel;
	public string idName;

	public int minArea, maxArea;

	public FoeInfo(int _challengeLevel, string _idName, int _minArea, int _maxArea){
		challengeLevel = _challengeLevel;
		idName = _idName;
		minArea = _minArea;
		maxArea = _maxArea;
	}
		
}

public class PodPlacement {

	private List<FoeInfo> foeInfo = new List<FoeInfo>();

	private float podClRangePrc = 0.4f;
	private int podCLPadding = 2; //how much it can be over by and still be OK
	private int maxMoveDistAwayToSpawn = 2;

	//private Board board;
	//private GameManager gm;

	public PodPlacement(XmlNodeList foeNodes){

		//populate the foe list
		foreach (XmlNode node in foeNodes) {
			int level = int.Parse (node ["cr"].InnerText);
			string idName = node.Attributes ["idName"].Value;
			int minArea = -1;
			if (node ["min_area"] != null) {
				minArea = int.Parse(node ["min_area"].InnerText);
			}
			int maxArea = 100;
			if (node ["max_area"] != null) {
				maxArea = int.Parse(node ["max_area"].InnerText);
			}
			if (level >= 0) {
				foeInfo.Add (new FoeInfo (level, idName, minArea, maxArea));
			}
		}

	}

	//these are wack numbers for now
	public void placeFoes(GameManager _gm, Board _board, int levelNum, int curArea){
		int totalCR = levelNum * 8;
		//Debug.Log ("total CL " + totalCR);
		int numPods = 3 + levelNum/3;
		if (numPods > 5) {
			numPods = 5;
		}
		int podCL = (int)Mathf.Ceil((float)totalCR / (float)numPods);

		if (GameManagerTacticsInterface.instance.debugOnePodPerLevel) {
			numPods = 1;
		}

		placePods(_gm, _board, numPods, podCL, curArea);
	}

	public void placeFoesIntoTheBreach(GameManager gm, Board board, int levelNum, int curArea){
		int podCL = levelNum * 2;// (int)Mathf.Ceil((float)totalCR / (float)numPods);

		makePod (gm, board, podCL, curArea);
	}



	public void placePods(GameManager gm, Board board, int numPods, int podCR, int curArea){
		//Debug.Log ("Placing " + numPods + " with a CL of " + podCR);
//		gm = _gm;
//		board = _board;

		for (int i = 0; i < numPods; i++) {
			makePod (gm, board, podCR, curArea);
		}
	}

	public void makePod(GameManager gm, Board board, int podCR, int curArea){
		
		int curCL = 0;
		int rangeMod = (int) Mathf.Ceil( (float)podCR * podClRangePrc);
		if (rangeMod < 1) {
			rangeMod = 1;
		}
		int targetCL = podCR + Random.Range (-rangeMod, rangeMod);
		if (targetCL < 2) {
			targetCL = 2;
		}
		//Debug.Log (rangeMod);
		//Debug.Log ("making a pod with CL " + targetCL);

		List<FoeInfo> potentialFoes = new List<FoeInfo> ();
		foreach (FoeInfo info in foeInfo) {
			if (info.minArea <= curArea && info.maxArea >= curArea) {
				potentialFoes.Add (info);
			} else {
				//Debug.Log (info.idName + " is out!");
			}
		}

		List<FoeInfo> foesToSpawn = new List<FoeInfo> ();

		int count = 0;
		while (curCL < targetCL && count < 1000) {
			count++;
			//find a foe
			FoeInfo thisFoe = potentialFoes[(int)Random.Range(0,potentialFoes.Count)];

			//make sure it would not push us over
			if (curCL + thisFoe.challengeLevel <= targetCL + podCLPadding) {
				//and that this would not be a pod of 1
				if ( !(foesToSpawn.Count == 0 && curCL + thisFoe.challengeLevel >= targetCL) ) {

					//add it to the list
					foesToSpawn.Add (thisFoe);
					//Debug.Log ("say hi to " + thisFoe.idName + ", CL " + thisFoe.challengeLevel);

					//mark down the challenge level
					curCL += thisFoe.challengeLevel;
				}
			}

		}
		if (count >= 1000) {
			Debug.Log ("fucked up making a foe list");
		}

		//find a place for them to start
		bool goodSpot = false;
		Tile originTile = null;
		List<Tile> spawnTiles = null;
		count = 0;
		while (!goodSpot && count < 1000) {
			count++;
			goodSpot = true;
			//give us a starting spot
			originTile = board.GetUnoccupiedTileWithSpawnProperty (Tile.SpawnProperty.Foe);
			//and get tiles within walking distance
			spawnTiles = board.getTilesInMoveRange (originTile, maxMoveDistAwayToSpawn, false, false);

			//remove the ones the player can see
			if (!GameManagerTacticsInterface.instance.intoTheBreachMode) {
				for (int i = spawnTiles.Count - 1; i >= 0; i--) {
					if (spawnTiles [i].isVisibleToPlayer) {
						spawnTiles.RemoveAt (i);
					}
				}
			}

			//make sure that there are enough tiles to nicely support the pod
			if (spawnTiles.Count < foesToSpawn.Count * 3) {
				goodSpot = false;
			}
		}
		if (count >= 1000) {
			Debug.Log ("fucked up finding starting place");
		}

		//Debug.Log ("starting 'em at " + originTile.Pos.x + "," + originTile.Pos.y + " with a list of " + spawnTiles.Count);

		//add the foes
		List <Unit> newFoes = new List<Unit>();
		for (int i = 0; i < foesToSpawn.Count; i++) {
			Unit unit = UnitManager.instance.getUnitFromIdName (foesToSpawn [i].idName);
			int spawnTileID = (int)Random.Range (0, spawnTiles.Count);
			if (spawnTiles.Count == 0) {
				Debug.Log ("NO MORE SPAWN TILES");
			}
			Tile spawnTile = spawnTiles[spawnTileID];
			spawnTiles.RemoveAt (spawnTileID);
			unit.setup (gm, board, spawnTile);
			newFoes.Add (unit);
			//Debug.Log ("Added unit " + unit.unitName + " on tile " + spawnTile.Pos.x + "," + spawnTile.Pos.y);
		}


		//link them to wake up together
		foreach (Unit unit in newFoes) {
			foreach (Unit mate in newFoes) {
				unit.podmates.Add (mate);
			}
		}
		//and define a leader
		newFoes[0].isPodLeader = true;

		board.units.AddRange (newFoes);
	}

	//THIS VERISON IS USED FOR REINFORCEMENTS
	//IF YOU USE INTO THE BREACH MODE, REMOVE THE VERISON ABOVE
	public void makePod(GameManager gm, Board board, Tile originTile, int podCR, int curArea){

		int curCL = 0;
		int rangeMod = (int) Mathf.Ceil( (float)podCR * podClRangePrc);
		if (rangeMod < 1) {
			rangeMod = 1;
		}
		int targetCL = podCR + Random.Range (-rangeMod, rangeMod);
		if (targetCL < 2) {
			targetCL = 2;
		}
		//Debug.Log (rangeMod);
		//Debug.Log ("making a pod with CL " + targetCL);

		List<FoeInfo> potentialFoes = new List<FoeInfo> ();
		foreach (FoeInfo info in foeInfo) {
			if (info.minArea <= curArea && info.maxArea >= curArea) {
				potentialFoes.Add (info);
			} else {
				//Debug.Log (info.idName + " is out!");
			}
		}

		List<FoeInfo> foesToSpawn = new List<FoeInfo> ();

		int count = 0;
		while (curCL < targetCL && count < 1000) {
			count++;
			//find a foe
			FoeInfo thisFoe = potentialFoes[(int)Random.Range(0,potentialFoes.Count)];

			//make sure it would not push us over
			if (curCL + thisFoe.challengeLevel <= targetCL + podCLPadding) {
				//and that this would not be a pod of 1
				if ( !(foesToSpawn.Count == 0 && curCL + thisFoe.challengeLevel >= targetCL) ) {

					//add it to the list
					foesToSpawn.Add (thisFoe);
					//Debug.Log ("say hi to " + thisFoe.idName + ", CL " + thisFoe.challengeLevel);

					//mark down the challenge level
					curCL += thisFoe.challengeLevel;
				}
			}

		}
		if (count >= 1000) {
			Debug.Log ("fucked up making a foe list");
		}

		//get tiles within walking distance
		List<Tile> spawnTiles = board.getTilesInMoveRange (originTile, maxMoveDistAwayToSpawn, false, false);

		//make sure that there are enough tiles to nicely support the pod
		if (spawnTiles.Count < foesToSpawn.Count * 3) {
			Debug.Log ("THAT SPAWN POS WAS BAD, DOG.");
		}

		//add the foes
		List <Unit> newFoes = new List<Unit>();
		for (int i = 0; i < foesToSpawn.Count; i++) {
			Unit unit = UnitManager.instance.getUnitFromIdName (foesToSpawn [i].idName);
			int spawnTileID = (int)Random.Range (0, spawnTiles.Count);
			if (spawnTiles.Count == 0) {
				Debug.Log ("NO MORE SPAWN TILES");
			}
			Tile spawnTile = spawnTiles[spawnTileID];
			spawnTiles.RemoveAt (spawnTileID);
			unit.setup (gm, board, spawnTile);
			newFoes.Add (unit);
			//Debug.Log ("Added unit " + unit.unitName + " on tile " + spawnTile.Pos.x + "," + spawnTile.Pos.y);
		}


		//link them to wake up together
		foreach (Unit unit in newFoes) {
			foreach (Unit mate in newFoes) {
				unit.podmates.Add (mate);
			}
		}
		//and define a leader
		newFoes[0].isPodLeader = true;

		board.units.AddRange (newFoes);
	}

}
