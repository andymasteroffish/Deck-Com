using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler {

	public static ObjectPooler instance;

	public int debugNewBoardCount, debugNewUnitCount;

	//private List<Tile> freeTiles;		//IF THE BOARD HANDLES ITS OWN TILES, YOU DON'T NEED TO POOL TILES AND CAN REMOVE THIS

	private List<Board> freeBoards;
	private List<Unit> freeUnits;

	public ObjectPooler(){
		instance = this;
		debugNewBoardCount = 0;
		debugNewUnitCount = 0;

		//freeTiles = new List<Tile> ();
		freeBoards = new List<Board> ();
		freeUnits = new List<Unit> ();
	}

	public Board getBoard(){
		Board board;
		if (freeBoards.Count > 0) {
			board = freeBoards [0];
			freeBoards.RemoveAt (0);
		} else {
			debugNewBoardCount++;
			board = new Board ();
		}
		return board;
	}
	public void retireBoard(Board board){
		freeBoards.Add (board);
	}
	public bool checkIfBoardHasBeenRetired(Board board){
		return freeBoards.Contains (board);
	}

	public Unit getUnit(){
		Unit unit;
		if (freeUnits.Count > 0) {
			unit = freeUnits [0];
			freeUnits.RemoveAt (0);
		} else {
			debugNewUnitCount++;
			unit = new Unit ();
		}
		return unit;
	}
	public void retireUnit(Unit unit){
		freeUnits.Add (unit);
	}

//	//ALL TILE FUNCTION CAN BE RMEOVED IF THE POOLED BOARDS MANAGE THEIR OWN TILES
//	public Tile getTile(Tile.Cover cover, Tile.SpawnProperty spawnProperty, GameManager gm){
//		Tile tile = getTile ();
//		tile.setup (cover, spawnProperty, gm);
//		return tile;
//	}
//	public Tile getTile(Tile parent){
//		Tile tile = getTile();
//		tile.setFromParent (parent);;
//		return tile;
//	}
//	public Tile getTile(){
//		//return new Tile ();
//
//		Tile tile;
//		if (freeTiles.Count > 0) {
//			tile = freeTiles [0];
//			freeTiles.RemoveAt (0);
//		} else {
//			debugNewTileCount++;
//			tile = new Tile ();
//		}
//		return tile;
//	}
//	public void retireTile(Tile tile){
//		freeTiles.Add (tile);
//	}


	public void printInfo(){
		Debug.Log ("new  units: " + debugNewUnitCount);
		Debug.Log ("free units: " + freeUnits.Count);

		Debug.Log ("new  boards: " + debugNewBoardCount);
		Debug.Log ("free boards: " + freeBoards.Count);
	}

}
