using UnityEngine;
using System.Collections;
using UnityEngine.Profiling;

public class Tile {

	public enum Direction{Up, Right, Down, Left, None};
	public enum Cover{None, Part, Full};
	public enum SpawnProperty{None, Player, Foe, Exit};


	private GameManager gm;


	public bool useGameObject;

	//REMOVE THIS ONCE YOUR BOARD DOES NOT USE RAYCASTING!!!!
	//if this tile does not need any visual representaiton, the GameObject will be null
	public TileGO GO;

	private Cover cover;
	public SpawnProperty spawnProperty;
	private Tile[] adjacent = new Tile[4];

	private bool mouseIsOver;

	private TilePos pos;

	private bool isHighlighted;
	public Color highlightCol;

	public string debugText = "";

	public bool isVisibleToPlayer;

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
			GO = GameObjectManager.instance.getTileGO ();
			GO.activate(this);
		} else {
			GO = null;
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
		GO = null;

		cover = parent.cover;
		isHighlighted = parent.isHighlighted;
		highlightCol = parent.highlightCol;
		isVisibleToPlayer = parent.isVisibleToPlayer;
		Profiler.EndSample ();
	}

	public void setInfo(Tile[] _adjacent){
		for (int i = 0; i < 4; i++) {
			adjacent [i] = _adjacent [i];
		}
	}

	// Update is called once per frame
	void Update () {
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

	public bool MouseIsOver{
		get{
			return this.mouseIsOver;
		}
		set{
			mouseIsOver = value;
		}
	}


}
