using UnityEngine;
using System.Collections;
using UnityEngine.Profiling;

public class Tile {

	public enum Direction{Up, Right, Down, Left, None};
	public enum Cover{None, Part, Full};
	public enum SpawnProperty{None, Player, Foe, Exit, StoreKey};


	private GameManager gm;


	public bool useGameObject;

	private Cover cover;
	public SpawnProperty spawnProperty;
	private Tile[] adjacent;// = new Tile[4];
	private Tile[] adjacentIncludingDiag;// = new Tile[8];

	private bool mouseIsOver;

	private TilePos pos;

	private bool isHighlighted;
	public Color highlightCol;

	public string debugText = "";

	public bool isVisibleToPlayer;

	//keeping track of visibilty
	private int cols, rows;
	public float [,] visibleRangeDists;
	public bool ignoreStoredRanges;	//flag to always check values for if cover gets destroyed during an AI turn

	//public BoxCollider2D collider;

	public Tile(){
	}
	public Tile(Cover _cover, SpawnProperty _spawnProperty, GameManager _gm){
		setup (_cover, _spawnProperty, _gm);
	}

	public void setup(Cover _cover, SpawnProperty _spawnProperty, GameManager _gm){
		gm = _gm;
		spawnProperty = _spawnProperty;

		useGameObject = true;//_useGameObject;

		setCover (_cover);
	}

	public void finalizeSetup(int x, int y){
		pos = new TilePos (x,y);

		if (useGameObject) {
			//GameObjectManager.instance.getTileGO ().activate (this);
			GameObjectManager.instance.getTileGO ().activate (this);
		}

		isVisibleToPlayer = false;
	
		mouseIsOver = false;
		highlightCol = Color.white;

		setHighlighted (false);
	}


	//creating new tiles for AI
	public Tile(Tile parent){
		setFromParent (parent);
	}
	public void setFromParent(Tile parent){
		Profiler.BeginSample ("setting tile from parent");
		pos = parent.pos;	//tile pos never changes so it can be a direct reference
		spawnProperty = parent.spawnProperty;

		useGameObject = false;

		cover = parent.cover;
		isHighlighted = parent.isHighlighted;
		highlightCol = parent.highlightCol;
		isVisibleToPlayer = parent.isVisibleToPlayer;
		//Debug.Log ("set from parent on frame " + Time.frameCount);

		ignoreStoredRanges = parent.ignoreStoredRanges;
		visibleRangeDists = parent.visibleRangeDists;	//this is a pointer so AI tiles should not change this value if the boardstate changes at all (ex: cover is destroyed)

		Profiler.EndSample ();
	}

	public void setInfo(Tile[] _adjacent, Tile[] _adjacentIncludingDiag){
		adjacent = _adjacent;
		adjacentIncludingDiag = _adjacentIncludingDiag;
//		for (int i = 0; i < 4; i++) {
//			adjacent [i] = _adjacent [i];
//		}
	}
		


	public void checkClick(){
		if (mouseIsOver && isHighlighted) {
			gm.tileClicked (this);
		}
	}

	public void setCover(Cover newCover){
		cover = newCover;
	}

	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		highlightCol = col;
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}

	public Cover getHighestAdjacentCover(){
		Cover highCover = Cover.None;
		foreach (Tile other in adjacent) {
			if (other != null) {
				if ((int)other.CoverVal > (int)highCover) {
					highCover = other.CoverVal;
				}
			}
		}
		return highCover;
	}


	//storing rnage and distance to other tiles
	public void createVisibilityGrid(int _cols, int _rows){
		Profiler.BeginSample ("create vis grid");
		cols = _cols;
		rows = _rows;
		ignoreStoredRanges = false;
		visibleRangeDists = new float [cols, rows];
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				visibleRangeDists [x, y] = -1;
			}
		}
		visibleRangeDists [pos.x, pos.y] = 0;
		Profiler.EndSample ();
	}

	public void setVisibleRangeDist(Tile other, float dist){
		if (!ignoreStoredRanges) {
			visibleRangeDists [other.pos.x, other.pos.y] = dist;
			other.visibleRangeDists [pos.x, pos.y] = dist;
//			if (dist < 900) {
//				Debug.Log ("set    " + Pos.x + "," + Pos.y + " <-> " + other.Pos.x + "," + other.Pos.y + " to " + dist); 
//			}
		}
	}

	public void clearVisibilityGridCrossingTile(int changedX, int changedY){
		int startX = 0;
		int startY = 0;
		int endX = cols - 1;
		int endY = rows - 1;

		if (pos.x < changedX) {
			startX = changedX;
		}
		if (pos.x > changedX) {
			endX = changedX;
		}
		if (pos.y < changedY) {
			startY = changedY;
		}
		if (pos.y > changedY) {
			endY = changedY;
		}

		for (int x = startX; x <= endX; x++) {
			for (int y = startY; y <= endY; y++) {
				//Debug.Log ("tile " + pos.x + "," + pos.y + " resetting dist to " + x + "," + y);
				visibleRangeDists [x, y] = -1;
			}
		}
	}

	//setters and getters

	public TilePos Pos {
		get {
			return this.pos;
		}
	}

	public Cover CoverVal {
		get {
			return this.cover;
		}
	}

	public bool IsHighlighted{
		get{
			return this.isHighlighted;
		}
	}

	public Tile[] Adjacent{
		get{
			return this.adjacent;
		}
	}

	public Tile[] AdjacentIncludingDiag{
		get{
			return this.adjacentIncludingDiag;
		}
	}

	public bool MouseIsOver{
		get{
			return this.mouseIsOver;
		}
		set{
			mouseIsOver = value;
		}
	}


}
