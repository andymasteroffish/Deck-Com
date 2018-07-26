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

	public Charm curCharm = null;
	public Charm weaponToAdd, charmToAdd = null;

	//public List<Charm> charms = new List<Charm>();
	//public List<Charm> charmsToAdd = new List<Charm>();

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
				if (charmID.Length > 1) {
					Charm thisCharm = CharmManager.instance.getCharmFromIdName (charmID);
					thisCharm.setup (null, false, charmID);
					curCharm = thisCharm;
				}
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

		cards.Sort ();

	}

	public void setAsActive(){
		for (int i = 0; i < cards.Count; i++) {
			DBManagerInterface.instance.getCardGO ().activate (cards[i], i, false);
		}
		cardsToAdd.Clear ();

		weaponToAdd = null;
		charmToAdd = null;
	}

	public void setAsInactive(){
		cardsToAdd.Clear ();

		weaponToAdd = null;
		charmToAdd = null;
	}

	public void setAsUnusedActive(int startingIndex){
		int curIndex = 0;
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
				curIndex++;
				if (curIndex >= startingIndex) {
					DBManagerInterface.instance.getCardGO ().activate (cards [i], orderCount, true);
					orderCount++;
					if (orderCount >= DBManagerInterface.instance.maxUnusedCardsShownAtOnce) {
						return;
					}
				}
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
		//check if charms have been changed
		if (charmToAdd != null) {
			curCharm = charmToAdd;
		}
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

		if (weaponToAdd != null) {
			returnVal += weaponToAdd.costToAddToDeck;
		}
		if (charmToAdd != null) {
			returnVal += charmToAdd.costToAddToDeck;
		}

		return returnVal;
	}

	public string getXML(){
		string xmlText = "";
		xmlText += "<unit idName = '" + idName + "'>\n";

		xmlText += "<name>" + displayName + "</name>\n";
		xmlText += "<sprite>" + spriteName + "</sprite>\n";
		xmlText += "<deck>" + deckListShortName + "</deck>\n";
		xmlText += "<hand_size>5</hand_size>\n";
		xmlText += "<player_controlled>true</player_controlled>\n";
		xmlText += "<currently_active>true</currently_active>\n";

		xmlText += "<charms>\n";
		//xmlText += "<charm>" + curWeapon.idName + "</charm>\n";
		if (curCharm != null) {
			xmlText += "<charm>" + curCharm.idName + "</charm>\n";
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
