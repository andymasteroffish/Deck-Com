﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {

	public int damageMod;
	public int rangeMod;

	public Card_Attack(XmlNode _node){
		node = _node;

		rangeMod = int.Parse (node ["range_mod"].InnerXml);
		damageMod = int.Parse (node ["damage_mod"].InnerXml);
	}

	public override void setupCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeMod >= 0 ? "+" : "") + rangeMod;

		description = damageText + "\n" + rangeText;
	}

	public override void mouseEnterEffects(){
		Debug.Log ("mouse enter");
		mouseEnterForWeapon (rangeMod);
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void selectCardCustom(){

		selectCardForWeapon (rangeMod);
	}

	public override void passInUnitCustom(Unit unit){
		doDamageToUnit( unit, getWeaponDamageToUnit(unit, damageMod) );
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}
}