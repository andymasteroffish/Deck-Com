﻿using UnityEngine;
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

		description = "Deals " + damage + " damage in area.\nRange "+throwRange;
		if (destroysCover){
			description += "\nDestroys cover";
		}
		if (harmsFriendly){
			description += "\ncan hurt allies";
		}
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, attackHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, attackHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		List<Tile> tiles = Owner.GM.board.getTilesInDist (tile, damageRange);

		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].setCover (Tile.Cover.None);
			Unit thisUnit = Owner.GM.board.getUnitOnTile (tiles [i]);
			if (thisUnit != null) {
				if (harmsFriendly || (thisUnit.isPlayerControlled != Owner.isPlayerControlled)) {
					doDamageToUnit (thisUnit, damage);
					//thisUnit.takeDamage (damage);
				}
			}
		}

		finish ();

	}
}
