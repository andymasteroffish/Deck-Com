using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class DBManager {
	

	public List<DBDeck> decks = new List<DBDeck> ();

	public DBDeck activeDeck;
	public DBDeck unusedCardsDeck;

	private List<Charm> unusedCharms = new List<Charm>();

	public bool unusedCardsOpen;

	public int money;

	private string xmlPath;
	private string unusedCharmListPath;

	private Charm.CharmType charmReplaceType;
	public bool unusedCharmsOpen;

	public DBManager(){

		activeDeck = null;

		unusedCardsOpen = false;
		unusedCharmsOpen = false;

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
		unusedCardsDeck = new DBDeck(Application.dataPath + "/external_data/player/unused_cards.txt", decks.Count);
		decks.Add (unusedCardsDeck);
		DBManagerInterface.instance.getDeckButtonGO().activate(unusedCardsDeck);

		//gather up all of the unused charms
		unusedCharmListPath = Application.dataPath + "/external_data/player/unused_charms.txt";
		string charmText = File.ReadAllText (unusedCharmListPath);
		string[] charmLines = charmText.Split ('\n');

		for (int i = 0; i < charmLines.Length; i++) {
			if (charmLines [i].Length > 2) {
				Charm thisCharm = CharmManager.instance.getCharmFromIdName (charmLines [i]);
				thisCharm.setup (null, false, charmLines [i]);
				unusedCharms.Add (thisCharm);
			}
		}

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
		unusedCharmsOpen = false;
	}

	//opens the list of unused charms
	public void openUnusedCharms(Charm.CharmType replaceType){
		charmReplaceType = replaceType;
		unusedCharmsOpen = true;
		unusedCardsOpen = false;

		int order = 0;
		for (int i = 0; i < unusedCharms.Count; i++) {
			if (unusedCharms [i].type == replaceType) {
				DBManagerInterface.instance.getUnusedCharmGO ().activate (unusedCharms [i], order);
				order++;
			}
		}
	}

	public void replaceCharm(Charm newCharm){
		if (newCharm.type == Charm.CharmType.Weapon) {
			activeDeck.weaponToAdd = newCharm;
		} else {
			activeDeck.charmToAdd = newCharm;
		}
		unusedCharmsOpen = false;
	}

	//steps back one
	public void cancel(){
		if (unusedCardsOpen || unusedCharmsOpen) {
			unusedCardsOpen = false;
			unusedCharmsOpen = false;
		} else {
			if (activeDeck != null) {
				activeDeck.setAsInactive ();
			}
			activeDeck = null;
		}
	}

	//saves changes to active deck and goes back
	public void saveChanges(){
		money -= activeDeck.getCurrentSaveCost ();

		unusedCardsDeck.removeCards (activeDeck.cardsToAdd);

		if (activeDeck.weaponToAdd != null) {
			unusedCharms.Remove (activeDeck.weaponToAdd);
		}
		if (activeDeck.charmToAdd != null) {
			unusedCharms.Remove (activeDeck.charmToAdd);
		}

		activeDeck.saveChanges ();
		activeDeck.setAsInactive ();
		activeDeck = null;

		//go ahead and create the xml for all of the player's info
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

		//and update the unsued charm file
		string[] charmLines = new string[unusedCharms.Count];
		for (int i = 0; i < unusedCharms.Count; i++) {
			charmLines [i] = unusedCharms [i].idName;
		}

		File.WriteAllLines(unusedCharmListPath, charmLines);

	}
}
