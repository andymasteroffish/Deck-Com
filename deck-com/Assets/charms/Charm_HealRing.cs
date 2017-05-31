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
	public Charm_HealRing(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = Charm.CharmType.Equipment;
		className = CharmClass.HealRing;
		aiGoodCharmPoints = 1;
	}

	public override void setFromParentCustom(Charm parent){
		usedThisRound = ((Charm_HealRing)parent).usedThisRound;
		healIncrease = ((Charm_HealRing)parent).healIncrease;
	}

	public override void resetRoundCustom(){
		usedThisRound = false;
	}

	public override void cardPlayedCustom(Card card){
		if (!usedThisRound && card.type == Card.CardType.Aid) {
			usedThisRound = true;
			Owner.deck.drawCard ();
		}
	}

	public override int getHealModCustom(Card card, Unit target){
		return healIncrease;
	}
}
