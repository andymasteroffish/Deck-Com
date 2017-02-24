using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_Heal : Card {

	public int range;
	public int healAmount;

	public override void setupCustom(){
		type = CardType.Aid;

		textField.text = "heals "+healAmount+" at range "+range;
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Debug.Log ("range: " + range);

		Owner.GM.board.highlightUnitsInVisibleRange(Owner.CurTile, range, true, true, aidHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		int healVal = healAmount;

		for (int i=Owner.Charms.Count-1; i>=0; i--){
			healVal += Owner.Charms[i].getHealMod (this, unit);
		}

		if (healVal < 0) {
			healVal = 0;
		}

		unit.heal (healVal);
		finish ();
	}
}
