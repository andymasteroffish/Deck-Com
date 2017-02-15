using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_BasicAOEAttack : Card {

	public int damage;
	public int throwRange;
	public float damageRange;

	public bool destroysCover;
	public bool harmsFriendly;


	public override void setupCustom(){

		type = CardType.AttackSpecial;

		textField.text = "Deals " + damage + " damage in area.\nRange "+throwRange;
		if (destroysCover){
			textField.text+="\nDestroys cover";
		}
		if (harmsFriendly){
			textField.text+="\ncan hurt allies";
		}
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, throwRange, false, true, attackHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		List<Tile> tiles = Owner.GM.board.getTilesInDist (tile, damageRange);

		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].setCover (Tile.Cover.None);
			Unit thisUnit = Owner.GM.board.getUnitOnTile (tiles [i]);
			if (thisUnit != null) {
				if (harmsFriendly || (thisUnit.isPlayerControlled != Owner.isPlayerControlled)) {
					thisUnit.takeDamage (damage);
				}
			}
		}

		finish ();

	}
}
