using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_MoveAndAttack : Card {

	public int moveRange;
	public int damageMod;

	private bool onAttackStep = false;

	public Card_MoveAndAttack(){}
	public Card_MoveAndAttack(XmlNode _node){
		node = _node;

		moveRange = int.Parse (node ["range"].InnerXml);
		damageMod = int.Parse (node ["damage_mod"].InnerXml);
	}

	public override void setupBlueprintCustom(){
		type = CardType.Attack;
		showVisibilityIconsWhenHighlighting = true;
		description = "move up to " + moveRange + " spaces and attack at "+damageMod+" damage";
	}

	public override void setupCustom(){
		Card_MoveAndAttack blueprintCustom = (Card_MoveAndAttack)blueprint;
		moveRange = blueprintCustom.moveRange;
		damageMod = blueprintCustom.damageMod;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, moveRange, false, false, highlightColors[CardType.Movement]);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		onAttackStep = false;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, moveRange, false, false, highlightColors[CardType.Movement]);
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
			int damageVal = getWeaponDamageToUnit (unit, damageMod);
			doDamageToUnit( unit, damageVal );
			for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
				Owner.Charms [i].dealWeaponDamage (this, unit, damageVal);
			}
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
