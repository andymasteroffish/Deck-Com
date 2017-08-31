using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {

	public int damageMod;
	public int rangeMod;

	public Card_Attack (){}
	public Card_Attack(XmlNode _node){
		node = _node;

		rangeMod = 0;
		if (node ["range_mod"] != null) {
			rangeMod = int.Parse (node ["range_mod"].InnerText);
		}

		damageMod = 0;
		if (node ["damage_mod"] != null) {
			damageMod = int.Parse (node ["damage_mod"].InnerText);
		}

	}

	public override void setupBlueprintCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeMod >= 0 ? "+" : "") + rangeMod;

		description = damageText + "\n" + rangeText;

		if (damageMod == 0 && rangeMod == 0) {
			description = "Attack using your equiped weapon";
		}
	}

	public override void setupCustom(){
		Card_Attack blueprintCustom = (Card_Attack)blueprint;
		damageMod = blueprintCustom.damageMod;
		rangeMod = blueprintCustom.rangeMod;
	}

	public override void mouseEnterEffects(){
		mouseEnterForWeapon (rangeMod);
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void selectCardCustom(){

		selectCardForWeapon (rangeMod);
	}

	public override void passInUnitCustom(Unit unit){
		int damageVal = getWeaponDamageToUnit (unit, damageMod);
		doDamageToUnit( unit, damageVal );

		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			Owner.Charms [i].dealWeaponDamageCustom (this, unit, damageVal);
		}
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}

//	public override int getAIAttackRange(){
//		return getRangeForWeapon(rangeMod);
//	}
}
