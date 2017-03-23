using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_MoveAttackAOE : Card {

	public int damage;
	public int range;


	public Card_MoveAttackAOE(XmlNode _node){
		node = _node;

		damage = int.Parse(node["damage"].InnerXml);
		range = int.Parse(node["range"].InnerXml);
	}

	public override void setupCustom(){
		type = CardType.Movement;

		description = "move up to " + range + " spaces and deal "+damage+" to ALL adjacent units";
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		Owner.moveTo (tile);

		List<Unit> units = Owner.board.getAdjacentUnits (tile, true);
		for (int i = 0; i < units.Count; i++) {
			doDamageToUnit (units [i], damage);
			//units [i].takeDamage (damage);
		}


		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom ( Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}
}
