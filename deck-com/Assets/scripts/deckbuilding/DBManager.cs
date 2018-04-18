using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using System.IO;

public class DBManager {
	

	public List<DBDeck> decks = new List<DBDeck> ();

	public DBDeck activeDeck;
	public DBDeck unusedCardsDeck;

	private List<Charm> unusedCharms = new List<Charm>();

	public bool unusedCardsOpen;
	public int curUnusedCardWindowPage = 0;
	public int maxUnusedCardWindowPage = 0;

	public int money;
	public int curLevel;

	private string xmlPath;
	private string unusedWeaponsListPath;

	//private Charm.CharmType charmReplaceType;
	public bool unusedWeaponsOpen;

	public DBManager(){

		activeDeck = null;

		unusedCardsOpen = false;
		unusedWeaponsOpen = false;

		//grabbing the info for the player
		xmlPath = Application.dataPath + "/external_data/player/player_info.xml";
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		money = int.Parse(infoNode["money"].InnerXml);

		curLevel = int.Parse (infoNode ["cur_level"].InnerXml);

		//go through each unit and make a deck if it is player controlled
		XmlNodeList unitNodes = xmlDoc.GetElementsByTagName ("unit");
		foreach (XmlNode node in unitNodes) {
			if (bool.Parse (node ["currently_active"].InnerXml)) {
				DBDeck deck = new DBDeck (node, decks.Count);
				decks.Add (deck);

				//and make a button for it
				DBManagerInterface.instance.getDeckButtonGO ().activate (deck);
			}
		}

		//make a special deck of unused cards
		unusedCardsDeck = new DBDeck(Application.dataPath + "/external_data/player/unused_cards.txt", decks.Count);
		decks.Add (unusedCardsDeck);
		DBManagerInterface.instance.getDeckButtonGO().activate(unusedCardsDeck);

		//gather up all of the unused charms
		unusedWeaponsListPath = Application.dataPath + "/external_data/player/unused_weapons.txt";
		string charmText = File.ReadAllText (unusedWeaponsListPath);
		string[] charmLines = charmText.Split ('\n');

		for (int i = 0; i < charmLines.Length; i++) {
			if (charmLines [i].Length > 2) {
				Debug.Log ("load " + charmLines [i]);
				Charm thisCharm = CharmManager.instance.getCharmFromIdName (charmLines [i]);
				thisCharm.setup (null, false, charmLines [i]);
				unusedCharms.Add (thisCharm);
			}
		}	

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
		curUnusedCardWindowPage = 0;
		maxUnusedCardWindowPage = (int)Mathf.Ceil ((float)unusedCardsDeck.cards.Count / (float)DBManagerInterface.instance.maxUnusedCardsShownAtOnce) -1;
		unusedCardsDeck.setAsUnusedActive (0);
		unusedCardsOpen = true;
		unusedWeaponsOpen = false;
	}
	public void scrollUnusedCards(int dir){
		if (curUnusedCardWindowPage + dir < 0)							return;
		if (curUnusedCardWindowPage + dir > maxUnusedCardWindowPage)	return;

		curUnusedCardWindowPage += dir;

		unusedCardsDeck.setAsUnusedActive (curUnusedCardWindowPage * DBManagerInterface.instance.maxUnusedCardsShownAtOnce);
	}

	//opens the list of unused charms
	public void openUnusedWeapons(){
		//charmReplaceType = replaceType;
		unusedWeaponsOpen = true;
		unusedCardsOpen = false;

		Debug.Log ("open " + unusedCharms.Count + " charms");

		int order = 0;
		for (int i = 0; i < unusedCharms.Count; i++) {
			if (unusedCharms [i].type == Charm.CharmType.Weapon) {
				DBUnusedCharmGO thisGO = DBManagerInterface.instance.getUnusedCharmGO ();
				Debug.Log (thisGO);
				thisGO.activate (unusedCharms [i], order);
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
		unusedWeaponsOpen = false;
	}

	//steps back one
	public void cancel(){
		if (unusedCardsOpen || unusedWeaponsOpen) {
			unusedCardsOpen = false;
			unusedWeaponsOpen = false;
		} else {
			if (activeDeck != null) {
				activeDeck.setAsInactive ();
			}
			activeDeck = null;
		}
	}

	//saves changes to active deck and goes back
	public void saveChanges(){
		unusedCardsOpen = false;
		unusedWeaponsOpen = false;

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
		xmlText += "<cur_level>" + curLevel + "</cur_level>\n";
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

		File.WriteAllLines(unusedWeaponsListPath, charmLines);

	}
}
