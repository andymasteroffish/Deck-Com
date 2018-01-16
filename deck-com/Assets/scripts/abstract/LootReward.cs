using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootReward {


	public List<List<Card>> cardGroups;
	public int baseMoney;
	public int moneyOption;

	public LootReward(){
		cardGroups = new List<List<Card>> ();
		baseMoney = 0;
		moneyOption = 0;
	}

	public void addSingleCard (Card card){
		cardGroups.Add (new List<Card> ());
		cardGroups [cardGroups.Count - 1].Add (card);
	}

	public void addMultipleCards(Card[] cards){
		cardGroups.Add (new List<Card> ());
		for (int i = 0; i < cards.Length; i++) {
			cardGroups [cardGroups.Count - 1].Add (cards[i]);
		}
	}

	public void addMoneyOption(int value){
		moneyOption = value;
	}

//	public void print(){
//		if (money > 0) {
//			Debug.Log ("$" + money);
//		}
//		for(int i=0; i<cardGroups.Count; i++){
//			Debug.Log ("Card Group: " + i);
//			foreach (Card card in cardGroups[i]) {
//				Debug.Log ("card: "+card.idName);
//			}
//		}
//	}

}
