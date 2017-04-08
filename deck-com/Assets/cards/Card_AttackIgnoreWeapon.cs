using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_AttackIgnoreWeapon : Card {

	public int damage;
	public float range;

	public Card_AttackIgnoreWeapon(XmlNode _node){
		node = _node;

		damage = int.Parse (node ["damage"].InnerXml);
		range = float.Parse (node ["range"].InnerXml);
	}

	public override void setupCustom(){
		type = Card.CardType.Attack;

		string damageText = "Damage: " + damage;
		string rangeText = "Range: " + Mathf.Floor(range);

		description = damageText + "\n" + rangeText + "\nDoes not use weapon.";
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange (Owner.CurTile, range, baseHighlightColor);
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

		//calculate cover
		Tile.Cover coverVal = Owner.board.getCover (Owner, unit);
		text += "\n"+getInfoStringForCover (coverVal)+"\n";

		//print the total
		text += "\nDAMAGE: "+(getDamageToUnit(unit)+totalPrevention);

		//set the target info text
		Owner.GM.targetInfoText.turnOn(text, unit);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, range, true, true, baseHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		doDamageToUnit( unit, getDamageToUnit(unit) );
		finish ();
	}

	public int getDamageToUnit(Unit unit){
		int damageVal = damage;

		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			damageVal += Owner.Charms [i].getGeneralDamageMod (this, unit);
		}

		Tile.Cover coverVal = Owner.board.getCover (Owner, unit);
		damageVal = Owner.board.getNewDamageValFromCover (damageVal, coverVal);

		if (damageVal < 0) {
			damageVal = 0;
		}

		//Debug.Log ("cover: " + coverVal + "  damage: " + damageVal);

		return damageVal;
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
