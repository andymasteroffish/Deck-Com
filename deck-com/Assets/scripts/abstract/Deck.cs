using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class Deck {
	
	private Unit owner;

	//the piles
	private List<Card> drawPile;// = new List<Card> ();
	private List<Card> discardPile;// = new List<Card> ();
	private List<Card> hand;// = new List<Card>();

	public Deck(){
	}

	//creating an AI deck
	public Deck(Deck parent, Unit _owner){
		owner = _owner;

		drawPile = new List<Card> ();
		discardPile = new List<Card> ();
		hand = new List<Card>();

		//let's try leaving deck and discard empty and only fill up hand
		//THIS WILL MAKE AI FOR CARDS THAT DRAW CARDS OR OTHERWISE MANIPULATE THE DECK IMPOSSIBLE
		foreach (Card card in parent.hand) {
			Card newCard = CardManager.instance.getCardFromXMLNode (card.node);
			newCard.setup (owner, this);
			hand.Add (newCard);
		}
	}

	//setup
	public void setup(Unit _owner, string deckListPath){
		owner = _owner;

		drawPile = new List<Card> ();
		discardPile = new List<Card> ();
		hand = new List<Card>();

		//create a card for each one in the list and add it to the deck
		List<Card> cards = CardManager.instance.getDeckFromTextFile (deckListPath);
		for (int i = 0; i < cards.Count; i++) {
			cards [i].setup (owner, this);
			addCardToDrawPile ( cards[i] );
		}

		shuffle ();

		setActive (false);
	}

	//starting the turn
	public void setActive(bool isActive){
		if (isActive) {
			for (int i = 0; i < hand.Count; i++) {
				hand [i].setOwnerActive ();
			}
		}

		updateCardsDisabled ();
	}

	public Card getCardInHandFromID(string idName){
		for (int i=0; i<hand.Count; i++){
			if (hand[i].idName == idName){
				return hand [i];
			}
		}
		Debug.Log ("HAND CARD NOT FOUND!");
		return null;
	}
	//manipulating the deck
	void shuffle(){
		if (GameManagerTacticsInterface.instance.debugDoNotShuffle) {
			return;
		}
		for (int i = 0; i < drawPile.Count * 50; i++) {
			int slotA = (int)Random.Range (0, drawPile.Count);
			int slotB = (int)Random.Range (0, drawPile.Count);
			Card temp = drawPile [slotA];
			drawPile [slotA] = drawPile [slotB];
			drawPile [slotB] = temp;
		}
	}

	public void drawCards(int num){
		for (int i=0; i<num; i++){
			drawCard();
		}
	}
	public void drawCard(){
		if (drawPile.Count == 0) {
			putDiscardInDrawPile ();
			shuffle ();
		}

		if (drawPile.Count == 0) {
			Debug.Log ("no card to draw");
			return;
		}

		addCardToHand (drawPile [0]);
		drawPile.RemoveAt (0);
	}

	public void addCardToHand(Card card){
		card.resetCard ();

		hand.Add (card);
		alignCardsInHand ();

		if (owner.IsActive) {
			card.setOwnerActive ();
		}
	}

	public void addCardToDrawPile(Card card){
		drawPile.Add (card);
	}
	public void addCardToDiscard(Card card){
		discardPile.Add (card);
		alignCardsInHand ();
	}

	public void destroyCard(Card card){
		card.isDead = true;
		owner.GM.clearActiveCard ();
		alignCardsInHand ();
	}

	public void putDiscardInDrawPile(){
		while (discardPile.Count > 0) {
			addCardToDrawPile (discardPile [0]);
			discardPile.RemoveAt (0);
		}
	}

	public void discardHand(){
		for (int i = hand.Count-1; i >= 0; i--) {
			hand [i].discard ();
		}
	}

	public void updateCardsDisabled(){
		for (int i=0; i<hand.Count; i++){
			hand [i].updateIsDisabled ();
		}
	}

	//interacting with the hand
	public void checkClick(){
		//if nothing is selected, see if they clicked a card
		if (!isCardWaitingForInput ()) {
			foreach (Card card in hand) {
				if (card.MouseIsOver && !card.IsDisabled) {
					card.selectCard ();
					return;
				}
			}
		}
	}

	public void cancel(){
		for (int i=hand.Count-1; i>=0; i--){
			if (hand[i].IsActive) {
				hand[i].cancel ();
			}
		}
	}

	//call this before you discard a card form hand
	public void removeCardFromHand(Card card){
		hand.Remove(card);
		alignCardsInHand();
	}
	//this moves a card to the discard. It must have alreayd been rmeoved from the source
	public void discardCard(Card card){
		addCardToDiscard(card);
		owner.GM.clearActiveCard ();
	}


	//visual things
	void alignCardsInHand(){
		for (int i=0; i<hand.Count; i++){
			hand [i].orderInHand = i;
		}
	}

	//info about the hand
	public bool isCardWaitingForInput(){
		for (int i = 0; i < hand.Count; i++) {
			if (hand [i].isWaitingForInput ()) {
				return true;
			}
		}
		return false;
	}

	//finishing the game
	public List<Card_Loot> syphonLootFromDrawPile(){
		List<Card_Loot> loot = new List<Card_Loot> ();
		for (int i=drawPile.Count-1; i >= 0; i--){
			if (drawPile [i].type == Card.CardType.Loot) {
				Card_Loot lootCard = (Card_Loot)drawPile [i];
				drawPile.RemoveAt (i);
				loot.Add (lootCard);
			}
		}
		return loot;
	}

	//saving
	public void saveDrawPile(string filePath){
		sortDrawPileAlphabetically ();

		string[] lines = new string[drawPile.Count];
		for (int i = 0; i < drawPile.Count; i++) {
			lines [i] = drawPile [i].idName;
		}

		File.WriteAllLines(filePath, lines);
	}

	//sorting
	public void sortDrawPileAlphabetically(){
		//FILL THIS IN
	}

	//setters and getters
	public List<Card> Hand{
		get{
			return this.hand;
		}
	}
}
