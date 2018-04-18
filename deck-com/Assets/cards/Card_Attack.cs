using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {

	public int damageMod;
	public int rangeMod;

	public string charmToGiveTarget;

	bool hitAllInRange;
	bool canEnd;


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

		charmToGiveTarget = "";
		if (node ["charm_to_give"] != null) {
			charmToGiveTarget = node ["charm_to_give"].InnerText;
		}

		hitAllInRange = false;
		if (node ["hit_all_in_range"] != null) {
			hitAllInRange = bool.Parse (node ["hit_all_in_range"].InnerXml);
		}

		canEnd = true;

	}

	public override void setupBlueprintCustom(){
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
		charmToGiveTarget = blueprintCustom.charmToGiveTarget;
		hitAllInRange = blueprintCustom.hitAllInRange;
		canEnd = blueprintCustom.canEnd;
	}


	public override void mouseEnterEffects(){
		mouseEnterForWeapon (rangeMod);

		if (hitAllInRange) {
			Owner.CurTile.setHighlighted (false);
		}
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void selectCardCustom(){
		selectCardForWeapon (rangeMod);

		//if we're hitting everybody, just do it
		if (hitAllInRange) {
			Owner.setHighlighted (false);
			canEnd = false;
			List<Unit> targets = Owner.board.getAllHighlightedUnits ();
			for (int i = 0; i < targets.Count; i++) {
				if (i == targets.Count - 1) {
					canEnd = true;
				}
				passInUnitCustom (targets [i]);
			}
		}
	}

	public override void passInUnitCustom(Unit unit){
		int damageVal = getWeaponDamageToUnit (unit, damageMod);
		doDamageToUnit( unit, damageVal );

		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			Owner.Charms [i].dealWeaponDamageCustom (this, unit, damageVal);
		}

		if (charmToGiveTarget != "" && damageVal > 0) {
			unit.addCharm (charmToGiveTarget);
		}

		if (canEnd) {
			finish ();
		}
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);

		//DOES NOT YET WORK WITH THE HIT ALL IN RANGE OPTION
	}

//	public override int getAIAttackRange(){
//		return getRangeForWeapon(rangeMod);
//	}
}
