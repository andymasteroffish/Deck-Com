using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_Teleport : Card {
	
	public int moveRange;

	private bool onTeleportStep = false;

	private Unit selectedUnit;

	public Card_Teleport(){}
	public Card_Teleport(XmlNode _node){
		node = _node;

		moveRange = int.Parse (node ["move_range"].InnerText);
	}

	public override void setupBlueprintCustom(){
		type = CardType.Magic;
		showVisibilityIconsWhenHighlighting = true;
		description = "teleport a visible unit up to "+moveRange+" spaces";
	}

	public override void setupCustom(){
		Card_Teleport blueprintCustom = (Card_Teleport)blueprint;
		moveRange = blueprintCustom.moveRange;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		onTeleportStep = false;

		Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
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
