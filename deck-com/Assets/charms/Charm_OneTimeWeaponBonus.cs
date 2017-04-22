﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_OneTimeWeaponBonus : Charm {

	public int damageMod;
	public int rangeMod;

	//public bool expiresAtEndOfTurn;

	public Charm_OneTimeWeaponBonus(XmlNode _node){
		node = _node;
		damageMod = 0;
		//expiresAtEndOfTurn = false;

		if (node ["damage_mod"] != null) {
			damageMod = int.Parse (node ["damage_mod"].InnerXml);
		}
		if (node ["range_mod"] != null) {
			rangeMod = int.Parse (node ["range_mod"].InnerXml);
		}
//		if (node ["expires_at_turn_end"] != null) {
//			expiresAtEndOfTurn = bool.Parse (node ["expires_at_turn_end"].InnerXml);
//		}
	}
	public Charm_OneTimeWeaponBonus(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		className = CharmClass.OneTimeWeaponBonus;
	}

	public override void setFromParentCustom(Charm parent){
		damageMod = ((Charm_OneTimeWeaponBonus)parent).damageMod;
		rangeMod = ((Charm_OneTimeWeaponBonus)parent).rangeMod;
	}

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

//	public override void turnEndPreDiscardCustom(){
//		if (expiresAtEndOfTurn) {
//			Owner.removeCharm (this);
//		}
//	}
}
