using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_SniperRifle : Charm {

	public Weapon_SniperRifle(XmlNode _node){
		
		node = _node;
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);
	}
	public Weapon_SniperRifle(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		className = CharmClass.SniperRifle;
	}

	public override void setFromParentCustom(Charm parent){
		
	}

	public override int getCardActionCostMod(Card card){
		if (card.type == Card.CardType.Attack) {
			
			return 1;
		}
		return 0;
	}
}
