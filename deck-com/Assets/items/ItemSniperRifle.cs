using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSniperRifle : Item {

	public override int getCardActionCostMod(Card card){
		if (card.type == Card.CardType.Attack) {
			
			return 1;
		}
		return 0;
	}
}
