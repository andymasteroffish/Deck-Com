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
