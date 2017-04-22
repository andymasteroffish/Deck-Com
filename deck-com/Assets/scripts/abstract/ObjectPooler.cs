using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler {

	public static ObjectPooler instance;

	public int debugNewTileCount, debugNewBoardCount;

	//private List<Tile> freeTiles;		//IF THE BOARD HANDLES ITS OWN TILES, YOU DON'T NEED TO POOL TILES AND CAN REMOVE THIS

	private List<Board> freeBoards;

	public ObjectPooler(){
		instance = this;
		debugNewTileCount = 0;

		//freeTiles = new List<Tile> ();
		freeBoards = new List<Board> ();
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
//		Debug.Log ("new  tiles: " + debugNewTileCount);
//		Debug.Log ("free tiles: " + freeTiles.Count);

		Debug.Log ("new  boards: " + debugNewBoardCount);
		Debug.Log ("free boards: " + freeBoards.Count);
	}

}
