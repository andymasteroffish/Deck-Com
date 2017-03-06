using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBDeck {

	public string idName;
	public string displayName;
	public Sprite sprite;

	public List<Card> cards;
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
				charms.Add(CharmManager.instance.getCharmFromIdName (n.InnerXml));
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
			DBManagerInterface.instance.getCardGO ().activate (cards[i], i);
		}
	}


	public void saveXML(){

	}

}
