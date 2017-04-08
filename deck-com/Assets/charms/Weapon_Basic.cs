using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_Basic : Charm {

	public string charmToGive;

	public Weapon_Basic(XmlNode _node){
		
		node = _node;
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);

		charmToGive = "none";
		if (node ["charm_to_give"] != null) {
			charmToGive = node ["charm_to_give"].InnerText;
		}
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		description = baseDamage + " damage, " + baseRange + " range.";
	}

	public override void dealWeaponDamage(Unit target, int damage){
		if (charmToGive != "none") {
			target.aiSimHasBeenCursedCount++;
			target.addCharm (charmToGive);
		}
	}

}
