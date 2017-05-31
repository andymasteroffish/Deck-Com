using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_Web : Charm {
	

	public Charm_Web(XmlNode _node){
		node = _node;
	}
	public Charm_Web(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = Charm.CharmType.StatusEffect;
		className = CharmClass.Web;
	}


	public override int getDamageTakenModCustom(Card card, Unit source){

		return -9999;
	}

	public override void takeDamageCustom(Card card, Unit source){
		Owner.removeCharm (this);
	}

	public override int getCardActionCostModCustom(Card card){
		return 999;
	}
}
