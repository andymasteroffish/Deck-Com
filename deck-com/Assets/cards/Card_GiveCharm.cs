using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_GiveCharm : Card {

	public string idNameOfGift;
	public bool anyUnit;


	public int bonusRandomDiscards;	//affects the target

	public Card_GiveCharm(){}
	public Card_GiveCharm(XmlNode _node){
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
	public override void setupBlueprintCustom(){
		type = CardType.Other;
	}
	public override void setupCustom(){
		Card_GiveCharm blueprintCustom = (Card_GiveCharm)blueprint;
		idNameOfGift = blueprintCustom.idNameOfGift;
		anyUnit = blueprintCustom.anyUnit;
		bonusRandomDiscards = blueprintCustom.bonusRandomDiscards;
	}

	public override void mouseEnterEffects(){
		if (anyUnit) {
			Owner.board.highlightTilesVisibleToUnit (Owner, baseHighlightColor);
		} else {
			Owner.CurTile.setHighlighted (true, baseHighlightColor);
		}
	}
		
	public override void selectCardCustom(){
		WaitingForUnit = true;
		if (anyUnit) {
			Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
		} else {
			Owner.board.clearHighlights ();
			Owner.setHighlighted (true, baseHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		unit.addCharm (idNameOfGift);

		if (bonusRandomDiscards > 0){
			unit.aiSimHasBeenCursedCount++;
			Debug.Log ("increase to " + unit.aiSimHasBeenCursedCount);

			if (!unit.isAISimUnit) {
				for (int i = 0; i < bonusRandomDiscards; i++) {
					unit.deck.discardACardAtRandom ();
				}
			}
		}

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
