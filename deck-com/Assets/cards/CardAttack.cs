using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardAttack : Card {

	public int rangeAdjust;
	public int damageAdjust;

	public override void setupCustom(){
		textField.text = "beat em up";
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, rangeAdjust, false, true, attackHighlightColor);
		Owner.GM.board.highlightUnitsInRange (Owner.CurTile, rangeAdjust, true, true, attackHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		unit.takeDamage (damageAdjust);
		finish ();
	}
}
