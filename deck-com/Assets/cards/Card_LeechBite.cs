using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_LeechBite : Card {

	public int damage;
	public int heal;

	public float range;

	public Card_LeechBite(){}
	public Card_LeechBite(XmlNode _node){
		node = _node;


		damage = int.Parse (node ["damage"].InnerXml);
		heal = int.Parse (node ["heal"].InnerXml);

		range = float.Parse (node ["range"].InnerXml);

	}

	public override void setupBlueprintCustom(){
		

	}

	public override void setupCustom(){
		Card_LeechBite blueprintCustom = (Card_LeechBite)blueprint;
		damage = blueprintCustom.damage;
		heal = blueprintCustom.heal;
		range = blueprintCustom.range;

	}

	private Charm_LatchOn getLatch(){
		foreach (Charm charm in Owner.Charms) {
			if (charm.idName == "latched_on") {
				return charm as Charm_LatchOn;
			}
		}
		return null;
	}

	public override bool checkIfCanBePlayedCustom(){
		return getLatch() != null;
	}

	public override void mouseEnterEffects(){
		if (getLatch () != null) {
			getLatch ().targetUnit.setHighlighted (true, baseHighlightColor);
		}
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForAttack (unit, damage);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		//can only hit the latched target
		Charm_LatchOn latch = getLatch();
		if (latch == null) {
			Debug.Log ("FUCK FUCK FUCK");
		}
		latch.targetUnit.setHighlighted(true, baseHighlightColor);

	}

	public override void passInUnitCustom(Unit unit){
		int damageVal = damage;

		if (!ignoreCasterCharms) {
			for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
				damageVal += Owner.Charms [i].getDamageMod (this, unit);
			}
		}

		if (damageVal < 0) {
			damageVal = 0;
		}

		doDamageToUnit( unit, damage );

		Owner.heal (heal);

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
