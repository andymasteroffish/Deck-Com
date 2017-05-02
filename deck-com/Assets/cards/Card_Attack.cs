using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {

	public int damageMod;
	public int rangeMod;

	public Card_Attack(XmlNode _node){
		node = _node;

		rangeMod = int.Parse (node ["range_mod"].InnerText);
		damageMod = int.Parse (node ["damage_mod"].InnerText);
	}

	public override void setupCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeMod >= 0 ? "+" : "") + rangeMod;

		description = damageText + "\n" + rangeText;
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
			Owner.Charms [i].dealWeaponDamage (unit, damageVal);
		}
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Debug.Log ("resolving attack");
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}

//	public override int getAIAttackRange(){
//		return getRangeForWeapon(rangeMod);
//	}
}
