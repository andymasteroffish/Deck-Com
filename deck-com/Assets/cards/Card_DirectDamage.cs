using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_DirectDamage : Card {

	public int damage;
	public float range;
	public bool useLineOfSight;


	public Card_DirectDamage(){}
	public Card_DirectDamage(XmlNode _node){
		node = _node;


		damage = int.Parse (node ["damage"].InnerXml);

		useLineOfSight = true;

		if (node ["range"] != null) {
			useLineOfSight = false;
			range = float.Parse (node ["range"].InnerXml);
		}

	}

	public override void setupBlueprintCustom(){
		type = Card.CardType.Magic;

		description = "deal " + damage + " damage to";// target unit";
		if (useLineOfSight) {
			description += " target visible unit.";
		} else {
			description += " a unit in range " + range+".";
		}
			
		description += "\nIgnores cover";
	}

	public override void setupCustom(){
		Card_DirectDamage blueprintCustom = (Card_DirectDamage)blueprint;
		damage = blueprintCustom.damage;
		range = blueprintCustom.range;
		useLineOfSight = blueprintCustom.useLineOfSight;
	}

	public override void mouseEnterEffects(){
		if (useLineOfSight) {
			Owner.board.highlightTilesVisibleToUnit (Owner, baseHighlightColor);
		} else {
			Owner.board.highlightTilesInVisibleRange (Owner.CurTile, range, baseHighlightColor);
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
		text += "\nDAMAGE: "+(damage+totalPrevention);

		//set the target info text
		Owner.GM.targetInfoText.turnOn(text, unit);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		if (useLineOfSight) {
			Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
		} else {
			Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, range, true, true, baseHighlightColor);
		}
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
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
