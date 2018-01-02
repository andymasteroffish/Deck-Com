using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_Trade_Places : Card {

	public Card_Trade_Places(){}
	public Card_Trade_Places(XmlNode _node){
		node = _node;
	}

	public override void setupBlueprintCustom(){
		
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
		Owner.setHighlighted (false);
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
