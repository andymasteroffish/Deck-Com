﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class CharmManager : MonoBehaviour {

	public static CharmManager instance = null;	

	private XmlNodeList nodes;

	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		XmlDocument fullXML = new XmlDocument ();
		fullXML.Load (Application.dataPath + "/external_data/charms.xml");
		nodes = fullXML.GetElementsByTagName ("charm");
	}


	public Charm getCharmFromIdName(string charmIdName){
		foreach (XmlNode node in nodes) {
			if (node.Attributes ["idName"].Value == charmIdName) {
				return	getCharmFromXMLNode (node);
			}
		}
		Debug.Log ("COULD NOT FIND CHARM ID: " + charmIdName);
		return null;
	}

	public Charm getCharmFromXMLNode(XmlNode node){
		Charm thisCharm = null;

		string scriptName = node ["script"].InnerText;// node.Attributes ["script"].Value;
		if (scriptName == "Charm") {
			thisCharm = new Charm (node);
		}
		else if (scriptName == "Weapon_Basic") {
			thisCharm = new Weapon_Basic (node);
		}
		else if (scriptName == "Charm_ExtraCard") {
			thisCharm = new Charm_ExtraCard (node);
		}
		else if (scriptName == "Charm_HealRing") {
			thisCharm = new Charm_HealRing (node);
		}
		else if (scriptName == "Charm_OneTimeWeaponBonus") {
			thisCharm = new Charm_OneTimeWeaponBonus (node);
		}
		else if (scriptName == "Charm_WeaponBonus") {
			thisCharm = new Charm_WeaponBonus (node);
		}
		else if (scriptName == "Charm_OneTimeShield") {
			thisCharm = new Charm_OneTimeShield (node);
		}
		else{
			Debug.Log ("SCRIPT NAME FOR CHARM NOT FOUND: "+scriptName);
		}
		return thisCharm;
	}

	public Charm getCharmFromParent(Charm parent){
		Charm thisCharm = null;

		if (parent.className == Charm.CharmClass.Charm) {
			thisCharm = new Charm (parent);
		}
		else if (parent.className == Charm.CharmClass.BasicWeapon) {
			thisCharm = new Weapon_Basic (parent);
		}
		else if (parent.className == Charm.CharmClass.ExtraCard) {
			thisCharm = new Charm_ExtraCard (parent);
		}
		else if (parent.className == Charm.CharmClass.HealRing) {
			thisCharm = new Charm_HealRing (parent);
		}
		else if (parent.className == Charm.CharmClass.OneTimeWeaponBonus) {
			thisCharm = new Charm_OneTimeWeaponBonus (parent);
		}
		else if (parent.className == Charm.CharmClass.WeaponBonus) {
			thisCharm = new Charm_WeaponBonus (parent);
		}
		else if (parent.className == Charm.CharmClass.OneTimeShield) {
			thisCharm = new Charm_OneTimeShield (parent);
		}
		else{
			Debug.Log ("SOMETHING WENT WRONG MAKING: "+parent.className);
		}
		return thisCharm;
	}
}
