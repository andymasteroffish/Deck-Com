using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardHeal : Card {

	public int range;
	public int healAmount;

	public override void setupCustom(){
		textField.text = "heals "+healAmount+" at range "+range;
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Debug.Log ("range: " + range);
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, true, aidHighlightColor);
		Owner.GM.board.highlightUnitsInRange (Owner.CurTile, range, true, true, aidHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		int healVal = healAmount;

		healVal += Owner.Weapon.getHealMod (this);
		healVal += Owner.Charm.getHealMod (this);

		if (healVal < 0) {
			healVal = 0;
		}

		unit.heal (healVal);
		finish ();
	}
}
