using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_WeaponBonus : Charm {

	public int damageMod;
	public int rangeMod;


	public Charm_WeaponBonus(XmlNode _node){
		node = _node;

		damageMod = 0;
		rangeMod = 0;
		expiresAfterAttack = false;

		if (node ["damage_mod"] != null) {
			damageMod = int.Parse (node ["damage_mod"].InnerXml);
		}
		if (node ["range_mod"] != null) {
			rangeMod = int.Parse (node ["range_mod"].InnerXml);
		}



		description = "";
		if (rangeMod != 0) {
			description += "Weapon range increase: " + rangeMod;
		}
		if (damageMod != 0) {
			description += "Damage increase: " + damageMod;
		}
	}
	public Charm_WeaponBonus(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		className = CharmClass.WeaponBonus;
	}

	public override void setFromParentCustom(Charm parent){
		damageMod = ((Charm_WeaponBonus)parent).damageMod;
		rangeMod = ((Charm_WeaponBonus)parent).rangeMod;
	}

	public override int getWeaponDamageMod(Card card, Unit target){
		return damageMod;
	}
	public override int getWeaponRangeMod(Card card){
		return rangeMod;
	}
}
