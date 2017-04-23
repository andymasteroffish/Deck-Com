using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_GiveBadCharm : Card {

	public string idNameOfGift;
	public bool anyUnit;

	public string infoText;

	public int bonusRandomDiscards;	//affects the target

	public Card_GiveBadCharm(XmlNode _node){
		node = _node;

		idNameOfGift = node ["gift_id"].InnerXml;

		anyUnit = true;
		if (node ["any_unit"] != null) {
			anyUnit = bool.Parse (node ["any_unit"].InnerXml);
		}

		//some cards may give negative charms and come with discard effects
		bonusRandomDiscards = 0;
		if (node["bonus_discard"] != null){
			bonusRandomDiscards = int.Parse(node["bonus_discard"].InnerXml);
		}
	}

	// Use this for initialization
	public override void setupCustom(){
		type = CardType.Other;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor);
	}
	
	public override void selectCardCustom(){
		WaitingForUnit = true;
		if (anyUnit) {
			Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
			//Owner.board.highlightAllUnits (true, true, baseHighlightColor);
		} else {
			Owner.setHighlighted (true, baseHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		Debug.Log ("give a bad charm");
		unit.addCharm (idNameOfGift);

		if (bonusRandomDiscards > 0) {
			for (int i = 0; i < bonusRandomDiscards; i++) {
				unit.deck.discardACardAtRandom ();
			}
		}
		unit.aiSimHasBeenCursedCount++;
		Debug.Log ("increase to " + unit.aiSimHasBeenCursedCount);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
