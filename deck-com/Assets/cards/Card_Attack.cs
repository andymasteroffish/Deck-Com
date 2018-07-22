using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Attack : Card {

//	public int damageMod;
//	public int rangeMod;
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
			range = int.Parse (node ["range"].InnerText);
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
		Owner.board.highlightTilesInVisibleRange (Owner.CurTile, range, baseHighlightColor);

		if (hitAllInRange) {
			Owner.CurTile.setHighlighted (false);
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

		//calculate cover
		Tile.Cover coverVal = Owner.board.getCover (Owner, unit);
		text += getInfoStringForCover (coverVal);

		//print the total
		//text += "\nDAMAGE: "+(getDamageToUnit(unit)+totalPrevention);
		int totalDamage = getDamageToUnit(unit)+totalPrevention;

		//set the target info text
		Owner.GM.targetInfoText.turnOn(text, totalDamage, unit);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		bool includeAI = true;
		if (Owner.isPlayerControlled == false) {
			if (Owner.aiProfile.willAttackAllies == false) {
				includeAI = false;
			}
		}

		Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, range, true, includeAI, baseHighlightColor);


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
		int damageVal = getDamageToUnit (unit);
		doDamageToUnit( unit, damageVal );

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
}
