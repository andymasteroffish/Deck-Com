using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardAttack : Card {

	public int rangeAdjust;
	public int damageAdjust;

	public override void setupCustom(){
		string rangeText = "Range: " + (rangeAdjust >= 0 ? "+" : "") + rangeAdjust;
		string damageText = "Damage: " + (damageAdjust >= 0 ? "+" : "") + damageAdjust;
		textField.text = rangeText + "\n" + damageText;
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		int range = Owner.Weapon.baseRange + rangeAdjust;
		Debug.Log ("range: " + range);
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, true, attackHighlightColor);
		Owner.GM.board.highlightUnitsInRange (Owner.CurTile, range, true, true, attackHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		int damageVal = Owner.Weapon.baseDamage + damageAdjust;

		if (damageVal < 0) {
			damageVal = 0;
		}

		unit.takeDamage (damageVal);
		finish ();
	}
}
