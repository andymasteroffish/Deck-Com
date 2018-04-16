﻿using System.Collections;
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
		//start with the weapon
		string text = "Card +"+damage+"\n";

		//check my charms
		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			text += Owner.Charms [i].getGeneralDamageModifierText (this, unit);
		}


		//check if the unit has any charms that would alter damage values
		int totalPrevention = 0;
		for (int i = unit.Charms.Count - 1; i >= 0; i--) {
			text += unit.Charms [i].getDamagePreventionText (this, Owner);
			totalPrevention += unit.Charms [i].getDamageTakenMod (this, Owner);
		}

		if (totalPrevention < -damage) {
			totalPrevention = -damage;
		}

		//print the total
		//text += "\nDAMAGE: "+(damage+totalPrevention);
		int totalDamage = damage+totalPrevention;

		//set the target info text
		Owner.GM.targetInfoText.turnOn(text, totalDamage, unit);
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
				damageVal += Owner.Charms [i].getGeneralDamageMod (this, unit);
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