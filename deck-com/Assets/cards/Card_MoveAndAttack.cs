using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_MoveAndAttack : Card {

	public int moveRange;
	public int damageMod;

	private bool onAttackStep = false;

	public Card_MoveAndAttack(XmlNode _node){
		node = _node;

		moveRange = int.Parse (node ["range"].InnerXml);
		damageMod = int.Parse (node ["damage_mod"].InnerXml);
	}

	public override void setupCustom(){
		type = CardType.Attack;

		description = "move up to " + moveRange + " spaces and attack at "+damageMod+" damage";
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, moveRange, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		onAttackStep = false;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, moveRange, false, false, moveHighlightColor);
	}

	//moving
	public override void passInTileCustom(Tile tile){
		if (!onAttackStep) {
			Owner.moveTo (tile);
			Owner.board.clearHighlights ();

			onAttackStep = true;
			WaitingForUnit = true;

			//highlight for the attack
			selectCardForWeapon(0);
		}
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForWeapon (unit, damageMod);
	}

	public override void passInUnitCustom(Unit unit){
		if (onAttackStep) {
			onAttackStep = false;
			doDamageToUnit( unit, getWeaponDamageToUnit(unit, damageMod) );
			//doWeaponDamageToUnit (unit, damageMod);
			finish ();
		}
	}

	//if the user cancels after moving, just discard the card
	public override void cancelCustom(){
		if (onAttackStep) {
			onAttackStep = false;
			finish ();
		}
	}
}
