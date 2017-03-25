using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Teleport : Card {

	public int targetRange;
	public int moveRange;

	private bool onTeleportStep = false;

	private Unit selectedUnit;

	public Card_Teleport(XmlNode _node){
		node = _node;

		targetRange = int.Parse (node ["target_range"].InnerXml);
		moveRange = int.Parse (node ["move_range"].InnerXml);
	}

	public override void setupCustom(){
		type = CardType.Other;

		description = "teleport a visible unit up to "+moveRange+" spaces";
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange (Owner.CurTile, targetRange, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		onTeleportStep = false;

		Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, targetRange, true, true, baseHighlightColor);

	}

	//selecting the unit to teleport
	public override void passInUnitCustom(Unit unit){
		if (!onTeleportStep) {
			onTeleportStep = true;
			WaitingForTile = true;
			WaitingForUnit = false;
			selectedUnit = unit;

			//highlight available spots
			Owner.board.highlightTilesInRange(selectedUnit.CurTile, moveRange, Tile.Cover.Part, false, baseHighlightColor);
		}
	}

	//moving
	public override void passInTileCustom(Tile tile){
		if (onTeleportStep) {
			selectedUnit.moveTo (tile);
			Owner.board.clearHighlights ();

			onTeleportStep = false;
			WaitingForTile = false;

			finish ();
		}
	}



}
