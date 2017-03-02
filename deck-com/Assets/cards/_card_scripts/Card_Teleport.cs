using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Card_Teleport : Card {

	public int targetRange;
	public int moveRange;

	private bool onTeleportStep = false;

	private Unit selectedUnit;

	public override void setupCustom(){
		type = CardType.Other;

		description = "teleport a visible unit up to "+moveRange+" spaces";
	}

	public override void mouseEnterEffects(){
		Owner.GM.board.highlightTilesInVisibleRange (Owner.CurTile, targetRange, otherHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		onTeleportStep = false;

		Owner.GM.board.highlightUnitsInVisibleRange (Owner.CurTile, targetRange, true, true, otherHighlightColor);

	}

	//selecting the unit to teleport
	public override void passInUnitCustom(Unit unit){
		if (!onTeleportStep) {
			onTeleportStep = true;
			WaitingForTile = true;
			WaitingForUnit = false;
			selectedUnit = unit;

			//highlight available spots
			Owner.GM.board.highlightTilesInRange(selectedUnit.CurTile, moveRange, Tile.Cover.Part, false, otherHighlightColor);
		}
	}

	//moving
	public override void passInTileCustom(Tile tile){
		if (onTeleportStep) {
			selectedUnit.moveTo (tile);
			Owner.GM.board.clearHighlights ();

			onTeleportStep = false;
			WaitingForTile = false;

			finish ();
		}
	}



}
