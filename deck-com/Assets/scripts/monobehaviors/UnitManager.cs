using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class UnitManager : MonoBehaviour {

	public static UnitManager instance = null;

	//public TextAsset xmlText;
	//private XmlDocument fullXML;

	[System.NonSerialized]
	public XmlNodeList playerNodes, foeNodes;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}


		XmlDocument foeXML = new XmlDocument ();
		foeXML.Load(Application.dataPath + "/external_data/foes/units.xml");
		XmlDocument playerXML = new XmlDocument ();
		playerXML.Load(Application.dataPath + "/external_data/player/player_info.xml");

		foeNodes = foeXML.GetElementsByTagName ("unit");
		playerNodes = playerXML.GetElementsByTagName ("unit");
	}

	public Unit getUnitFromIdName(string idName){
		foreach (XmlNode node in foeNodes) {
			if (node.Attributes ["idName"].Value == idName) {
				return	getUnitFromNode (node);
			}
		}
		foreach (XmlNode node in playerNodes) {
			if (node.Attributes ["idName"].Value == idName) {
				return	getUnitFromNode (node);
			}
		}
		Debug.Log ("COULD NOT FIND UNIT ID: " + idName);
		return null;
	}

	public Unit getUnitFromNode(XmlNode node){
		Unit unit = new Unit (node);

		return unit;
	}
}
