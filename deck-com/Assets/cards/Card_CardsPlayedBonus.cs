using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_CardsPlayedBonus : Card {

	public bool anyUnit;

	public Card.CardType typeToCareAbout;

	public int numCardsToDraw;
	public int numActionsToGain;



	public Card_CardsPlayedBonus(){}
	public Card_CardsPlayedBonus(XmlNode _node){
		node = _node;

		anyUnit = false;
		if (node ["any_unit"] != null) {
			anyUnit = bool.Parse (node ["any_unit"].InnerXml);
		}

		numCardsToDraw = 0;
		if (node ["cards_to_draw"] != null) {
			numCardsToDraw = int.Parse (node ["cards_to_draw"].InnerXml);
		}

		numActionsToGain = 0;
		if (node ["actions_to_gain"] != null) {
			numActionsToGain = int.Parse (node ["actions_to_gain"].InnerXml);
		}

		typeToCareAbout = Card.CardType.Other;
		if (node ["type_to_care_about"] != null) {
			typeToCareAbout = cardTypeFromString (node ["type_to_care_about"].InnerText);
		}
	}


	public override void setupBlueprintCustom(){
		

	}

	public override void setupCustom(){
		Card_CardsPlayedBonus blueprintCustom = (Card_CardsPlayedBonus)blueprint;
		anyUnit = blueprintCustom.anyUnit;
		numCardsToDraw = blueprintCustom.numCardsToDraw;
		numActionsToGain = blueprintCustom.numActionsToGain;
		typeToCareAbout = blueprintCustom.typeToCareAbout;
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
			passInUnitCustom(Owner);
		}
	}

	public override void passInUnitCustom(Unit unit){
		int numCardsOfType = unit.getCardsOfTypePlayedThisTurn (typeToCareAbout);

		Debug.Log ("I lvoe " + typeToCareAbout + " and I see " + numCardsOfType);
		Debug.Log ("love to gain " + numActionsToGain + " and draw " + numCardsToDraw);

		unit.deck.drawCards (numCardsToDraw * numCardsOfType);
		unit.gainActions (numActionsToGain * numCardsOfType);
		unit.aiSimHasBeenAidedCount++;
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
