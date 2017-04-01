using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_Basic : Charm {

	public Weapon_Basic(XmlNode _node){
		
		node = _node;
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		description = baseDamage + " damage, " + baseRange + " range.";
	}

}
