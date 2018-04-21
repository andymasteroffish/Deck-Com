using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckInfoMarker : MonoBehaviour {


	public Transform anchorPos;
	public Vector2 anchorOffset;

	public Text cardsLeftInDeckText;

	public GameObject discardObj;
	public Text discardNameField;
	public Text discardDescField;
	public Text discardTypeField;
	public SpriteRenderer discardColorSprite;

	public float pauseBeforeSwitchingDiscard;	//this should be the same or slightly less than as the cardGO move time to let the card reahc the discard
	private float discardTimer;
	private Card lastDiscardCard;
	private Unit lastActiveUnit;

	// Use this for initialization
	void Start () {
		discardTimer = 0;
	}
	
	// Update is called once per frame
	void Update () {
		discardTimer -= Time.deltaTime;

		Unit myUnit =  GameManagerTacticsInterface.instance.gm.activePlayerUnit;

		//place it
		transform.position = anchorPos.position + new Vector3 (anchorOffset.x, anchorOffset.y, 0); 

		//update the draw pile text
		cardsLeftInDeckText.text = myUnit.deck.getNumCardsInDrawPile().ToString();

		//and the discard pile
		Card topDiscard = myUnit.deck.getTopCardOfDiscard();

		//some timing info

		//if the top discard just changed, we shoudl wait a sec to change the card
		if (topDiscard != lastDiscardCard) {
			discardTimer = pauseBeforeSwitchingDiscard;
		}

		//if the unit just changed, always update the discard card
		//also update it if we just ran out of cards int he discard
		if (myUnit != lastActiveUnit || topDiscard == null) {
			discardTimer = 0;
		}

		//set the info if it isn't locked for an animation
		if (discardTimer <= 0){
			if (topDiscard == null) {
				discardObj.SetActive (false);
			} else {
				discardObj.SetActive (true);
				discardNameField.text = topDiscard.name;
				discardDescField.text = topDiscard.description;
				discardTypeField.text = CardManager.instance.TypeNames [topDiscard.type];

				discardColorSprite.color = new Color(topDiscard.baseHighlightColor.r, topDiscard.baseHighlightColor.g, topDiscard.baseHighlightColor.b, 0.3f);
			}
		}

		lastDiscardCard = topDiscard;
		lastActiveUnit = myUnit;
		
	}

	public void setTopDiscardCard(Card card){

	}
}
