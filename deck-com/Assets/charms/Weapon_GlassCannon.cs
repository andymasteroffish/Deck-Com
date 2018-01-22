using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Weapon_GlassCannon : Charm {

	int startDamage;

	public Weapon_GlassCannon(XmlNode _node){
		
		node = _node;
		baseDamage = int.Parse(node ["damage"].InnerText);
		baseRange = int.Parse(node ["range"].InnerText);

		startDamage = baseDamage;

	}
	public Weapon_GlassCannon(Charm parent){
		setFromParent (parent);

		startDamage = baseDamage;
	}

	public override void setupCustom(){
		type = CharmType.Weapon;
		className = CharmClass.GlassCannon;

		startDamage = baseDamage;


		setText ();
	}

	public override void setFromParentCustom(Charm parent){
		
	}

	public override void takeDamageCustom (Card card, Unit source){
		Debug.Log ("FUCK. OW");
		if (baseDamage > 0) {
			baseDamage--;
		}

		setText ();

	}

	private void setText(){
		description =  baseDamage + " damage, " + baseRange + " range.\nDamage is reduced by 1 every time unit takes damage. Starts at "+startDamage.ToString();
	}

}
