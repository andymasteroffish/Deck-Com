using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_BasicTargetBonus : Card {

	public bool anyUnit;
	public int numCardsToDraw;
	public int numActionsToGain;

	public Card_BasicTargetBonus(XmlNode _node){
		node = _node;

		anyUnit = bool.Parse (node ["any_unit"].InnerXml);
		numCardsToDraw = int.Parse (node ["cards_to_draw"].InnerXml);
		numActionsToGain = int.Parse (node ["actions_to_gain"].InnerXml);
	}


	public override void setupCustom(){
		type = CardType.Aid;

		description = "Gain";
		if (numCardsToDraw > 0) {
			description += "\n+" + numCardsToDraw + " card(s)";
		}
		if (numActionsToGain > 0) {
			description += "\n+" + numActionsToGain + " actions(s)";
		}
		if (anyUnit) {
			description += "\n(any unit)";
		}
	}
	
	public override void selectCardCustom(){
		WaitingForUnit = true;
		if (anyUnit) {
			Owner.board.highlightAllUnits (true, true, aidHighlightColor);
		} else {
			Owner.setHighlighted (true, aidHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		unit.deck.drawCards (numCardsToDraw);
		unit.gainActions (numActionsToGain);

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
