using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_MovementSelfDestruct : Card {

	public int range;


	public override void setupCustom(){
		type = Card.CardType.Movement;

		baseActionCost = 0;

		textField.text = "move up to " + range + " spaces. Costs 0 acitons. One Time Use.";
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);


		finish (true);
	}
}
