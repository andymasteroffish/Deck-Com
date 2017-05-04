using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class FoeInfo{
	public int challengeLevel;
	public string idName;

	public FoeInfo(int _challengeLevel, string _idName){
		challengeLevel = _challengeLevel;
		idName = _idName;
	}
		
}

public class PodPlacement {

	private List<FoeInfo> foeInfo = new List<FoeInfo>();

	private float podClRangePrc = 0.4f;
	private int podCLPadding = 1; //how much it can be over by and still be OK
	private int maxMoveDistAwayToSpawn = 2;

	private Board board;
	private GameManager gm;

	public PodPlacement(XmlNodeList foeNodes){

		//populate the foe list
		foreach (XmlNode node in foeNodes) {
			int level = int.Parse (node ["level"].InnerText);
			string idName = node.Attributes ["idName"].Value;
			if (level >= 0) {
				foeInfo.Add (new FoeInfo (level, idName));
			}
		}

	}

	//these are wack numbers for now
	public void placeFoes(GameManager _gm, Board _board, int levelNum){
		int totalCL = levelNum * 6;
		int numPods = 3 + levelNum/3;
		if (numPods > 6) {
			numPods = 6;
		}
		int podCL = (int)Mathf.Ceil((float)totalCL / (float)numPods);
		placeFoes(_gm, _board, numPods, podCL);
	}

	public void placeFoes(GameManager _gm, Board _board, int numPods, int podCL){
		Debug.Log ("Placing " + numPods + " with a CL of " + podCL);
		gm = _gm;
		board = _board;

		//List<Unit> foes = new List<Unit> ();

		for (int i = 0; i < numPods; i++) {
			makePod (podCL);
			//foes.AddRange( makePod (podCL) );
		}

		//return foes;
	}

	public void makePod(int podCL){
		
		int curCL = 0;
		int rangeMod = (int) Mathf.Ceil( (float)podCL * podClRangePrc);
		if (rangeMod < 1) {
			rangeMod = 1;
		}
		int targetCL = podCL + Random.Range (-rangeMod, rangeMod);
		if (targetCL < 2) {
			targetCL = 2;
		}
		//Debug.Log (rangeMod);
		Debug.Log ("making a pod with CL " + targetCL);

		List<FoeInfo> foesToSpawn = new List<FoeInfo> ();

		int count = 0;
		while (curCL < targetCL && count < 1000) {
			count++;
			//find a foe
			FoeInfo thisFoe = foeInfo[(int)Random.Range(0,foeInfo.Count)];

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

		board.units.AddRange (newFoes);
	}


}
