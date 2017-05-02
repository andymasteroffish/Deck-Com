using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Movement : Card {

	public int range;

	public Card_Movement(XmlNode _node){
		node = _node;
		range = int.Parse (node ["range"].InnerText);


	}

	public override void setupCustom(){
		type = Card.CardType.Movement;

		description = "move up to " + range + " spaces";

	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, baseHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);
		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom ( Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}

}
