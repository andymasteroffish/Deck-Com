using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_SpellCount : Charm {

	private int damage_per_spell = 2;
	
	public Weapon_SpellCount(XmlNode _node){
		
		node = _node;

		baseDamage = 0;
		baseRange = int.Parse(node ["range"].InnerText);


	}
	public Weapon_SpellCount(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		className = CharmClass.SpellCountWeapon;

	}

	public override void setFromParentCustom(Charm parent){
		
	}

	public override int getBaseDamage(){
		return Owner.deck.getCardsInHandOfType (Card.CardType.Spell) * damage_per_spell;
	}

}
