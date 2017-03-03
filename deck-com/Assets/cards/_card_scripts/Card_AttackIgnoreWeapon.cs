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
		type = Card.CardType.Other;

		string damageText = "Damage: " + damage;
		string rangeText = "Range: " + Mathf.Floor(range);

		description = damageText + "\n" + rangeText + "\nDoes not use weapon.";
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInVisibleRange (Owner.CurTile, range, attackHighlightColor);
	}

	public override void setPotentialTargetInfo(Unit unit){
		//start with the weapon
		string text = "Card +"+damage+"\n";

		//check my charms
		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			text += Owner.Charms [i].getDamageModifierText (this, unit);
		}

		//check if the unit has any charms that would alter damage values
		int totalPrevention = 0;
		for (int i = unit.Charms.Count - 1; i >= 0; i--) {
			text += unit.Charms [i].getDamagePreventionText (this, Owner);
			totalPrevention += unit.Charms [i].getDamageTakenMod (this, Owner);
		}

		//calculate cover
		Tile.Cover coverVal = Owner.GM.board.getCover (Owner, unit);
		text += "\n"+getInfoStringForCover (coverVal)+"\n";

		//print the total
		text += "\nDAMAGE: "+(damage+totalPrevention);

		//set the target info text
		Owner.GM.targetInfoText.turnOn(text, unit);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Owner.GM.board.highlightUnitsInVisibleRange (Owner.CurTile, range, true, true, attackHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		int damageVal = damage;

		Tile.Cover coverVal = Owner.GM.board.getCover (Owner, unit);
		damageVal = Owner.GM.board.getNewDamageValFromCover (damageVal, coverVal);

		if (damageVal < 0) {
			damageVal = 0;
		}


		doDamageToUnit( unit, damage );
		finish ();
	}
}
