using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_TeamDash : Card {

	public int range;

	public string giftCardID;

	private bool onGiftStep = false;

	public Card_TeamDash(){}
	public Card_TeamDash(XmlNode _node){
		node = _node;

		range = int.Parse (node ["range"].InnerXml);
		giftCardID = node ["gift_id"].InnerXml;
	}

	public override void setupBlueprintCustom(){
		showVisibilityIconsWhenHighlighting = true;
		//description = "move up to " + range + " spaces and give an a free "+range+" space move to a teammate you can see";
	}

	public override void setupCustom(){
		Card_TeamDash blueprintCustom = (Card_TeamDash)blueprint;
		range = blueprintCustom.range;
		giftCardID = blueprintCustom.giftCardID;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, baseHighlightColor);
	}

	public override void selectCardCustom(){
		onGiftStep = false;
		WaitingForTile = true;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, baseHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);
		onGiftStep = true;

		WaitingForUnit = true;

		Owner.board.clearHighlights ();
		Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
		Owner.setHighlighted (false);	//can't select itself
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
