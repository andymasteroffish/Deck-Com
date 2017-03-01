using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_OneTimeWeaponBonus : Charm {

	public int damageMod;
	public int rangeMod;

	public bool expiresAtEndOfTurn;

	public Charm_OneTimeWeaponBonus(XmlNode node, Unit _owner){
		damageMod = int.Parse (node ["damageMod"].InnerXml);
		rangeMod = int.Parse (node ["rangeMod"].InnerXml);
		expiresAtEndOfTurn = bool.Parse(node ["expires_at_turn_end"].InnerXml);
		setup (node, _owner);
	}

	public override void setupCustom(XmlNode node){
		
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

	public override void turnEndPreDiscard(){
		if (expiresAtEndOfTurn) {
			Owner.removeCharm (this);
		}
	}
}
