using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHealRing : Item {

	bool usedThisRound;
	public int healIncrease;

	public override void resetRoundCustom(){
		usedThisRound = false;
	}

	public override void cardPlayed(Card card){
		if (!usedThisRound && card.type == Card.CardType.Aid) {
			usedThisRound = true;
			Owner.deck.drawCard ();
		}
	}

	public override int getHealMod(Card card){
		return healIncrease;
	}
}
