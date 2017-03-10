using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

public class DBDeck {

	public string idName;
	public string displayName;
	public Sprite sprite;

	public List<Card> cards;
	public List<Card> cardsToAdd = new List<Card>();

	public List<Charm> charms = new List<Charm>();

	public int order;

	private string deckListPath;

	//saving info
	string spriteName;
	string deckListShortName;

	public DBDeck(XmlNode node, int _order){
		//general stuff
		idName = node.Attributes ["idName"].Value;
		displayName = node ["name"].InnerXml;
		spriteName = node ["sprite"].InnerXml;
		sprite = Resources.Load<Sprite> (spriteName);
		order = _order;

		//deck
		deckListShortName = node ["deck"].InnerXml;
		deckListPath = Application.dataPath + "/external_data/player/decks/"+deckListShortName+".txt";
		cards = CardManager.instance.getDeckFromTextFile (deckListPath);
		for (int i = 0; i < cards.Count; i++) {
			cards [i].setup (null, null);
		}

		//charms
		XmlNodeList childNodes = node["charms"].ChildNodes;
		foreach (XmlNode n in childNodes) {
			if (n.Name == "charm") {
				string charmID = n.InnerXml;
				Charm thisCharm = CharmManager.instance.getCharmFromIdName (charmID);
				thisCharm.setup (null, false, charmID);
				charms.Add(thisCharm);
			}
		}
	}

	public DBDeck(string unusedCardFilePath, int _order){
		idName = "unused";
		displayName = "Unused Cards";
		sprite = Resources.Load<Sprite> ("unused_cards_icon");
		order = _order;

		//deck
		deckListPath = unusedCardFilePath;
		cards = CardManager.instance.getDeckFromTextFile (deckListPath);
		for (int i = 0; i < cards.Count; i++) {
			cards [i].setup (null, null);
		}

		//no charms
	}

	public void setAsActive(){
		for (int i = 0; i < cards.Count; i++) {
			DBManagerInterface.instance.getCardGO ().activate (cards[i], i, false);
		}
		cardsToAdd.Clear ();
	}

	public void setAsUnusedActive(){
		int orderCount = 0;
		for (int i = 0; i < cards.Count; i++) {
			//first check that this card is not already in the added list
			bool canAdd = true;
			foreach (Card card in DBManagerInterface.instance.manager.activeDeck.cardsToAdd) {
				if (card == cards [i]) {
					canAdd = false;
				}
			}
			if (canAdd) {
				DBManagerInterface.instance.getCardGO ().activate (cards [i], orderCount, true);
				orderCount++;
			}
		}
	}

	public void addCard(Card card){
		int order = cards.Count + cardsToAdd.Count;
		cardsToAdd.Add (card);
		card.setup (null, null);
		if (DBManagerInterface.instance.manager.activeDeck == this) {
			DBCardGO GO = DBManagerInterface.instance.getCardGO ();
			GO.activate (card, order, false);
			GO.setSpriteColor (Color.cyan);
		}
	}

	public void removeCards(List<Card> cardsToRemove){
		for (int i = 0; i < cardsToRemove.Count; i++) {
			cards.Remove (cardsToRemove [i]);
		}
	}

	public void removeCardToAdd(Card card){
		cardsToAdd.Remove (card);
	}

	public void saveChanges(){
		//add everything in the to-add list to the deck and then clear it
		for (int i = 0; i < cardsToAdd.Count; i++) {
			cards.Add (cardsToAdd [i]);
		}
		cardsToAdd.Clear ();
	}

	public int getCurrentSaveCost(){
		int returnVal = 0;
		for (int i = 0; i < cardsToAdd.Count; i++) {
			returnVal += cardsToAdd [i].CostToAddToDeck;
		}
		return returnVal;
	}

	public string getXML(){
		string xmlText = "";
		xmlText += "<unit idName = '" + idName + "'>\n";

		xmlText += "<name>" + displayName + "</name>\n";
		xmlText += "<sprite>" + spriteName + "</sprite>\n";
		xmlText += "<deck>" + deckListShortName + "</deck>\n";
		xmlText += "<player_controlled>true</player_controlled>\n";

		xmlText += "<charms>\n";
		for (int i = 0; i < charms.Count; i++) {
			xmlText += "<charm>" + charms [i].idName + "</charm>\n";
		}
		xmlText += "</charms>\n";

		xmlText += "</unit>";

		return xmlText;
	}

	public void saveDeckFile(){
		string[] lines = new string[cards.Count];
		for (int i = 0; i < cards.Count; i++) {
			lines [i] = cards [i].idName;
		}

		File.WriteAllLines(deckListPath, lines);
	}

}
