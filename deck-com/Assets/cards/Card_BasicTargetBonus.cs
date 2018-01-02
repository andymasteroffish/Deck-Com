using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_BasicTargetBonus : Card {

	public bool anyUnit;
	public int numCardsToDraw;
	public int numActionsToGain;

	public Card_BasicTargetBonus(){}
	public Card_BasicTargetBonus(XmlNode _node){
		node = _node;

		anyUnit = bool.Parse (node ["any_unit"].InnerXml);
		numCardsToDraw = int.Parse (node ["cards_to_draw"].InnerXml);
		numActionsToGain = int.Parse (node ["actions_to_gain"].InnerXml);
	}


	public override void setupBlueprintCustom(){
		
		description = "Gain";
		if (numCardsToDraw > 0) {
			description += "\n+" + numCardsToDraw + " card(s)";
		}
		if (numActionsToGain > 0) {
			description += "\n+" + numActionsToGain + " actions(s)";
		}
		if (anyUnit) {
			description += "\n(any visible unit)";
		}
	}

	public override void setupCustom(){
		Card_BasicTargetBonus blueprintCustom = (Card_BasicTargetBonus)blueprint;
		anyUnit = blueprintCustom.anyUnit;
		numCardsToDraw = blueprintCustom.numCardsToDraw;
		numActionsToGain = blueprintCustom.numActionsToGain;
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
			//Owner.board.clearHighlights ();
			//Owner.setHighlighted (true, baseHighlightColor);
			passInUnitCustom(Owner);
		}
	}

	public override void passInUnitCustom(Unit unit){
		unit.deck.drawCards (numCardsToDraw);
		unit.gainActions (numActionsToGain);
		unit.aiSimHasBeenAidedCount++;
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
