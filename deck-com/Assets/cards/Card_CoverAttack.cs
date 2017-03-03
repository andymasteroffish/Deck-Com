using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_CoverAttack : Card {

	public int damageMod;
	public int rangeMod;


	public Card_CoverAttack(XmlNode _node){
		node = _node;

		damageMod = int.Parse (node ["damage_mod"].InnerXml);
		rangeMod = int.Parse (node ["range_mod"].InnerXml);

	}

	public override void setupCustom(){
		type = CardType.Attack;

		string damageText = "Damage: " + (damageMod >= 0 ? "+" : "") + damageMod;
		string rangeText = "Range: " + (rangeMod >= 0 ? "+" : "") + rangeMod;

		description = damageText + "\n" + rangeText+"\nCan go through cover, destroying it";
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
		while (Owner.GM.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile) != null) {
			Owner.GM.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile).setCover (Tile.Cover.None); 
		}

		//do the damage
		doDamageToUnit( unit, getWeaponDamageToUnit(unit, damageMod) );
		//doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}
}
