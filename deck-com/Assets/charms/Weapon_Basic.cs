using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_Basic : Charm {

	public string charmToGive;

	public int attackCardActionCostMod;

	public Weapon_Basic(XmlNode _node){
		
		node = _node;
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);

		charmToGive = "none";
		if (node ["charm_to_give"] != null) {
			charmToGive = node ["charm_to_give"].InnerText;
		}

		attackCardActionCostMod = 0;
		if (node ["attack_card_cost_mod"] != null) {
			attackCardActionCostMod = int.Parse(node ["attack_card_cost_mod"].InnerText);
		}
	}
	public Weapon_Basic(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		className = CharmClass.BasicWeapon;
		description = baseDamage + " damage, " + baseRange + " range.";
	}

	public override void setFromParentCustom(Charm parent){
		charmToGive = ((Weapon_Basic)parent).charmToGive;
	}

	public override void dealWeaponDamage(Unit target, int damage){
		if (charmToGive != "none") {
			//target.aiSimHasBeenCursedCount++;
			target.addCharm (charmToGive);
		}
	}

	public override int getCardActionCostMod(Card card){
		if (card.type == Card.CardType.Attack) {
			return attackCardActionCostMod;
		}
		return 0;
	}

}
