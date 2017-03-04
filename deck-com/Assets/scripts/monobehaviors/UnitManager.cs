using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class UnitManager : MonoBehaviour {

	public static UnitManager instance = null;

	public TextAsset xmlText;

	private XmlDocument fullXML;
	private XmlNodeList nodes;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		fullXML = new XmlDocument ();
		fullXML.LoadXml (xmlText.text);
		nodes = fullXML.GetElementsByTagName ("unit");
	}

	public Unit getUnitFromIdName(string idName){
		foreach (XmlNode node in nodes) {
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
