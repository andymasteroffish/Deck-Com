using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_MoveAndAttack : Card {

	public int moveRange;
	public int damageMod;

	private bool onAttackStep;

	public override void setupCustom(){
		type = CardType.Attack;

		textField.text = "move up to " + moveRange + " spaces and attack at "+damageMod+" damage";
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		onAttackStep = false;
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, moveRange, false, false, moveHighlightColor);
	}

	//moving
	public override void passInTileCustom(Tile tile){
		if (!onAttackStep) {
			Owner.moveTo (tile);
			Owner.GM.board.clearHighlights ();

			onAttackStep = true;
			WaitingForUnit = true;

			//highlight for the attack
			int attackRange = Owner.Weapon.baseRange;
			Owner.GM.board.highlightTilesInRange (Owner.CurTile, attackRange, false, true, attackHighlightColor);
			Owner.GM.board.highlightUnitsInRange (Owner.CurTile, attackRange, true, true, attackHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		if (onAttackStep) {
			doWeaponDamageToUnit (unit, damageMod);
			finish ();
		}
	}

	//if the user cancels after moving, just discard the card
	public override void cancelCustom(){
		if (onAttackStep) {
			finish ();
		}
	}
}
