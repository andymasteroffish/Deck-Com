using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card_GiveCharm : Card {

	public string idNameOfGift;
	public bool anyUnit;

	public string infoText;


	// Use this for initialization
	public override void setupCustom(){
		type = CardType.Other;
		description = infoText;
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
		unit.addCharm (idNameOfGift);

		finish ();
	}
}
