using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_BasicTargetBonus : Card {

	public bool anyUnit;
	public int numCardsToDraw;
	public int numActionsToGain;

	// Use this for initialization
	void Start () {
		textField.text = "Gain";
		if (numCardsToDraw > 0) {
			textField.text = "\n+" + numCardsToDraw + " card(s)";
		}
		if (numActionsToGain > 0) {
			textField.text = "\n+" + numActionsToGain + " actions(s)";
		}
		if (anyUnit) {
			textField.text += "\n(any unit)";
		}
	}
	
	public override void selectCardCustom(){
		WaitingForUnit = true;
		Owner.GM.board.highlightAllUnits (true, true, aidHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		unit.deck.drawCards (numCardsToDraw);
		unit.gainActions (numActionsToGain);

		finish ();
	}
}
