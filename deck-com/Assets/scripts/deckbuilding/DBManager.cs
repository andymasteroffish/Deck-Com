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

	public bool unusedCardsOpen;

	public DBManager(TextAsset unitList, TextAsset unusedCardsList){

		activeDeck = null;

		unusedCardsOpen = false;

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

	public void addUnusedCardToDeck(Card cardToAdd){
		activeDeck.addCard (cardToAdd);
		unusedCardsOpen = false;
	}

	//selects a deck for viewing/editting
	public void setDeckActive(DBDeck deck){
		activeDeck = deck;

		activeDeck.setAsActive ();
	}

	//opens the list of unused cards on top of the active deck
	public void openUnusedCards(){
		unusedCardsDeck.setAsUnusedActive ();
		unusedCardsOpen = true;
	}

	//steps back one
	public void cancel(){
		if (unusedCardsOpen) {
			unusedCardsOpen = false;
		} else {
			activeDeck = null;
		}
	}

	//saves changes to active deck and goes back
	public void saveChanges(){
		unusedCardsDeck.RemoveCards (activeDeck.cardsToAdd);
		activeDeck.saveChanges ();
		activeDeck = null;
	}
}
