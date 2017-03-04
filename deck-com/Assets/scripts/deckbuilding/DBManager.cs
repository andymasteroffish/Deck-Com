using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBManager {


	private XmlDocument fullXML;
	private XmlNodeList nodes;

	public List<DBDeck> decks = new List<DBDeck> ();

	public DBDeck activeDeck;

	public DBManager(TextAsset unitList){

		activeDeck = null;

		//build the xml
		fullXML = new XmlDocument ();
		fullXML.LoadXml (unitList.text);
		nodes = fullXML.GetElementsByTagName ("unit");

		//go through each unit and make a deck if it is player controlled
		int i=0;
		foreach (XmlNode node in nodes) {
			if (bool.Parse (node ["player_controlled"].InnerXml)) {
				DBDeck deck = new DBDeck (node, i);
				decks.Add (deck);

				//and make a button for it
				DBManagerInterface.instance.getDeckButtonGO().activate(deck);

				i++;
			}

		}

	}

	public void click(DBDeck deck){
		Debug.Log("ya clicked "+deck.displayName);
		setDeckActive (deck);
	}

	public void setDeckActive(DBDeck deck){
		activeDeck = deck;

		activeDeck.setAsActive ();
	}
}
