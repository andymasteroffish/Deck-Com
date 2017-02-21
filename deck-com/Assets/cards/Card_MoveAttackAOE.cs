using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_MoveAttackAOE : Card {

	public int range;
	public int damage;

	public override void setupCustom(){
		type = CardType.Movement;

		textField.text = "move up to " + range + " spaces and deal "+damage+" to ALL adjacent units";
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);

		List<Unit> units = Owner.GM.board.getAdjacentUnits (tile, true);
		for (int i = 0; i < units.Count; i++) {
			doDamageToUnit (units [i], damage);
			//units [i].takeDamage (damage);
		}


		finish ();
	}
}
