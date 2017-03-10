using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class DBManager {
	

	public List<DBDeck> decks = new List<DBDeck> ();

	public DBDeck activeDeck;
	public DBDeck unusedCardsDeck;

	public bool unusedCardsOpen;

	public int money;

	private string xmlPath;

	public DBManager(){

		activeDeck = null;

		unusedCardsOpen = false;


		//grabbing the info for the player
		xmlPath = Application.dataPath + "/external_data/player/player_info.xml";
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		money = int.Parse(infoNode["money"].InnerXml);

		//go through each unit and make a deck if it is player controlled
		XmlNodeList unitNodes = xmlDoc.GetElementsByTagName ("unit");
		foreach (XmlNode node in unitNodes) {
			DBDeck deck = new DBDeck (node, decks.Count);
			decks.Add (deck);

			//and make a button for it
			DBManagerInterface.instance.getDeckButtonGO().activate(deck);
		}

		//make a special deck of unused cards
		string unusedCardsListFile = Application.dataPath + "/external_data/player/unused_cards.txt";
		unusedCardsDeck = new DBDeck(unusedCardsListFile, decks.Count);
		decks.Add (unusedCardsDeck);
		DBManagerInterface.instance.getDeckButtonGO().activate(unusedCardsDeck);


		//TESTING KILL ME
		//ON MAC, THIS WILL GO ON THE SAME LEVEL AS DATA, FRAMEWOKRS, RESOURCES ETC
//		Debug.Log (Application.dataPath);
//		XmlDocument testDoc = new XmlDocument();
//		testDoc.Load (Application.dataPath + "/test_load/thisisatest.xml");
//		Debug.Log (testDoc.InnerXml);
//
//		//XmlNode testNode = new XmlNode ();
//		testDoc.InnerXml = "<testing>wut up</testing>";
//
//		testDoc.Save(Application.dataPath + "/test_load/thisisatest.xml");
			

	}

	public void addUnusedCardToDeck(Card cardToAdd){
		activeDeck.addCard (cardToAdd);
		unusedCardsOpen = false;
	}

	public void deckCardClicked(Card card){
		if (activeDeck != unusedCardsDeck && activeDeck.cardsToAdd.Contains (card)) {
			activeDeck.removeCardToAdd (card);
			unusedCardsDeck.addCard (card);
		}
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
		money -= activeDeck.getCurrentSaveCost ();
		unusedCardsDeck.removeCards (activeDeck.cardsToAdd);
		activeDeck.saveChanges ();
		activeDeck = null;

		//go ahead and create the xml
		string xmlText = "";
		xmlText += "<player>\n";

		xmlText += "<info>\n";
		xmlText += "<money>" + money + "</money>\n";
		xmlText += "</info>\n";

		for (int i = 0; i < decks.Count; i++) {
			if (decks [i] != unusedCardsDeck) {
				xmlText += decks [i].getXML ();
			}
		}

		xmlText += "</player>";

		XmlDocument saveDoc = new XmlDocument ();
		saveDoc.InnerXml = xmlText;
		saveDoc.Save(xmlPath);
	
		//and have each deck update their text file
		for (int i = 0; i < decks.Count; i++) {
			decks [i].saveDeckFile ();
		}
	}
}
