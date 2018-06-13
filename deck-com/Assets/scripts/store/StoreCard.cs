using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreCard  {

	public Card card;
	public int cost;
	public bool hasBeenBought;

	public StoreCard(Card _card, int _cost, bool _hasBeenBought){
		card = _card;
		cost = _cost;
		hasBeenBought = _hasBeenBought;
	}
}
