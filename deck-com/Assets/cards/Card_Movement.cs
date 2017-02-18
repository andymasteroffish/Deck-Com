using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_Movement : Card {

	public int range;

	public int bonusActions;
	public int bonusCards;

	public override void setupCustom(){
		type = Card.CardType.Movement;

		textField.text = "move up to " + range + " spaces";
		if (bonusActions > 0) {
			textField.text += "\n+" + bonusActions + " action(s)";
		}
		if (bonusCards > 0) {
			textField.text += "\n+" + bonusCards + " card(s)";
		}
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInRange (Owner.CurTile, range, false, false, moveHighlightColor);
//		List<Tile> selectable = Owner.GM.board.getTilesInRange (Owner.CurTile, range);
//		foreach (Tile tile in selectable) {
//			tile.setHighlighted (true);
//		}
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);
		if (bonusActions > 0) {
			Owner.gainActions (bonusActions);
		}
		if (bonusCards > 0) {
			Owner.deck.drawCards (bonusCards);
		}
		finish ();
	}
}
