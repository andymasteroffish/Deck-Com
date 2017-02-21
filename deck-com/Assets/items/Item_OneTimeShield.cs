using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_OneTimeShield : Item {

	public int damagePrevented;

	public override int getDamageTakenMod(Card card, Unit source){

		return -damagePrevented;
	}

	public override void takeDamage(Card card, Unit source){
		Owner.removeCharm (this);
	}
}
