using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Card_Loot : Card {
	
	public int level;

	public Card_Loot(){}
	public Card_Loot(XmlNode _node){
		node = _node;

		baseActionCost = 0;
	}

	public void lootSetup(int _level){
		level = _level;
	}

	public override bool checkIfCanBePlayedCustom(){
		return false;
	}


	public override void setupBlueprintCustom(){
		
		description = "Loot! This card cannot be played but turns into treausure at the end of the encounter\n";
		description += "Level: " + level;
	}
}
