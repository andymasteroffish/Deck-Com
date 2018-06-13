using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System;
using System.IO;


public class StoreManager {


	public int money;
	private int curLevel, curAreaNum;

	private string xmlPath;

	public List<StoreCard> cards;


	private int getCostForLevel(int level){
		if (level == 1)		return 2;
		if (level == 2)		return 4;
		if (level == 3)		return 7;

		Debug.Log ("BAD! NO COST FOR THIS LEVEL");
		return -1;
	}


	public StoreManager(){

		//grabbing the info for the player
		xmlPath = Application.dataPath + "/external_data/player/player_info.xml";
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		money = int.Parse(infoNode["money"].InnerXml);

		//get the store node
		XmlNode storeNode = xmlDoc.GetElementsByTagName("store")[0];

		//get the area
		curLevel = int.Parse (infoNode ["cur_level"].InnerXml);
		curAreaNum = (curLevel / 3) + 1;		//this needs to match what tactics interface syas and should not be a magic number
		if (curLevel < 0) {
			curAreaNum = 0;
		}

		Debug.Log ("cur level: " + curAreaNum);

		//seed the store
		int seed = int.Parse(infoNode["seed"].InnerXml);
		UnityEngine.Random.seed = seed + (curLevel * 101);


		//grab some cards
		List<string> cardsPrevLevel = CardManager.instance.getIDListAtLevel ( Mathf.Max(1, curAreaNum-1));
		List<string> cardsThisLevel = CardManager.instance.getIDListAtLevel (curAreaNum);
		List<string> cardsNextLevel = CardManager.instance.getIDListAtLevel ( Mathf.Min(3, curAreaNum+1));	//TODO: 3 is a magic number!

		List<string> cardIDs = new List<string> ();
		for (int i = 0; i < 2; i++) {
			int randID = (int)UnityEngine.Random.Range (0, cardsPrevLevel.Count - 1);
			cardIDs.Add (cardsPrevLevel [randID]);
			cardsPrevLevel.RemoveAt (randID);
		}
		for (int i = 0; i < 3; i++) {
			int randID = (int)UnityEngine.Random.Range (0, cardsThisLevel.Count - 1);
			cardIDs.Add (cardsThisLevel [randID]);
			cardsThisLevel.RemoveAt (randID);
		}
		for (int i = 0; i < 1; i++) {
			int randID = (int)UnityEngine.Random.Range (0, cardsNextLevel.Count - 1);
			cardIDs.Add (cardsNextLevel [randID]);
			cardsNextLevel.RemoveAt (randID);
		}


		//make store cards for them
		cards = new List<StoreCard>();
		for (int i = 0; i < cardIDs.Count; i++) {
			Debug.Log (i + ": " + cardIDs [i]);
			Card thisCard = CardManager.instance.getCardFromIdName (cardIDs [i]);
			thisCard.setup (null, null);
			int cost = getCostForLevel (thisCard.cardLevel);
			bool hasBeenBought = bool.Parse (storeNode ["purchased_card" + i.ToString ()].InnerXml);
			cards.Add( new StoreCard (thisCard, cost, hasBeenBought) );
		}

	}

	public void buyCard(int IDNum){
		money -= cards [IDNum].cost;
		cards [IDNum].hasBeenBought = true;
		saveCards (cards[IDNum].card.idName);
		savePlayerInfo ();
	}


	public void saveCards(string newCardIDName){
		//load in the unused cards
		string unusedCardFilePath = Application.dataPath + "/external_data/player/unused_cards.txt";
		string fileText = File.ReadAllText (unusedCardFilePath);
		string[] fileLines = fileText.Split ('\n');

		//combine the existing cards with the new ones
		List<string> cardsToSave = new List<string> ();
		for (int i = 0; i < fileLines.Length; i++) {
			if (fileLines [i].Length > 1) {
				cardsToSave.Add (fileLines [i]);
			}
		}

		//add the new card
		cardsToSave.Add(newCardIDName);

		//save it
		File.WriteAllLines(unusedCardFilePath, cardsToSave.ToArray());
	}

	public void savePlayerInfo(){
		//load in the player XML
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node and set the money field
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		infoNode ["money"].InnerXml = money.ToString ();

		//set the store purchases field
		XmlNode storeNode = xmlDoc.GetElementsByTagName("store")[0];
		for (int i = 0; i < cards.Count; i++) {
			storeNode ["purchased_card" + i.ToString ()].InnerXml = cards [i].hasBeenBought.ToString();
		}

		//save it
		xmlDoc.Save(xmlPath);
	}
}
