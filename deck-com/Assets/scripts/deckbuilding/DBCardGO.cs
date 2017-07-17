using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBCardGO : MonoBehaviour {

	private DBManager manager;

	private Card card;
	private int order;

	private bool needPosInfo = true;
	private Vector3 topLeftPoint;

	private Vector3 homePos;
	public Vector3 spacing;

	public int numCardsPerCol, numCardsPerColumnUnusedCards;

	public Text nameField;
	public Text descField;
	public SpriteRenderer spriteRend;
	public SpriteRenderer colorSprite;

	private bool doingAnimation;

	private bool isActive;

	public Vector3 mouseOverAdjust;

	public float moveTime;

	private bool mouseIsOver;

	//positiong the cards used to select from the unused list
	private bool isUnusedCardSelector;
	public Vector3 unusedCardOffset;

	public void activate(Card _card, int _order, bool _isUnusedCardSelector){
		card = _card;
		isUnusedCardSelector = _isUnusedCardSelector;

		manager = DBManagerInterface.instance.manager;

		isActive = true;
		gameObject.SetActive (true);

		gameObject.name = "card "+card.name;

		if (needPosInfo) {
			needPosInfo = false;
			topLeftPoint = GameObject.Find ("cardTopLeft").transform.position;
		}

		setPos (_order);

		spriteRend.transform.localPosition = Vector3.zero;
		spriteRend.color = Color.white;

		colorSprite.color = new Color(card.baseHighlightColor.r, card.baseHighlightColor.g, card.baseHighlightColor.b, 0.3f);


		//set the text
		nameField.text = card.name;
		descField.text = card.description;

		mouseIsOver = false;
	}

	public void setPos(int _order){
		order = _order;

		int col = order / numCardsPerCol;
		int row = order % numCardsPerCol;
		if (isUnusedCardSelector) {
			col = order / numCardsPerColumnUnusedCards;
			row = order % numCardsPerColumnUnusedCards;
		}

		homePos = topLeftPoint + new Vector3(spacing.x * col, spacing.y * row, spacing.z * row);

		if (isUnusedCardSelector) {
			homePos += unusedCardOffset;
		}

		transform.position = homePos;
	}

	public void deactivate(){
		card = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "card unused";
	}
	
	// Update is called once per frame
	void Update () {

		//clicks
		if (mouseIsOver && Input.GetMouseButtonDown (0)) {
			if (isUnusedCardSelector) {
				manager.addUnusedCardToDeck (card);
			} else {
				manager.deckCardClicked (card);
			}
		}

		//set the sprite color if this is a real ass button
		if (isUnusedCardSelector) {
			if (mouseIsOver) {
				spriteRend.color = new Color (0.75f, 0.75f, 0.75f);
			} else {
				spriteRend.color = new Color (1f, 1f, 1f);
			}
		}

		//set the position (if we're not sliding it)
		if (!doingAnimation) {
			Vector3 spritePos = new Vector3 (0, 0, 0);

			if (mouseIsOver) {
				spritePos += mouseOverAdjust;
			}

			spriteRend.transform.localPosition = spritePos;
		}

		//time to die?
		if (!isUnusedCardSelector) {
			if (manager.activeDeck == null) {
				deactivate ();
			} else if (manager.activeDeck.cards.Contains (card)==false && manager.activeDeck.cardsToAdd.Contains (card)==false) {
				deactivate ();
			}
		}
		if (isUnusedCardSelector && !manager.unusedCardsOpen) {
			deactivate ();
		}

		//recently added cards can be removed and may need to be repositioned
		if (manager.activeDeck != null && manager.activeDeck.cardsToAdd.Contains (card)) {
			int thisOrder = manager.activeDeck.cards.Count;
			for (int i = 0; i < manager.activeDeck.cardsToAdd.Count; i++) {
				thisOrder++;
				if (manager.activeDeck.cardsToAdd [i] == card) {
					break;
				}
			}
			setPos (thisOrder);
		}

	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}

//	IEnumerator doMoveAnimation(Vector3 target, float time, bool deactivateWhenDone){
//		doingAnimation = true;
//
//		Vector3 startPos = transform.position;
//
//		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;
//
//		float timer = 0;
//
//		while (timer < time) {
//			timer += Time.deltaTime;
//			float prc = Mathf.Clamp (timer / time, 0, 1);
//			prc = Mathf.Pow (prc, 0.75f);
//			transform.position = Vector3.Lerp (startPos, target, prc);
//			yield return null;
//		}
//
//		doingAnimation = false;
//		transform.position = target;
//
//		if (deactivateWhenDone) {
//			deactivate ();
//		}
//	}



	public void setSpriteColor(Color col){
		spriteRend.color = col;
	}

	//setters and getters
	public bool IsActive{
		get{
			return this.isActive;
		}
		set{
			isActive = value;
		}
	}
	public bool DoingAnimation {
		get {
			return this.doingAnimation;
		}
	}
}
