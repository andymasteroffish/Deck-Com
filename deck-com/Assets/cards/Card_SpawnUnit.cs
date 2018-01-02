using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_SpawnUnit : Card {

	public string spawn_id;
	public float range;
	public bool replace_parent;


	public Card_SpawnUnit(){}
	public Card_SpawnUnit(XmlNode _node){
		node = _node;

		spawn_id = node ["spawn_id"].InnerXml;

		replace_parent = false;
		if (node ["replace_parent"] != null) {
			replace_parent = bool.Parse (node ["replace_parent"].InnerXml);
		}

		//some cards may give negative charms and come with discard effects
		range = 0;
		if (node ["range"] != null) {
			range = float.Parse (node ["range"].InnerXml);
		}
	}

	// Use this for initialization
	public override void setupBlueprintCustom(){
		
	}

	public override void setupCustom(){
		Card_SpawnUnit blueprintCustom = (Card_SpawnUnit)blueprint;
		spawn_id = blueprintCustom.spawn_id;
		range = blueprintCustom.range;
		replace_parent = blueprintCustom.replace_parent;
	}

	public override void mouseEnterEffects(){

		doHighlight ();

	}
		
	public override void selectCardCustom(){
		WaitingForTile = true;

		doHighlight ();
	}

	private void doHighlight(){
		//filter out tiles with units (except for the one this unit is on if it will kill itself)
		List<Tile> tiles = Owner.board.highlightTilesInVisibleRange (Owner.CurTile, range, baseHighlightColor);
		foreach (Tile tile in tiles) {
			if (Owner.board.getUnitOnTile (tile) != null) {
				tile.setHighlighted (false);
			}
			if (tile.CoverVal != Tile.Cover.None) {
				tile.setHighlighted (false);
			}
		}
		if (replace_parent) {
			Owner.CurTile.setHighlighted (true, baseHighlightColor);
		}
	}

	public override void passInTileCustom(Tile tile){

//		Debug.Log ("spawn " + spawn_id);
//		Debug.Log ("board is AIsim: " + Owner.board.isAISim);
		Unit unit = UnitManager.instance.getUnitFromIdName (spawn_id);
		unit.setup (Owner.GM, Owner.board, tile);
		unit.reset ();
		unit.wakeUp ();
		unit.ActionsLeft = 0;
		unit.checkExausted ();
		Owner.board.units.Add (unit);

		if (replace_parent) {
			Debug.Log ("kill parent");
			Owner.killUnit ();
		}

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom (Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}
}
