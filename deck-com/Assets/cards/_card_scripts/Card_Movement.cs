﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Movement : Card {

	public int range;

	public int bonusActions;
	public int bonusCards;

	public Card_Movement(XmlNode _node){
		node = _node;
		range = int.Parse (node ["range"].InnerText);

		if (node ["bonus_actions"] != null) {
			bonusActions = int.Parse (node ["bonus_actions"].InnerText);
		} else {
			bonusActions = 0;
		}
		if (node ["bonus_cards"] != null) {
			bonusCards = int.Parse (node ["bonus_cards"].InnerText);
		} else {
			bonusCards = 0;
		}
	}

	public override void setupCustom(){
		type = Card.CardType.Movement;

		description = "move up to " + range + " spaces";
		if (bonusActions > 0) {
			description += "\n+" + bonusActions + " action(s)";
		}
		if (bonusCards > 0) {
			description += "\n+" + bonusCards + " card(s)";
		}
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.GM.board.highlightTilesInMoveRange (Owner.CurTile, range, false, false, moveHighlightColor);
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