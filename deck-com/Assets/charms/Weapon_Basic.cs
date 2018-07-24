using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

//KILL THIS CLASS ENTIRELY

public class Weapon_Basic : Charm {

	public string charmToGive;

	public int attackCardActionCostMod;

	public int cardDrawOnDamage;
	public int actionGainOnDamage;

	public Weapon_Basic(XmlNode _node){
		
//		node = _node;
//		baseDamage = int.Parse(node ["damage"].InnerText);
//		baseRange = int.Parse(node ["range"].InnerText);
//
//		charmToGive = "none";
//		if (node ["charm_to_give"] != null) {
//			charmToGive = node ["charm_to_give"].InnerText;
//		}
//
//		attackCardActionCostMod = 0;
//		if (node ["attack_card_cost_mod"] != null) {
//			attackCardActionCostMod = int.Parse(node ["attack_card_cost_mod"].InnerText);
//		}
//
//		cardDrawOnDamage = 0;
//		if (node ["card_draw_on_damage"] != null) {
//			cardDrawOnDamage = int.Parse(node ["card_draw_on_damage"].InnerText);
//		}
//		actionGainOnDamage = 0;
//		if (node ["action_gain_on_damage"] != null) {
//			actionGainOnDamage = int.Parse(node ["action_gain_on_damage"].InnerText);
//		}
	}
	public Weapon_Basic(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		className = CharmClass.BasicWeapon;
		//description = baseDamage + " damage, " + baseRange + " range.";
	}

	public override void setFromParentCustom(Charm parent){
		charmToGive = ((Weapon_Basic)parent).charmToGive;
		cardDrawOnDamage = ((Weapon_Basic)parent).cardDrawOnDamage;
		actionGainOnDamage = ((Weapon_Basic)parent).actionGainOnDamage;
	}

//	public override void dealWeaponDamageCustom(Card card, Unit target, int damage){
//		if (charmToGive != "none") {
//			//target.aiSimHasBeenCursedCount++;
//			target.addCharm (charmToGive);
//		}
//
//		if (cardDrawOnDamage > 0 && damage > 0) {
//			Owner.deck.drawCards(cardDrawOnDamage);
//		}
//		if (actionGainOnDamage > 0 && damage > 0) {
//			Owner.gainActions (actionGainOnDamage);
//		}
//	}

	public override int getCardActionCostModCustom(Card card){
		if (card.type == Card.CardType.Attack) {
			return attackCardActionCostMod;
		}
		return 0;
	}

}
