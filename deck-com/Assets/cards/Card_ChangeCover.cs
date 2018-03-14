using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Profiling;

public class Card_ChangeCover : Card {

	public int newCoverLevel;

	public Card_ChangeCover(){}
	public Card_ChangeCover(XmlNode _node){
		node = _node;

		newCoverLevel = int.Parse (node ["new_cover_level"].InnerXml);
	}

	public override void setupBlueprintCustom(){
		

	}

	public override void setupCustom(){
		Card_ChangeCover blueprintCustom = (Card_ChangeCover)blueprint;
		newCoverLevel = blueprintCustom.newCoverLevel;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor, newCoverLevel, false);

	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor, newCoverLevel, false);

	}

	public override void passInTileCustom(Tile tile){
		Owner.board.changeCover (tile, (Tile.Cover)newCoverLevel);

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom ( Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}




}
