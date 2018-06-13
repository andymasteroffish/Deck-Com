using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class EndGameManager {

	public List<LootReward> rewards;
	private List<Card> newCards;
	private int totalMoneyEarned;

	public bool hasStoreKey;	//set in EndGameReportInterface

	public EndGameManager(List<Card_Loot> lootCards){
		rewards = new List<LootReward> ();
		newCards = new List<Card> ();
		totalMoneyEarned = 0;

		//generate the rewards
		for (int i = 0; i < lootCards.Count; i++) {
			rewards.Add (getRewardFromLootCard (lootCards [i]));
		}

		//add the baseline money for the rewards
		foreach(LootReward reward in rewards) {
			totalMoneyEarned += reward.baseMoney;
		}
	}

	private LootReward getRewardFromLootCard(Card_Loot lootCard){

		LootReward reward = new LootReward();
		int level = (int)Mathf.Max(lootCard.level, 1);
		reward.baseMoney = level+1;

		int numCardsAtLevel = 2;
		int numCardPairsAtLowerLevel = 2;

		//every reward needs to include at least one movement card
		bool hasMovementCard = false;

		List<string> cardsThisLevel = CardManager.instance.getIDListAtLevel (level);
		List<string> cardsLowerLevel = CardManager.instance.getIDListAtLevel ( (int)Mathf.Max(1,level-1));

		int numLowerLevelCardsPerGroup = 2;
		if (level <= 1) {
			numLowerLevelCardsPerGroup = 1;
		}
		for (int i = 0; i < numCardPairsAtLowerLevel; i++) {
			Card[] cards = new Card[numLowerLevelCardsPerGroup];
			for (int j = 0; j < numLowerLevelCardsPerGroup; j++) {
				int idNum = (int)Random.Range (0, cardsLowerLevel.Count);
				string idName = cardsLowerLevel [idNum];
				//cardsLowerLevel.RemoveAt (idNum);	//no duplicate cards in the same pack
				//maybe this is OK for lower level packs
				Card card = CardManager.instance.getCardFromIdName (idName);
				card.setup (null, null);
				cards [j] = card;
				if (card.type == Card.CardType.Movement) {
					hasMovementCard = true;
				}
			}
			reward.addMultipleCards (cards);
		}

		for (int i = 0; i < numCardsAtLevel; i++) {
			int idNum = (int)Random.Range (0, cardsThisLevel.Count);
			string idName = cardsThisLevel [idNum];
			cardsThisLevel.RemoveAt (idNum);	//no duplicate cards in the same pack
			Card card = CardManager.instance.getCardFromIdName (idName);
			card.setup (null, null);
			if (card.type == Card.CardType.Movement) {
				hasMovementCard = true;
			}
			reward.addSingleCard (card);
		}

		//and a money option
		reward.addMoneyOption( getRewardMoney(level) );

		//if there is a movement card, return it, otheriwse try again
		if (hasMovementCard) {
			return reward;
		} else {
			return getRewardFromLootCard (lootCard);
		}
	}

	int getRewardMoney(int level){
		float val = Mathf.Pow (level, 1.4f) * 3.0f;
		//Debug.Log ("level " + level + "  val " + val);
		float wiggle = val * 0.4f;
		val += Random.Range (-wiggle, wiggle);
		return (int) val;
	}

	public void selectLoot(Card card, int money){
		if (card != null) {
			//Debug.Log ("love to add " + card.idName);
			newCards.Add (card);
		}
		totalMoneyEarned += money;
		//Debug.Log ("love to add $" + money);
	}

	public void saveCards(){
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
		for (int i = 0; i < newCards.Count; i++) {
			cardsToSave.Add (newCards [i].idName);
		}

		//save it
		File.WriteAllLines(unusedCardFilePath, cardsToSave.ToArray());
	}

	public void saveMoney(){
		//load in the player XML
		string xmlPath = Application.dataPath + "/external_data/player/player_info.xml";
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node and set the money field
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		int curMoney = int.Parse(infoNode["money"].InnerXml);
		int newMoney = curMoney + totalMoneyEarned;
		infoNode ["money"].InnerXml = newMoney.ToString ();

		//set if the store has been unlocked and set all cards in the store to not be purchased
		XmlNode storeNode = xmlDoc.GetElementsByTagName("store")[0];
		storeNode ["unlocked"].InnerXml = hasStoreKey.ToString ();
		for (int i = 0; i < 6; i++) {	//magic number! Bad!
			storeNode ["purchased_card" + i.ToString ()].InnerXml = false.ToString();
		}

		//save it
		xmlDoc.Save(xmlPath);
	}
}
