using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_OneTimeWeaponBonus : Item {

	public int damageMod;
	public int rangeMod;

	public bool expiresAtEndOfTurn;

	public override int getWeaponDamageMod(Card card, Unit target){
		return damageMod;
	}

	public override int getWeaponRangeMod(Card card){
		return rangeMod;
	}

	public override void cardPlayed(Card card){
		if (card.type == Card.CardType.Attack) {
			Owner.removeCharm (this);
		}
	}

	public override void turnEndPreDiscard(){
		if (expiresAtEndOfTurn) {
			Owner.removeCharm (this);
		}
	}
}
