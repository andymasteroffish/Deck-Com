using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_DiscardAndDraw : Card {

	public bool mulliganEffect;
	public bool anyUnit;


	public Card_DiscardAndDraw (){}
	public Card_DiscardAndDraw(XmlNode _node){
		node = _node;


		anyUnit = false;
		if (node ["any_unit"] != null) {
			anyUnit = bool.Parse (node ["any_unit"].InnerXml);
		}

		mulliganEffect = false;
		if (node ["mulligan_effect"] != null) {
			mulliganEffect = bool.Parse (node ["mulligan_effect"].InnerXml);
		}
	}


	public override void setupCustom(){
		Card_DiscardAndDraw blueprintCustom = (Card_DiscardAndDraw)blueprint;
		mulliganEffect = blueprintCustom.mulliganEffect;
		anyUnit = blueprintCustom.anyUnit;
	}

	public override void mouseEnterEffects(){
		Owner.CurTile.setHighlighted (true, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		passInUnitCustom (Owner);
	}

	public override void passInUnitCustom(Unit unit){
		int numCards = Owner.deck.Hand.Count;

		Owner.deck.discardHand ();

		Owner.deck.drawCards (numCards - 1);
		
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);

	}

//	public override int getAIAttackRange(){
//		return getRangeForWeapon(rangeMod);
//	}
}
