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

	public override int getCardActionCostMod(Card card){
		if (card.type == Card.CardType.Attack) {
			
			return 1;
		}
		return 0;
	}
}
