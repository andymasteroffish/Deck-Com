using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBDeck {

	public string idName;
	public string displayName;
	public Sprite sprite;

	public List<Card> cards;
	public List<Card> cardsToAdd = new List<Card>();
	public List<Charm> charms = new List<Charm>();

	public int order;

	public DBDeck(XmlNode node, int _order){
		//general stuff
		idName = node.Attributes ["idName"].Value;
		displayName = node ["name"].InnerXml;
		sprite = Resources.Load<Sprite> (node ["sprite"].InnerXml);
		order = _order;

		//deck
		TextAsset deckList = Resources.Load<TextAsset> (node ["deck"].InnerXml);
		cards = CardManager.instance.getDeckFromTextFile (deckList);
		for (int i = 0; i < cards.Count; i++) {
			cards [i].setup (null, null);
		}

		//charms
		XmlNodeList childNodes = node["charms"].ChildNodes;
		foreach (XmlNode n in childNodes) {
			if (n.Name == "charm") {
				Charm thisCharm = CharmManager.instance.getCharmFromIdName (n.InnerXml);
				thisCharm.setup (null, false);
				charms.Add(thisCharm);
			}
		}
	}

	public DBDeck(TextAsset unusedCardsList, int _order){
		idName = "unused";
		displayName = "Unused Cards";
		sprite = Resources.Load<Sprite> ("unused_cards_icon");
		order = _order;

		//deck
		cards = CardManager.instance.getDeckFromTextFile (unusedCardsList);
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
		DBCardGO GO = DBManagerInterface.instance.getCardGO ();
		GO.activate (card, order, false);
		GO.setSpriteColor (Color.cyan);
	}

	public void RemoveCards(List<Card> cardsToRemove){
		for (int i = 0; i < cardsToRemove.Count; i++) {
			cards.Remove (cardsToRemove [i]);
		}
	}

	public void saveChanges(){
		//add everything in the to-add list to the deck and then clear it
		for (int i = 0; i < cardsToAdd.Count; i++) {
			cards.Add (cardsToAdd [i]);
		}
		cardsToAdd.Clear ();
	}


	public void saveXML(){

	}

}
