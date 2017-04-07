using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_HandSize : Charm {

	private int handSizeMod;

	public Charm_HandSize(XmlNode _node){
		node = _node;
		handSizeMod = int.Parse (node ["hand_size_mod"].InnerXml);
	}

	public override int getHandSizeMod(){
		return handSizeMod;
	}
}
