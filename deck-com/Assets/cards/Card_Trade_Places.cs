using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_Trade_Places : Card {

	public int range;

	public Card_Trade_Places(XmlNode _node){
		node = _node;

		range = int.Parse (node ["range"].InnerXml);
	}

	public override void setupCustom(){
		type = CardType.Other;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, range, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		Owner.board.highlightUnitsInVisibleRange(Owner.CurTile, range, true, true, baseHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		Tile ownerStartPos = Owner.CurTile;
		Tile otherStartPos = unit.CurTile;
		unit.moveTo(ownerStartPos);
		Owner.moveTo (otherStartPos);

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
