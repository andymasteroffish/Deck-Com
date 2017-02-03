using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

	public Transform drawPileTransform, discardPileTransform, handTransform;

	private Unit owner;
	//private Hand hand;

	//the card objects before they come into the game
	public GameObject[] collection;

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
	public void setup(Unit _owner){
		owner = _owner;

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

		startPos = GameObject.Find ("handStartPos").transform;
		endPos = GameObject.Find ("handEndPos").transform;

		doingAnimation = false;
		setActive (false);
	}

	//starting the turn
	public void setActive(bool _isActive){
		isActive = _isActive;
		float moveTime = 0.2f;
		if (isActive) {
			StartCoroutine (doMoveAnimation (startPos.position, Vector3.zero, moveTime));
			foreach (Card card in hand) {
				card.setDisabled (owner.ActionsLeft <= 0);
			}
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

	public void drawCard(){
		if (drawPile.Count == 0) {
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

	public void discardHand(){
		while (hand.Count > 0) {
			addCardToDiscard (hand[0]);
			hand.RemoveAt (0);
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

	public void markCardPlayed(Card card){
		//get rid of this card
		addCardToDiscard(card);
		hand.Remove(card);
		alignCardsInHand();
		owner.GM.clearActiveCard ();
	}

	//visual things
	void alignCardsInHand(){
		for (int i=0; i<hand.Count; i++){
			hand[i].setPos (cardStartPos.position + cardSpacing * (float)i, i);
		}
	}

	//animations
	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time){
		doingAnimation = true;

		float timer = 0;

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
		if (owner.ActionsLeft < actionOrbSprites.Count) {
			Destroy (actionOrbSprites [actionOrbSprites.Count - 1].gameObject);
			actionOrbSprites.RemoveAt (actionOrbSprites.Count - 1);

			foreach (Card card in hand) {
				card.setDisabled (owner.ActionsLeft <= 0);
			}
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
}
