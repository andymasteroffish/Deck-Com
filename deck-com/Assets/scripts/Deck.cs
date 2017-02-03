using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

	public Transform drawPileTransform, discardPileTransform;

	private Unit owner;
	private Hand hand;

	public GameObject[] collection;
	private List<Card> drawPile = new List<Card> ();
	private List<Card> discardPile = new List<Card> ();


	public void setup(Unit _owner){
		owner = _owner;
		hand = owner.hand;



		//create a card for each one in the collection and add it to the deck
		for (int i = 0; i < collection.Length; i++) {
			GameObject cardObj = Instantiate (collection [i], Vector3.zero, Quaternion.identity) as GameObject;
			cardObj.transform.parent = transform;
			Card thisCard = cardObj.GetComponent<Card> ();
			thisCard.setup (owner);
			cardObj.gameObject.SetActive (false);
			addCardToDrawPile (thisCard);
		}

		shuffle ();
	}

	void shuffle(){
		for (int i = 0; i < drawPile.Count * 50; i++) {
			int slotA = (int)Random.Range (0, drawPile.Count);
			int slotB = (int)Random.Range (0, drawPile.Count);
			Card temp = drawPile [slotA];
			drawPile [slotA] = drawPile [slotB];
			drawPile [slotB] = temp;
		}

	}



	public void drawCard(){
		if (drawPile.Count == 0) {
			putDiscardInDrawPile ();
			shuffle ();
		}

		hand.addCard (drawPile [0]);
		drawPile.RemoveAt (0);
	}

	public void addCardToDrawPile(Card card){
		drawPile.Add (card);
		card.gameObject.transform.parent = drawPileTransform;
		card.gameObject.SetActive (false);
	}
	public void addCardToDiscard(Card card){
		discardPile.Add (card);
		card.gameObject.transform.parent = discardPileTransform;
		card.gameObject.SetActive (false);
	}

	void putDiscardInDrawPile(){
		while (discardPile.Count > 0) {
			addCardToDrawPile (discardPile [0]);
			discardPile.RemoveAt (0);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
