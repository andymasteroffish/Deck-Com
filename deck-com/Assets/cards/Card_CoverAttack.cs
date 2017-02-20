using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_CoverAttack : Card {

	public int damageAdjust;
	public int rangeAdjust;


	public override void setupCustom(){
		type = CardType.Attack;

		string damageText = "Damage: " + (damageAdjust >= 0 ? "+" : "") + damageAdjust;
		string rangeText = "Range: " + (rangeAdjust >= 0 ? "+" : "") + rangeAdjust;

		textField.text = damageText + "\n" + rangeText+"\nCan go through cover, destroying it";
	}

	public override void mouseEnterEffects(){
		mouseEnterForWeapon (rangeAdjust);
	}

	public override void selectCardCustom(){

		selectCardForWeapon (rangeAdjust);

	}

	public override void passInUnitCustom(Unit unit){

		//kill cover
		while (Owner.GM.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile) != null) {
			Owner.GM.board.getFirstTileWithCover (Owner.CurTile, unit.CurTile).setCover (Tile.Cover.None); 
		}

		//do the damage
		doWeaponDamageToUnit (unit, damageAdjust);
		finish ();
	}
}
