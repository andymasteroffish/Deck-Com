using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_HealRing : Charm {

	bool usedThisRound;
	public int healIncrease;


	public Charm_HealRing(XmlNode _node){
		node = _node;
	}
	public override void resetRoundCustom(){
		usedThisRound = false;
	}

	public override void cardPlayed(Card card){
		if (!usedThisRound && card.type == Card.CardType.Aid) {
			usedThisRound = true;
			Owner.deck.drawCard ();
		}
	}

	public override int getHealMod(Card card, Unit target){
		return healIncrease;
	}
}
