using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	private GameManager gm;

	public enum Direction{Up, Right, Down, Left, None};
	public enum Cover{None, Part, Full};
	public enum SpawnProperty{None, Player, Foe, Exit};

	public Sprite[] coverSprites;
	public SpriteRenderer spriteRend;

	public Sprite goalSprite;

	private Cover cover;
	public SpawnProperty spawnProperty;
	private Tile[] adjacent = new Tile[4];

	private bool mouseIsOver;

	private TilePos pos;

	public TextMesh debugText;

	private bool isHighlighted;

	public BoxCollider2D collider;

	public void setup(Cover _cover, SpawnProperty _spawnProperty){
		cover = _cover;
		spawnProperty = _spawnProperty;

//		if (spawnProperty == SpawnProperty.Foe) {
//			Debug.Log ("foe go here");
//		}
//		cover = (Cover)(int)Random.Range (0, 3);
//		if (Random.value < 0.5f) {
//			cover = Cover.None;
//		}

		setCover (cover);

		mouseIsOver = false;

		setHighlighted (false);
	}

	public void setInfo(int xPos, int yPos, Tile[] _adjacent,  GameManager _gm){
		pos = new TilePos(xPos, yPos);

		gm = _gm;

		for (int i = 0; i < 4; i++) {
			adjacent [i] = _adjacent [i];
		}

	}

	// Update is called once per frame
	void Update () {
	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}

	public void checkClick(){
		if (mouseIsOver && isHighlighted) {
			gm.tileClicked (this);
		}
	}

	public void setCover(Cover newCover){
		cover = newCover;
		spriteRend.sprite = coverSprites [(int)cover];

		if (spawnProperty == SpawnProperty.Exit) {
			spriteRend.sprite = goalSprite;
		}
	}

	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		if (isHighlighted) {
			spriteRend.color = col;
		} else {
			spriteRend.color = new Color (1, 1, 1);
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

}
