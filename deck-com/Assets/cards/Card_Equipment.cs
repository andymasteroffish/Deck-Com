using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_Equipment : Card {

	public string idNameOfEquipment;


	public Card_Equipment(){}
	public Card_Equipment(XmlNode _node){
		node = _node;

		idNameOfEquipment = node ["equipment_id"].InnerXml;

	}

	// Use this for initialization
	public override void setupBlueprintCustom(){
		type = CardType.Equipment;

		description = "Equip " + idNameOfEquipment;
	}

	public override void setupCustom(){
		Card_Equipment blueprintCustom = (Card_Equipment)blueprint;
		idNameOfEquipment = blueprintCustom.idNameOfEquipment;
	}

	public override void mouseEnterEffects(){
		Owner.CurTile.setHighlighted (true, baseHighlightColor);
	}
		
	public override void selectCardCustom(){
		WaitingForUnit = true;
		//Owner.board.clearHighlights ();
		//Owner.setHighlighted (true, baseHighlightColor);

		//just do the thing
		passInUnitCustom (Owner);
	}

	public override void passInUnitCustom(Unit unit){
		Owner.board.clearHighlights ();
		Charm newEquipment = unit.addCharm (idNameOfEquipment);
		newEquipment.storeCard (this);
		//unit.aiSimHasBeenAidedCount++;
		finish (true);
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
