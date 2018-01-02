using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_CoverAttack : Card {

	public int damageMod;
	public int rangeMod;

	public Card_CoverAttack(){}
	public Card_CoverAttack(XmlNode _node){
		node = _node;

		damageMod = int.Parse (node ["damage_mod"].InnerXml);
		rangeMod = int.Parse (node ["range_mod"].InnerXml);

	}

	public override void setupBlueprintCustom(){
		
		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeMod >= 0 ? "+" : "") + rangeMod;

		description = damageText + "\n" + rangeText+"\nCan go through cover, destroying it";
	}

	public override void setupCustom(){
		Card_CoverAttack blueprintCustom = (Card_CoverAttack)blueprint;
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

		//kill cover
		while (Owner.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile) != null) {
			Owner.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile).setCover (Tile.Cover.None); 
		}

		//do the damage
		int damageVal =  getWeaponDamageToUnit(unit, damageMod); 
		doDamageToUnit( unit, damageVal );

		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			Owner.Charms [i].dealWeaponDamage (this, unit, damageVal);
		}
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
