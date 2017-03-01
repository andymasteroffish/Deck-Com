using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_ExtraCard : Charm {

	private int numExtraCards;

	public Charm_ExtraCard(XmlNode node, Unit _owner){
		numExtraCards = int.Parse (node ["extra_cards"].InnerXml);
		setup (node, _owner);
	}

	public override void resetRoundCustom(){
		for (int i = 0; i < numExtraCards; i++) {
			Owner.deck.drawCard ();
		}
	}
}
