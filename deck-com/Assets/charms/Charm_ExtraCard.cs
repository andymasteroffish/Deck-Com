using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_ExtraCard : Charm {

	private int numExtraCards;

	public Charm_ExtraCard(XmlNode _node){
		node = _node;
		numExtraCards = int.Parse (node ["extra_cards"].InnerXml);
	}
	public Charm_ExtraCard(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		className = CharmClass.ExtraCard;
	}

	public override void setFromParentCustom(Charm parent){
		numExtraCards = ((Charm_ExtraCard)parent).numExtraCards;
	}

	public override void resetRoundCustom(){
		for (int i = 0; i < numExtraCards; i++) {
			Owner.deck.drawCard ();
		}
	}
}
