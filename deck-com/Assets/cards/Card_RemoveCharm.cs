using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_RemoveCharm : Card {

	//public bool removesEquipment;

	public List<Charm.CharmType> typesToRemove;

	public Card_RemoveCharm(){}
	public Card_RemoveCharm(XmlNode _node){
		node = _node;

		//removesEquipment = bool.Parse (node ["hits_equipment"].InnerXml);

		typesToRemove = new List<Charm.CharmType> ();

		for (int i = 0; i < node.ChildNodes.Count; i++) {
			if (node.ChildNodes [i].Name == "remove_type") {
				typesToRemove.Add( CharmTypeFromString(node.ChildNodes[i].InnerText));
				//Debug.Log (typesToRemove [(int)typesToRemove.Count - 1]);
			}
		}

	

	}

	public override void setupBlueprintCustom(){
		
	}

	public override void setupCustom(){
		Card_RemoveCharm blueprintCustom = (Card_RemoveCharm)blueprint;

		typesToRemove = new List<Charm.CharmType> ();
		for (int i = 0; i < blueprintCustom.typesToRemove.Count; i++) {
			typesToRemove.Add( blueprintCustom.typesToRemove[i]);
		}
		//removesEquipment = blueprintCustom.removesEquipment;
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
			if (typesToRemove.Contains(unit.Charms [i].type)) {
				unit.removeCharm (unit.Charms [i]);
			}
		}

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		Unit targetUnit = Owner.board.getUnitOnTile (move.targetTilePos);
		passInUnitCustom (targetUnit);
	}
}
