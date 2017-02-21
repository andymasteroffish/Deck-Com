using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_Attack : Card {

	public int damageMod;
	public int rangeAdjust;


	public override void setupCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeAdjust >= 0 ? "+" : "") + rangeAdjust;

		textField.text = damageText + "\n" + rangeText;
	}

	public override void mouseEnterEffects(){
		mouseEnterForWeapon (rangeAdjust);
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void selectCardCustom(){

		selectCardForWeapon (rangeAdjust);
	}

	public override void passInUnitCustom(Unit unit){
		doDamageToUnit( unit, getWeaponDamageToUnit(unit, damageMod) );
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}
}
