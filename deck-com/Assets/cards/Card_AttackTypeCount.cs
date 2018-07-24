using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_AttackTypeCount : Card {
	
	public float range;
	public int damagePerCard;
	public Card.CardType typeToCareAbout;

	public Card_AttackTypeCount (){}
	public Card_AttackTypeCount(XmlNode _node){
		node = _node;

		range = 0;
		if (node ["range"] != null) {
			range = float.Parse (node ["range"].InnerText);
		}else{
			Debug.Log ("ATTACK CARD HAS NO RANGE: " + idName);
		}


		typeToCareAbout = Card.CardType.Other;
		if (node ["type_to_care_about"] != null) {
			typeToCareAbout = cardTypeFromString (node ["type_to_care_about"].InnerText);
		}

		damagePerCard = 1;
		if (node ["damage_per_card"] != null) {
			damagePerCard = int.Parse (node ["damage_per_card"].InnerText);
		}

	}

	public override void setupBlueprintCustom(){
	}

	public override void setupCustom(){
		Card_AttackTypeCount blueprintCustom = (Card_AttackTypeCount)blueprint;
		range = blueprintCustom.range;
		typeToCareAbout = blueprintCustom.typeToCareAbout;
		damagePerCard = blueprintCustom.damagePerCard;
	}


	public override void mouseEnterEffects(){
		mouseEnterForAttack ( range);
	}

	public override void setPotentialTargetInfo(Unit unit){
		setPotentialTargetInfoTextForAttack (unit, getDamage());
	}

	public override void selectCardCustom(){
		selectCardForAttack (range);
	}

	public override void passInUnitCustom(Unit unit){
		passInUnitForAttack (unit, getDamage());

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}

	private int getDamage(){
		return Owner.deck.getCardsInHandOfType (Card.CardType.Spell) * damagePerCard;
	}
}
