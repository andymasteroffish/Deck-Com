using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_Attack : Card {

	public int damageAdjust;
	public int rangeAdjust;


	public override void setupCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + (damageAdjust >= 0 ? "+" : "") + damageAdjust;
		string rangeText = "Range: " + (rangeAdjust >= 0 ? "+" : "") + rangeAdjust;

		textField.text = damageText + "\n" + rangeText;
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		int range = Owner.Weapon.baseRange + rangeAdjust;
		//Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, true, attackHighlightColor);
		Owner.GM.board.highlightUnitsInRange (Owner.CurTile, range, true, true, attackHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}
}
