using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler {

	public static ObjectPooler instance;

	public int debugNewTileCount;

	//public List<Tile> activeTiles;
	private List<Tile> freeTiles;

	public ObjectPooler(){
		instance = this;
		debugNewTileCount = 0;

		//activeTiles = new List<Tile> ();
		freeTiles = new List<Tile> ();
	}


	public Tile getTile(Tile.Cover cover, Tile.SpawnProperty spawnProperty, GameManager gm){
		Tile tile = getTile ();
		tile.setup (cover, spawnProperty, gm);
		return tile;
	}

	public Tile getTile(Tile parent){
		Tile tile = getTile();
		tile.setFromParent (parent);;
		return tile;
	}

	public Tile getTile(){
		//return new Tile ();

		Tile tile;
		if (freeTiles.Count > 0) {
			tile = freeTiles [0];
			freeTiles.RemoveAt (0);
		} else {
			debugNewTileCount++;
			tile = new Tile ();
		}
		return tile;
	}


	public void retireTile(Tile tile){
		freeTiles.Add (tile);
	}

	public void printInfo(){
		Debug.Log ("new  tiles: " + debugNewTileCount);
		Debug.Log ("free tiles: " + freeTiles.Count);
	}

}
