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

	private int podClRange = 2;
	private int podCLPadding = 1; //how much it can be over by and still be OK
	private int maxMoveDistAwayToSpawn = 2;

	private Board board;
	private GameManager gm;

	public PodPlacement(XmlNodeList node){

		//tetsing
		foeInfo.Add( new FoeInfo(1, "test_boy"));
	}

	public List<Unit> placeFoes(GameManager _gm, Board _board, int numPods, int podCL){
		gm = _gm;
		board = _board;

		List<Unit> foes = new List<Unit> ();

		for (int i = 0; i < numPods; i++) {
			foes.AddRange( makePod (podCL) );
		}

		return foes;
	}

	public List<Unit> makePod(int podCL){
		int curCL = 0;
		int targetCL = podCL + Random.Range (-podClRange, podClRange);

		List<FoeInfo> foesToSpawn = new List<FoeInfo> ();

		while (curCL < targetCL) {
			//find a foe
			FoeInfo thisFoe = foeInfo[(int)Random.Range(0,foeInfo.Count)];

			//make sure it would not push us over
			if (curCL + thisFoe.challengeLevel <= targetCL + podCLPadding) {

				//add it to the list
				foesToSpawn.Add(thisFoe);

				//mark down the challenge level
				curCL += thisFoe.challengeLevel;
			}

		}

		//find a place for them to start
		bool goodSpot = false;
		Tile originTile = null;
		List<Tile> spawnTiles = null;
		while (!goodSpot) {
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

		//add the foes
		List <Unit> newFoes = new List<Unit>();
		for (int i = 0; i < foesToSpawn.Count; i++) {
			Unit unit = UnitManager.instance.getUnitFromIdName (foesToSpawn [i].idName);
			int spawnTileID = (int)Random.Range (0, spawnTiles.Count);
			Tile spawnTile = spawnTiles[spawnTileID];
			spawnTiles.RemoveAt (spawnTileID);
			unit.setup (gm, board, spawnTile);
			newFoes.Add (unit);
		}


		//link them to wake up together
		foreach (Unit unit in newFoes) {
			foreach (Unit mate in newFoes) {
				unit.podmates.Add (mate);
			}
		}

		return newFoes;
	}


}
