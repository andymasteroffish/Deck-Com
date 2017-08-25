using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_RemoveCharm : Card {

	public bool removesEquipment;

	public Card_RemoveCharm(){}
	public Card_RemoveCharm(XmlNode _node){
		node = _node;

		removesEquipment = bool.Parse (node ["hits_equipment"].InnerXml);
	}

	// Use this for initialization
	public override void setupBlueprintCustom(){
		type = CardType.Other;
	}

	public override void setupCustom(){
		Card_RemoveCharm blueprintCustom = (Card_RemoveCharm)blueprint;
		removesEquipment = blueprintCustom.removesEquipment;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesVisibleToUnit(Owner, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForUnit = true;
		Owner.board.highlightUnitsVisibleToUnit (Owner, true, true, baseHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		Debug.Log ("Kill some charms");

		for (int i = unit.Charms.Count - 1; i >= 0; i--) {
			if (unit.Charms [i].type == Charm.CharmType.Equipment && removesEquipment) {
				unit.removeCharm (unit.Charms [i]);
				//unit.aiSimHasBeenCursedCount++;
			}
		}

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
