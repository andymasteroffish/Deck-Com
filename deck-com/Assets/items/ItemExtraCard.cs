using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemExtraCard : Item {

	public int numExtraCards;

	public override void resetRoundCustom(){
		for (int i = 0; i < numExtraCards; i++) {
			Owner.deck.drawCard ();
		}
	}
}
