using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGO : MonoBehaviour {

	public BoxCollider2D collider;
	public SpriteRenderer spriteRend;

	public Sprite[] coverSprites;
	public Sprite goalSprite;

	private Tile tile;

	private bool isActive;

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
