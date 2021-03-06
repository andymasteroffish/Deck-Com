﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Heal : Card {

	public int healAmount;
	public int range;

	public Card_Heal(){}
	public Card_Heal(XmlNode _node){
		node = _node;

		healAmount = int.Parse (node ["heal"].InnerXml);
		range = int.Parse (node ["range"].InnerXml);
	}

	public override void setupBlueprintCustom(){
		
		description = "heals "+healAmount+" at range "+range;
		if (manaCost > 0) {
			description += "\nMust have "+manaCost.ToString()+" other spell in hand to cast.";
		}
	}

	public override void setupCustom(){
		Card_Heal blueprintCustom = (Card_Heal)blueprint;
		healAmount = blueprintCustom.healAmount;
		range = blueprintCustom.range;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, range, baseHighlightColor);

	}

	public override void selectCardCustom(){
		WaitingForUnit = true;

		Owner.board.highlightUnitsInVisibleRange(Owner.CurTile, range, true, true, baseHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		int healVal = healAmount;

		for (int i=Owner.Charms.Count-1; i>=0; i--){
			healVal += Owner.Charms[i].getHealMod (this, unit);
		}

		if (healVal < 0) {
			healVal = 0;
		}

		unit.heal (healVal);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
