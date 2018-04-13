using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_LatchOn : Card {

	public int range;

	public Card_LatchOn(){}
	public Card_LatchOn(XmlNode _node){
		node = _node;

	}

	public override void setupBlueprintCustom(){
		range = int.Parse (node ["range"].InnerXml);
		
	}
	public override void setupCustom(){
		Card_LatchOn blueprintCustom = (Card_LatchOn)blueprint;
		range = blueprintCustom.range;
	}

	//DON'T LET THIS BE PLAYED IF THE OWNER IS ALREADY LATCHED ON
	public virtual bool checkIfCanBePlayedCustom(){
		return true;
	}

	public override void mouseEnterEffects(){
		highlightValidTiles ();

	}
		
	public override void selectCardCustom(){
		WaitingForTile = true;
		highlightValidTiles ();
	}

	private void highlightValidTiles(){
		List<Unit> validUnits = Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, range, Owner.isPlayerControlled, false, true, baseHighlightColor);
		List<Tile> validTiles = new List<Tile> ();

		//for each valid unit, find an empy adjacent tile in range
		foreach (Unit unit in validUnits) {
			for (int d = 0; d < 4; d++) {
				Tile adj = unit.CurTile.Adjacent [d];
				if (adj != null) {
					if (adj.CoverVal == Tile.Cover.None && Owner.board.getUnitOnTile (adj) == null) {
						validTiles.Add (adj);
					}
				}
			}
		}

		Owner.board.clearHighlights ();

		foreach (Tile tile in validTiles) {
			tile.setHighlighted (true, baseHighlightColor);
		}
	}

//	public override void passInUnitCustom(Unit unit){
//		
//	}

	public override void passInTileCustom(Tile tile){
		//move there
		Owner.moveTo(tile);

		//figure out who is a valid target here
		Unit otherUnit = null;

		for (int d = 0; d < 4; d++) {
			Unit thisUnit = Owner.board.getUnitOnTile (tile.Adjacent [d]);
			if (thisUnit != null) {
				//enemies only!
				if (thisUnit.isPlayerControlled != Owner.isPlayerControlled) {
					otherUnit = thisUnit;
				}
			}
		}

		Charm_LatchOn otherCharm = (Charm_LatchOn) otherUnit.addCharm ("latched_on");
		otherCharm.setTarget (Owner);

		Charm_LatchOn myCharm = (Charm_LatchOn) Owner.addCharm ("latched_on");
		myCharm.setTarget (otherUnit);

		finish ();

	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
