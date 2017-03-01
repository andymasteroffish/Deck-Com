using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

	public Transform drawPileTransform, discardPileTransform, handTransform;

	private Unit owner;
	//private Hand hand;

	//the card objects before they come into the game
	//private List<GameObject[] collection;

	//the piles
	private List<Card> drawPile = new List<Card> ();
	private List<Card> discardPile = new List<Card> ();
	private List<Card> hand = new List<Card>();

	//hand display info
	public Transform cardStartPos;
	public Vector3 cardSpacing;

	public Transform actionOrbStartPos;
	public float actionOrbSpacing;
	private List<SpriteRenderer> actionOrbSprites = new List<SpriteRenderer>();
	public GameObject actionOrbPrefab;

	//sliding in when a unit starts their turn
	private Transform startPos, endPos;

	private bool isActive;
	private bool doingAnimation;

	//setup
	public void setup(Unit _owner, TextAsset deckList){
		owner = _owner;

		//create a card for each one in the list and add it to the deck
		List<GameObject> cardPrefabs = CardManager.instance.getCardPrefabsFromTextFile (deckList);
		for (int i = 0; i < cardPrefabs.Count; i++) {
			addCardToDrawPile ( spawnCard(cardPrefabs[i]) );
		}

		shuffle ();

		startPos = GameObject.Find ("handStartPos").transform;
		endPos = GameObject.Find ("handEndPos").transform;

		doingAnimation = false;
		setActive (false);
	}

	public Card spawnCard(GameObject prefab){
		GameObject cardObj = Instantiate (prefab, Vector3.zero, Quaternion.identity) as GameObject;
		cardObj.transform.parent = transform;
		Card thisCard = cardObj.GetComponent<Card> ();
		thisCard.setup (owner, this);
		cardObj.gameObject.SetActive (false);
		return thisCard;
	}

	//starting the turn
	public void setActive(bool _isActive){
		isActive = _isActive;
		float moveTime = 0.2f;
		if (isActive) {
			StartCoroutine (doMoveAnimation (startPos.position, Vector3.zero, moveTime));
		} else {
			StartCoroutine (doMoveAnimation (Vector3.zero, endPos.position, moveTime));
		}
	}

	//manipulating the deck
	void shuffle(){
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
			Debug.Log ("need to shuff");
			putDiscardInDrawPile ();
			shuffle ();
		}

		addCardToHand (drawPile [0]);
		drawPile.RemoveAt (0);
	}

	public void addCardToHand(Card card){
		card.gameObject.SetActive (true);
		card.gameObject.transform.parent = handTransform;
		card.reset ();

		hand.Add (card);
		alignCardsInHand ();

		card.startDrawAnimation ();
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
		alignCardsInHand ();
	}

	public void destroyCard(Card card){
		Destroy (card.gameObject);
		owner.GM.clearActiveCard ();
		alignCardsInHand ();
	}

	void putDiscardInDrawPile(){
		Debug.Log ("cards in discard: " + discardPile.Count);
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
		foreach (Card card in hand) {
			if (card.IsActive) {
				card.cancel ();
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
			hand[i].setPos (cardStartPos.localPosition + cardSpacing * (float)i, i);
		}
	}

	//animations
	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time){
		doingAnimation = true;

		float timer = 0;
		time *= owner.GM.debugAnimationTimeMod;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			prc = Mathf.Pow (prc, 0.75f);
			transform.position = Vector3.Lerp (start, target, prc);
			yield return null;
		}

		doingAnimation = false;
		transform.position = target;
	}
	
	// Update is called once per frame
	void Update () {

		//making sure we have the right orb count
		if (owner.ActionsLeft > actionOrbSprites.Count) {
			GameObject orbObj = Instantiate (actionOrbPrefab, actionOrbStartPos.position + new Vector3 (actionOrbSpacing * actionOrbSprites.Count, 0, 0), Quaternion.identity) as GameObject;
			orbObj.transform.parent = transform;
			actionOrbSprites.Add(orbObj.GetComponent<SpriteRenderer>());
		}
		if (owner.ActionsLeft < actionOrbSprites.Count && actionOrbSprites.Count > 0) {
			Destroy (actionOrbSprites [actionOrbSprites.Count - 1].gameObject);
			actionOrbSprites.RemoveAt (actionOrbSprites.Count - 1);

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

	//checking for animations
	public bool areAnimationsHappening(){
		if (doingAnimation) {
			return true;
		}
		foreach (Card card in hand) {
			if (card.DoingAnimation) {
				return true;
			}
		}

		return false;
	}
}
