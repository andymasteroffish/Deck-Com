using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class EndGameManager {

	public List<LootReward> rewards;


	public EndGameManager(List<Card_Loot> lootCards){

		rewards = new List<LootReward> ();
		//generate the rewards
		for (int i = 0; i < lootCards.Count; i++) {
			if (lootCards [i].lootType == Loot.Type.booster) {
				rewards.Add (getBoosterRewardFromLootCard (lootCards [i]));
			}
			if (lootCards [i].lootType == Loot.Type.money) {
				rewards.Add (getMoneyRewardFromLootCard (lootCards [i]));
			}
		}

		//testing
//		for (int i = 0; i < rewards.Count; i++) {
//			Debug.Log ("-- reward level " + lootCards [i].level + " --");
//			rewards [i].print ();
//		}

		//add it all up
		int totalMoneyEarned = 0;
		List<Card> newCards = new List<Card> ();
		foreach(LootReward reward in rewards) {
			totalMoneyEarned += reward.money;
			for (int i = 0; i < reward.cards.Count; i++) {
				newCards.Add (reward.cards [i]);
			}
		}

		//save 'em!
		saveMoney(totalMoneyEarned);
		saveCards (newCards);

	}

	private LootReward getMoneyRewardFromLootCard(Card_Loot lootCard){
		LootReward reward = new LootReward();
		int level = lootCard.level;

		float baseValue = 6 * level;
		float variance = 2 * level;

		reward.money = Mathf.RoundToInt((baseValue + Random.Range (-variance, variance)));
		reward.money = Mathf.Max (1, reward.money);	//make sure they can't get less than 1 somehow

		return reward;
	}

	private LootReward getBoosterRewardFromLootCard(Card_Loot lootCard){

		LootReward reward = new LootReward();
		int level = lootCard.level;

		float[] chanceOfCardAtLevel = new float[3];	//must all add up to 1
		chanceOfCardAtLevel [0] = 							0.4f;
		chanceOfCardAtLevel [1] = chanceOfCardAtLevel [0] + 0.4f;
		chanceOfCardAtLevel [2] = chanceOfCardAtLevel [1] + 0.2f;

		List<string>[] possibleCardIDs = new List<string>[3];
		for (int i = 0; i < possibleCardIDs.Length; i++) {
			possibleCardIDs [i] = CardManager.instance.getIDListAtLevel (level - 1 + i);
		}

		int numCards = (int)Random.Range (2, 5);
		for (int i = 0; i < numCards; i++) {
			float roll = Random.value;

			int listToUse = 0;
			for (int k = 0; k < chanceOfCardAtLevel.Length; k++) {
				if (roll > chanceOfCardAtLevel [k]) {
					listToUse = k;
				}
			}
			Debug.Log ("list to use: " + listToUse);
			if (possibleCardIDs[listToUse].Count == 0) listToUse++;

			int idNum = (int)Random.Range (0, possibleCardIDs [listToUse].Count);
			string idName = possibleCardIDs [listToUse] [idNum];
			possibleCardIDs [listToUse].RemoveAt (idNum);	//no duplicate cards in the same pack
			Card card = CardManager.instance.getCardFromIdName(idName);
			card.setup (null, null);
			reward.cards.Add (card);
		}

		return reward;
	}

	private void saveCards(List<Card> newCards){
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

	private void saveMoney(int freshEarnings){



		//load in the player XML
		string xmlPath = Application.dataPath + "/external_data/player/player_info.xml";
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);

		//get the info node and set the money field
		XmlNode infoNode = xmlDoc.GetElementsByTagName("info")[0];
		int curMoney = int.Parse(infoNode["money"].InnerXml);
		int newMoney = curMoney + freshEarnings;
		infoNode ["money"].InnerXml = newMoney.ToString ();

		//save it
		xmlDoc.Save(xmlPath);
	}
}
