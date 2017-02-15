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

	public override void selectCardCustom(){
		WaitingForUnit = true;
		int range = Owner.Weapon.baseRange + rangeAdjust;
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, true, attackHighlightColor);
		Owner.GM.board.highlightUnitsInRange (Owner.CurTile, range, true, true, attackHighlightColor);
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
