using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardMovement : Card {

	public int range;

	public override void setupCustom(){
		textField.text = "move up to " + range + " spaces";
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
		finish ();
	}
}
