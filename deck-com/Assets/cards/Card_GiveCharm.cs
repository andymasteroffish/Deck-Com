using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_GiveCharm : Card {

	public string idNameOfGift;
	public bool anyUnit;

	public string infoText;

	public Card_GiveCharm(XmlNode _node){
		node = _node;

		idNameOfGift = node ["gift_id"].InnerXml;
		anyUnit = bool.Parse (node ["any_unit"].InnerXml);
	}

	// Use this for initialization
	public override void setupCustom(){
		type = CardType.Other;
	}
	
	public override void selectCardCustom(){
		WaitingForUnit = true;
		if (anyUnit) {
			Owner.board.highlightAllUnits (true, true, aidHighlightColor);
		} else {
			Owner.setHighlighted (true, aidHighlightColor);
		}
	}

	public override void passInUnitCustom(Unit unit){
		unit.addCharm (idNameOfGift);

		finish ();
	}
}
