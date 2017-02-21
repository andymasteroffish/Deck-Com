using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_OneTimeWeaponBonus : Item {

	public int damageMod;

	public override int getWeaponDamageMod(Card card){
		Debug.Log ("do ya thing for " + damageMod);

		return damageMod;
	}

	public override void cardPlayed(Card card){
		if (card.type == Card.CardType.Attack) {
			Debug.Log ("buh bye "+name);
			Owner.removeCharm (this);
		}
	}
}
