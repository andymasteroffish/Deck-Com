using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hand : MonoBehaviour {

	public Transform cardStartPos;
	public Vector3 cardSpacing;

	public Transform actionOrbStartPos;
	public float actionOrbSpacing;
	private List<SpriteRenderer> actionOrbSprites = new List<SpriteRenderer>();
	public GameObject actionOrbPrefab;

	//public GameObject cardTestPrefab;

	private List<Card> cards = new List<Card>();

	private Unit owner;
	private Deck deck;

	private Transform startPos, endPos;

	private bool isActive;
	private bool doingAnimation;


	public void setup(Unit _owner) {
		owner = _owner;
		deck = owner.deck;

		startPos = GameObject.Find ("handStartPos").transform;
		endPos = GameObject.Find ("handEndPos").transform;

		doingAnimation = false;
		setActive (false);

		//testing
//		for (int i = 0; i < 3; i++) {
//			GameObject cardObj = Instantiate (cardTestPrefab, new Vector3 (0, 0, 0), Quaternion.identity) as GameObject;
//			cardObj.transform.parent = transform;
//			Card thisCard = cardObj.GetComponent<Card> ();
//			thisCard.setup (owner);
//			thisCard.setPos (cardStartPos.position + cardSpacing * (float)i, i);
//			cards.Add (thisCard);
//		}
	}

	public void setActive(bool _isActive){
		isActive = _isActive;
		float moveTime = 0.2f;
		if (isActive) {
			StartCoroutine (doMoveAnimation (startPos.position, Vector3.zero, moveTime));
			foreach (Card card in cards) {
				card.setDisabled (owner.ActionsLeft <= 0);
			}
		} else {
			StartCoroutine (doMoveAnimation (Vector3.zero, endPos.position, moveTime));
		}
	}

	public void addCard(Card card){
		card.gameObject.SetActive (true);
		card.gameObject.transform.parent = transform;
		card.setPos (cardStartPos.position + cardSpacing * (float)cards.Count, cards.Count);
		card.reset ();

		cards.Add (card);
	}

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

			foreach (Card card in cards) {
				card.setDisabled (owner.ActionsLeft <= 0);
			}
		}
	}

	public void cancel(){
		foreach (Card card in cards) {
			if (card.isWaitingForInput ()) {
				card.cancel ();
			}
		}
	}

	public void discardHand(){
		while (cards.Count > 0) {
			deck.addCardToDiscard (cards[0]);
			cards.RemoveAt (0);
		}
	}

	public void checkClick(){
		//if nothing is selected, see if they clicked a card
		if (!isCardWaitingForInput ()) {
			foreach (Card card in cards) {
				if (card.MouseIsOver && !card.IsDisabled) {
					card.selectCard ();
					return;
				}
			}
		}
	}

	public void markCardPlayed(Card card){
		//get rid of this card
		deck.addCardToDiscard(card);
		cards.Remove(card);
		//adjust remaining cards
		for (int i=0; i<cards.Count; i++){
			cards[i].setPos (cardStartPos.position + cardSpacing * (float)i, i);
		}
	}

	public void passInTile(Tile tile){
		//check if any card is witing on tile input
		foreach (Card card in cards) {
			if (card.WaitingForTile) {
				card.passInTile (tile);
				return;
			}
		}
	}
	public void passInUnit(Unit unit){
		//check if any card is witing on unit input
		foreach (Card card in cards) {
			if (card.WaitingForUnit) {
				card.passInUnit (unit);
				return;
			}
		}
	}

	public bool isCardWaitingForInput(){
		for (int i = 0; i < cards.Count; i++) {
			if (cards [i].isWaitingForInput ()) {
				return true;
			}
		}
		return false;
	}
}
