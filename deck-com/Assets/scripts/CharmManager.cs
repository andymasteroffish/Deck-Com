using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class CharmManager : MonoBehaviour {

	public static CharmManager instance = null;	

	public TextAsset xmlText;

	private XmlDocument fullXML;
	private XmlNodeList nodes;

	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		fullXML = new XmlDocument ();
		fullXML.LoadXml (xmlText.text);
		nodes = fullXML.GetElementsByTagName ("charm");
	}

	void Start () {
		//testing
		//getCharmFromIdName("long bow");
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

		if (scriptName == "Weapon_Basic") {
			thisCharm = new Weapon_Basic (node);
		}
		else if (scriptName == "Weapon_SniperRifle") {
			thisCharm = new Weapon_SniperRifle (node);
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
		else if (scriptName == "Charm_OneTimeShield") {
			thisCharm = new Charm_OneTimeShield (node);
		}
		else{
			Debug.Log ("SCRIPT NAME FOR CHARM NOT FOUND: "+scriptName);
		}


		return thisCharm;
	}
}
