using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_BasicTargetBonus : Card {

	public bool anyUnit;
	public int numCardsToDraw;
	public int numActionsToGain;

	// Use this for initialization
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
			Owner.GM.board.highlightAllUnits (true, true, aidHighlightColor);
		} else {
			Owner.setHighlighted (true, aidHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		unit.deck.drawCards (numCardsToDraw);
		unit.gainActions (numActionsToGain);

		finish ();
	}
}
