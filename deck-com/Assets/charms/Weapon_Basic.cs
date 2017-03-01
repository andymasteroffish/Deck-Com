using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_Basic : Charm {

	public Weapon_Basic(XmlNode node, Unit _owner){
		setup (node, _owner);
	}

	public override void setupCustom(XmlNode node){
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);
		description = baseDamage + " damage, " + baseRange + " range.";
	}

}
