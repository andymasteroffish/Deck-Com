using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_Loot : Card {

	public Loot.Type lootType;
	public int level;

	public Card_Loot(XmlNode _node){
		node = _node;

		baseActionCost = 1000;
	}

	public void lootSetup(Loot.Type _type, int _level){
		lootType = _type;
		level = _level;
	}


	public override void setupCustom(){
		type = Card.CardType.Loot;

		description = "Loot! This card cannot be played but turns into treausure at the end of the encounter\n";
		description += "Level: " + level;
	}
}
