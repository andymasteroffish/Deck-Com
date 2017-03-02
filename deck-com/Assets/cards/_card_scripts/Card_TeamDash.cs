using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_TeamDash : Card {

	public int range;

	public string giftCardID;

	private bool onGiftStep = false;

	public override void setupCustom(){
		type = Card.CardType.Movement;

		description = "move up to " + range + " spaces and give an a free "+range+" space move";
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		onGiftStep = false;
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);
		onGiftStep = true;

		WaitingForUnit = true;

		Owner.GM.board.clearHighlights ();
		Owner.GM.board.highlightAllUnits (true, true, moveHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){

		Debug.Log (unit.name + " gets a card");
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
