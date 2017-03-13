using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootReward {


	public List<Card> cards;
	public int money;

	public LootReward(){
		cards = new List<Card> ();
		money = 0;
	}

	public void print(){
		if (money > 0) {
			Debug.Log ("$" + money);
		}
		if (cards.Count > 0) {
			foreach (Card card in cards) {
				Debug.Log ("card: "+card.idName);
			}
		}
	}

}
