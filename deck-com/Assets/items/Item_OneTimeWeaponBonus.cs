using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item_OneTimeWeaponBonus : Item {

	public int damageMod;

	public override int getWeaponDamageMod(Card card){
		Debug.Log ("do ya thing for " + damageMod);
		Owner.removeCharm (this);
		return damageMod;
	}
}
