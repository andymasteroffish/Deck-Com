using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_OneTimeShield : Charm {

	public int damagePrevented;

	public Charm_OneTimeShield(XmlNode _node){
		node = _node;
		damagePrevented = int.Parse (node ["damagePrevented"].InnerXml);
	}
	public Charm_OneTimeShield(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		className = CharmClass.OneTimeShield;
	}

	public override void setFromParentCustom(Charm parent){
		damagePrevented = ((Charm_OneTimeShield)parent).damagePrevented;
	}

	public override int getDamageTakenMod(Card card, Unit source){

		return -damagePrevented;
	}

	public override void takeDamage(Card card, Unit source){
		Owner.removeCharm (this);
	}
}
