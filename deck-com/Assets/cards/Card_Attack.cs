using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {
	
	public int damage;
	public float range;

	public string charmToGiveTarget;

	bool hitAllInRange;
	bool canEnd;


	public Card_Attack (){}
	public Card_Attack(XmlNode _node){
		node = _node;



		range = 0;
		if (node ["range"] != null) {
			range = float.Parse (node ["range"].InnerText);
		}else{
			Debug.Log ("ATTACK CARD HAS NO RANGE: " + idName);
		}

		damage = 0;
		if (node ["damage"] != null) {
			damage = int.Parse (node ["damage"].InnerText);
		}else{
			Debug.Log ("ATTACK CARD HAS NO DAMAGE: " + idName);
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
		string damageText = "Damage: " + damage;
		string rangeText = "Range: " + Mathf.Floor(range);

		description = damageText + "\n" + rangeText;

	}

	public override void setupCustom(){
		Card_Attack blueprintCustom = (Card_Attack)blueprint;
		damage = blueprintCustom.damage;
		range = blueprintCustom.range;
		charmToGiveTarget = blueprintCustom.charmToGiveTarget;
		hitAllInRange = blueprintCustom.hitAllInRange;
		canEnd = blueprintCustom.canEnd;
	}


	public override void mouseEnterEffects(){
		mouseEnterForAttack ( range);

		if (hitAllInRange) {
			Owner.CurTile.setHighlighted (false);
		}
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForAttack (unit, damage);
	}

	public override void selectCardCustom(){
		selectCardForAttack (range);

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
		passInUnitForAttack (unit, damage);

		//if there is a charm to give and the attack does damage, add it
		if (charmToGiveTarget != "" && calculateAttackDamageToUnit(unit, damage) > 0) {
			unit.addCharm (charmToGiveTarget);
		}

		//for hitAllInRange, we keep going until we hit the last one
		if (canEnd) {
			finish ();
		}
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);

		//DOES NOT YET WORK WITH THE HIT ALL IN RANGE OPTION
	}

}
