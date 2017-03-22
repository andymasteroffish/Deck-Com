﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_TeamDash : Card {

	public int range;

	public string giftCardID;

	private bool onGiftStep = false;

	public Card_TeamDash(XmlNode _node){
		node = _node;

		range = int.Parse (node ["range"].InnerXml);
		giftCardID = node ["gift_id"].InnerXml;
	}

	public override void setupCustom(){
		type = Card.CardType.Movement;

		description = "move up to " + range + " spaces and give an a free "+range+" space move";
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		onGiftStep = false;
		WaitingForTile = true;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);
		onGiftStep = true;

		WaitingForUnit = true;

		Owner.board.clearHighlights ();
		Owner.board.highlightAllUnits (true, true, moveHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		Card gift = CardManager.instance.getCardFromIdName (giftCardID);
		gift.setup (unit, unit.deck);
		unit.deck.addCardToHand (gift);
		onGiftStep = false;

		finish ();
	}

	//if the user cancels after moving, just discard the card
	public override void cancelCustom(){
		if (onGiftStep) {
			onGiftStep = false;
			finish ();
		}
	}
}
