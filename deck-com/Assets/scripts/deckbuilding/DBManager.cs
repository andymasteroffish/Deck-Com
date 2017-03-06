using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBManager {


	private XmlDocument fullXML;
	private XmlNodeList nodes;

	public List<DBDeck> decks = new List<DBDeck> ();

	public DBDeck activeDeck;
	public DBDeck unusedCardsDeck;

	public DBManager(TextAsset unitList, TextAsset unusedCardsList){

		activeDeck = null;

		//build the xml
		fullXML = new XmlDocument ();
		fullXML.LoadXml (unitList.text);
		nodes = fullXML.GetElementsByTagName ("unit");

		//go through each unit and make a deck if it is player controlled
		foreach (XmlNode node in nodes) {
			if (bool.Parse (node ["player_controlled"].InnerXml)) {
				DBDeck deck = new DBDeck (node, decks.Count);
				decks.Add (deck);

				//and make a button for it
				DBManagerInterface.instance.getDeckButtonGO().activate(deck);
			}
		}

		//make a special deck of unused cards
		unusedCardsDeck = new DBDeck(unusedCardsList, decks.Count);
		decks.Add (unusedCardsDeck);
		DBManagerInterface.instance.getDeckButtonGO().activate(unusedCardsDeck);
			

	}


	public void setDeckActive(DBDeck deck){
		activeDeck = deck;

		activeDeck.setAsActive ();
	}

	public void cancel(){
		activeDeck = null;
	}
}
