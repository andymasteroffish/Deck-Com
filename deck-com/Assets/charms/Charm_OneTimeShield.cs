using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_OneTimeShield : Charm {

	public int damagePrevented;

	public Charm_OneTimeShield(XmlNode node, Unit _owner){
		damagePrevented = int.Parse (node ["damagePrevented"].InnerXml);
		setup (node, _owner);
	}

	public override int getDamageTakenMod(Card card, Unit source){

		return -damagePrevented;
	}

	public override void takeDamage(Card card, Unit source){
		Owner.removeCharm (this);
	}
}
