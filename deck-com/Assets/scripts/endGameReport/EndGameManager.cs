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
			rewards.Add (getRewardFromLootCard (lootCards [i]));
		}

		//add it all up
		int totalMoneyEarned = 0;
		List<Card> newCards = new List<Card> ();
		foreach(LootReward reward in rewards) {
			totalMoneyEarned += reward.money;
			for (int i = 0; i < reward.cards.Count; i++) {
				newCards.Add (reward.cards [i]);
			}
		}

		Debug.Log ("money total: " + totalMoneyEarned);

		//save 'em!
		saveMoney(totalMoneyEarned);
		saveCards (newCards);

	}

	private LootReward getRewardFromLootCard(Card_Loot lootCard){

		int numCardsAtLevel = 3;
		int numCardsAtNextLeve = 1;
		int bonusSlots = 1;

		float chanceOfCardAtLevelBecomingMoney = 0.3f;
		float chaneMoneyReplacementDecrease = 0.1f;

		LootReward reward = new LootReward();
		int level = (int)Mathf.Max(lootCard.level, 1);

		reward.money = 0;

		List<string> cardsThisLevel = CardManager.instance.getIDListAtLevel (level);
		List<string> cardsNextLevel = CardManager.instance.getIDListAtLevel (level+1);

		for (int i = 0; i < numCardsAtLevel; i++) {
			float moneyRoll = Random.value;
			if (moneyRoll < chanceOfCardAtLevelBecomingMoney) {
				reward.money += getRewardMoney (level);
				chanceOfCardAtLevelBecomingMoney -= chaneMoneyReplacementDecrease;
			} else {
				int idNum = (int)Random.Range (0, cardsThisLevel.Count);
				string idName = cardsThisLevel [idNum];
				cardsThisLevel.RemoveAt (idNum);	//no duplicate cards in the same pack
				Card card = CardManager.instance.getCardFromIdName (idName);
				card.setup (null, null);
				reward.cards.Add (card);
			}
		}

		for (int i = 0; i < numCardsAtNextLeve; i++) {
			int idNum = (int)Random.Range (0, cardsNextLevel.Count);
			string idName = cardsNextLevel[idNum];
			cardsNextLevel.RemoveAt (idNum);	//no duplicate cards in the same pack
			Card card = CardManager.instance.getCardFromIdName(idName);
			card.setup (null, null);
			reward.cards.Add (card);
		}

		//bonus slot
		int roll = (int)Random.Range(0,4);
		if (roll == 0) {
			//do nothing
		} else if (roll == 1 || roll == 2) {
			List<string> listToUse = roll == 1 ? cardsThisLevel : cardsNextLevel;
			int idNum = (int)Random.Range (0, listToUse.Count);
			string idName = listToUse [idNum];
			listToUse.RemoveAt (idNum);	//no duplicate cards in the same pack
			Card card = CardManager.instance.getCardFromIdName (idName);
			card.setup (null, null);
			reward.cards.Add (card);
		} else if (roll == 3) {
			reward.money += getRewardMoney (level);
		}

		//pitty dollar
		if (reward.money <= 0) {
			reward.money = 1;
		}

		return reward;
	}

	int getRewardMoney(int level){
		float val = Mathf.Pow (level, 1.3f) * 3.3f;
		Debug.Log ("level " + level + "  val " + val);
		float wiggle = val * 0.4f;
		val += Random.Range (-wiggle, wiggle);
		return (int) val;
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
