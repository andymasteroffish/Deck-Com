using UnityEngine;
using System.Collections;


public class Tile {

	private GameManager gm;

	//if this tile does not need any visual representaiton, the GameObject will be null
	public TileGO GO;

	public enum Direction{Up, Right, Down, Left, None};
	public enum Cover{None, Part, Full};
	public enum SpawnProperty{None, Player, Foe, Exit};

	private Cover cover;
	public SpawnProperty spawnProperty;
	private Tile[] adjacent = new Tile[4];

	private bool mouseIsOver;

	private TilePos pos;

	public TextMesh debugText;

	private bool isHighlighted;
	public Color highlightCol;

	//public BoxCollider2D collider;

	public Tile(int x, int y, Cover _cover, SpawnProperty _spawnProperty, bool usesGameObject, GameManager _gm){
		gm = _gm;
		spawnProperty = _spawnProperty;

		pos = new TilePos (x,y);

		setCover (_cover);

		if (usesGameObject) {
			GO = GameObjectManager.instance.getTileGO ();
			GO.activate (this);
		} else {
			GO = null;
		}

		mouseIsOver = false;

		highlightCol = Color.white;

		setHighlighted (false);
	}

	public void setInfo(Tile[] _adjacent){
		for (int i = 0; i < 4; i++) {
			adjacent [i] = _adjacent [i];
		}

		if (GO != null) {
			GO.refresh ();
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
		if (GO != null) {
			GO.refresh ();
		}
	}

	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		highlightCol = col;
		if (GO != null) {
			GO.refresh ();
		}
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
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
