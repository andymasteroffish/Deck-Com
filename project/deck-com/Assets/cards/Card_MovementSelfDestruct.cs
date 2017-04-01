using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_MovementSelfDestruct : Card {

	public int range;

	public Card_MovementSelfDestruct(XmlNode _node){
		node = _node;

		range = int.Parse (node ["range"].InnerXml);
	}

	public override void setupCustom(){
		type = Card.CardType.Movement;

		baseActionCost = 0;

		description = "move up to " + range + " spaces. Costs 0 acitons. One Time Use.";
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


		finish (true);
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom ( Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}
}
