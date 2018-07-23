using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_MoveAndAttack : Card {

	public int moveRange;

	public int damage;
	public float attackRange;

	private bool onAttackStep = false;

	public Card_MoveAndAttack(){}
	public Card_MoveAndAttack(XmlNode _node){
		node = _node;

		moveRange = int.Parse (node ["move_range"].InnerXml);
		damage = int.Parse (node ["damage"].InnerXml);
		attackRange = int.Parse (node ["attack_range"].InnerXml);
	}

	public override void setupBlueprintCustom(){
		showVisibilityIconsWhenHighlighting = true;
		description = "move up to " + moveRange + " spaces and attack for "+damage+" damage at "+attackRange+" range";
	}

	public override void setupCustom(){
		Card_MoveAndAttack blueprintCustom = (Card_MoveAndAttack)blueprint;
		moveRange = blueprintCustom.moveRange;
		damage = blueprintCustom.damage;
		attackRange = blueprintCustom.attackRange;
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
			selectCardForAttack(attackRange);
		}
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForAttack (unit, damage);
	}

	public override void passInUnitCustom(Unit unit){
		if (onAttackStep) {
			onAttackStep = false;
			int damageVal = calculateAttackDamageToUnit (unit, damage);// getWeaponDamageToUnit (unit, damageMod);
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
