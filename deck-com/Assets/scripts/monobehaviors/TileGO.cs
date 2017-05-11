using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGO : MonoBehaviour {

	public BoxCollider2D collider;
	public SpriteRenderer spriteRend;
	public SpriteRenderer fogSprite;

	public Sprite[] coverSprites;
	public Sprite goalSprite;

	private Tile tile;

	private bool isActive;

	public TextMesh debugtext;

	public void activate(Tile _tile){
		tile = _tile;
		isActive = true;
		gameObject.SetActive (true);
		transform.position = tile.Pos.getV3 ();
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
			spriteRend.color = Color.white;
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
	}
	void OnMouseExit(){
		tile.MouseIsOver = false;
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
