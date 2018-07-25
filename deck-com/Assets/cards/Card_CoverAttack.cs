using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_CoverAttack : Card {

	public int damage;
	public int range;

	public Card_CoverAttack(){}
	public Card_CoverAttack(XmlNode _node){
		node = _node;

		damage = int.Parse (node ["damage"].InnerXml);
		range = int.Parse (node ["range"].InnerXml);

	}

	public override void setupBlueprintCustom(){
		
		string damageText = "Damage: " + damage;
		string rangeText = "Range: " + range;

		description = damageText + "\n" + rangeText+"\nCan go through cover, destroying it";
	}

	public override void setupCustom(){
		Card_CoverAttack blueprintCustom = (Card_CoverAttack)blueprint;
		damage = blueprintCustom.damage;
		range = blueprintCustom.range;
	}

	public override void mouseEnterEffects(){
		mouseEnterForAttack (range);
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForAttack (unit, damage);
		//setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void selectCardCustom(){
		selectCardForAttack (range);
		//selectCardForWeapon (rangeMod);

	}

	public override void passInUnitCustom(Unit unit){
		//do the damage
		passInUnitForAttack (unit, damage);

		//kill cover
		while (Owner.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile) != null) {
			Owner.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile).setCover (Tile.Cover.None); 
		}



		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
