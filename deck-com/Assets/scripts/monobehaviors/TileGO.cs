using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGO : MonoBehaviour {

	public BoxCollider2D collider;
	public SpriteRenderer spriteRend;
	public SpriteRenderer fogSprite;

	public Sprite[] coverSprites;
	public Sprite goalSprite;

	public Color coverColorGood, coverColExposed;
	public Sprite[] coverIconSprites;
	public SpriteRenderer[] coverIconSpriteRend;

	private Tile tile;

	private bool isActive;

	public TextMesh debugtext;

	public void activate(Tile _tile){
		tile = _tile;
		isActive = true;
		gameObject.SetActive (true);
		transform.position = tile.Pos.getV3 ();

		turnOffCoverIcons ();
		//refresh ();
	}

	public void deactivate(){
		tile = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "tile unused";
	}


	void Update () {
		if (tile.IsHighlighted) {
			spriteRend.color = tile.highlightCol;
		} else {
			spriteRend.color = GameManagerTacticsInterface.instance.areaColors [GameManagerTacticsInterface.instance.gm.CurAreaNum];
		}

		if (tile.spawnProperty == Tile.SpawnProperty.Exit) {
			spriteRend.sprite = goalSprite;
		} else {
			spriteRend.sprite = coverSprites [(int)tile.CoverVal];
		}

		fogSprite.enabled = !tile.isVisibleToPlayer;
		if (GameManagerTacticsInterface.instance.debugRevealFOW) {
			fogSprite.color = new Color (1, 1, 1, 0.2f);
		}
//		if (!tile.isVisibleToPlayer) {
//			spriteRend.sprite = coverSprites [0];
//			spriteRend.color = new Color (1, 1, 1, 0.5f);
//		}

		if (tile.debugText != debugtext.text) {
			debugtext.text = tile.debugText;
		}
			
	}

//	public void refresh(){
//		transform.position = tile.Pos.getV3 ();
//
//		spriteRend.sprite = coverSprites [(int)tile.CoverVal];
//
//		if (tile.spawnProperty == Tile.SpawnProperty.Exit) {
//			spriteRend.sprite = goalSprite;
//		}
//
//		if (tile.IsHighlighted) {
//			spriteRend.color = tile.highlightCol;
//		} else {
//			//Debug.Log ("time " + Time.time);
//			spriteRend.color = new Color (1, 1, 1);
//		}
//	}

	void OnMouseEnter(){
		tile.MouseIsOver = true;
		GameManagerTacticsInterface.instance.curMouseOverTile = tile;

		//testing
		if (GameManagerTacticsInterface.instance.debugShowTileDist) {
			for (int x = 0; x < GameManagerTacticsInterface.instance.gm.board.cols; x++) {
				for (int y = 0; y < GameManagerTacticsInterface.instance.gm.board.rows; y++) {
					GameManagerTacticsInterface.instance.gm.board.Grid [x, y].debugText = tile.visibleRangeDists [x, y].ToString ("N1");
					//Tile otherTile = GameManagerTacticsInterface.instance.gm.board.Grid [x, y];
					//otherTile.debugText = GameManagerTacticsInterface.instance.gm.board.dm.getDist(tile, otherTile) .ToString ("N1");
				}
			}
		}

		if (tile.IsHighlighted) {
			turnOnCoverIcons ();
		}

		//GameManagerTacticsInterface.instance.gm.board.updateUnitVisibilityIconsFromTile (tile, GameManagerTacticsInterface.instance.gm.activePlayerUnit);
	}
	void OnMouseExit(){
		tile.MouseIsOver = false;
		if (GameManagerTacticsInterface.instance.curMouseOverTile == tile) {
			GameManagerTacticsInterface.instance.curMouseOverTile = null;
		}
		turnOffCoverIcons ();
	}

	void turnOnCoverIcons(){

		//do nothing if this tile is cover
		if ((int)tile.CoverVal > 0) {
			return;
		}

		//go through and check adjacent tiles
		for (int i = 0; i < coverIconSpriteRend.Length; i++) {
			if (tile.Adjacent [i] != null) {
				if ((int)tile.Adjacent [i].CoverVal > 0) {
					coverIconSpriteRend [i].enabled = true;
					coverIconSpriteRend [i].sprite = coverIconSprites [(int)tile.Adjacent [i].CoverVal];
				}
			}
		}

		//if the lowest cover (from AI units) to this spot is none, than we are exposed
		List<Unit> enemies = GameManagerTacticsInterface.instance.gm.getAIUnits();
		int lowestCover = (int)Tile.Cover.Full;
		foreach (Unit enemy in enemies) {

			if (enemy.getIsVisibleToPlayer ()) {

				List<Tile> visible = GameManagerTacticsInterface.instance.gm.board.getTilesInVisibleRange (enemy.CurTile, enemy.getSightRange() + 1);
				if (visible.Contains (tile)) {

					Tile.Cover thisCover = GameManagerTacticsInterface.instance.gm.board.getCover (enemy.CurTile, tile);
					if ((int)thisCover < lowestCover) {
						lowestCover = (int)thisCover;
					}
				}
			}
		}

		Color colToUse = lowestCover == (int)Tile.Cover.None ? coverColExposed : coverColorGood;
		for (int i = 0; i < coverIconSpriteRend.Length; i++) {
			coverIconSpriteRend [i].color = colToUse;
		}
	}

	void turnOffCoverIcons(){
		for (int i = 0; i < coverIconSpriteRend.Length; i++) {
			coverIconSpriteRend [i].enabled = false;
		}
	}



	public bool IsActive{
		get{
			return this.isActive;
		}
	}

	public Tile MyTile {
		get {
			return this.tile;
		}

	}
}
