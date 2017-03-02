﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGO : MonoBehaviour {

	private Card card;

	public Text nameField;
	public Text descField;
	public SpriteRenderer spriteRend;

	private bool doingAnimation;

	private bool isActive;

	public float xSpacing;	//1.5
	public float mouseOverYRaise; //0.5
	public float activeYRaise;	//2

	public float moveTime;

	//sliding in and out
	//private Transform enterTrans, restTrans, endTrans = null;
	bool needsAnimationPositions = true;
	private Vector3 startPos, restPos, endPos;	//rest pos is the position of the far left card when it is not being raised up

	public void activate(Card _card){
		card = _card;
		isActive = true;
		gameObject.SetActive (true);

		if (needsAnimationPositions) {
			startPos = GameObject.Find ("card_startPos").transform.position;
			restPos = GameObject.Find ("card_endPos").transform.position;
			endPos = GameObject.Find ("card_endPos").transform.position;
		}

		gameObject.name = "card "+card.Owner.name+" "+card.name;

		transform.position = startPos;
		StartCoroutine (doMoveAnimation (getPos(), moveTime, false));
	}

	public void deactivate(){
		card = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "card unused";
	}
	
	// Update is called once per frame
	void Update () {

		//set the text
		nameField.text = card.name;
		descField.text = card.description;

		//set the position (if we're not sliding it)
		if (!doingAnimation) {
			Vector3 spritePos = getPos();

			if (card.mouseIsOver) {
				spritePos.z += -2;
				spritePos.y = mouseOverYRaise;
			}

			if (isActive) {
				spritePos.z += -5;
				spritePos.y = activeYRaise;
			}

			spriteRend.transform.position = spritePos;
		}

		//set the color
		Color col = new Color (1, 1, 1, 1);
		if (card.IsDisabled) {
			col = new Color (1, 1, 1, 0.4f);
		}
		spriteRend.color = col;

		//check if it is time to go away
	}

	Vector3 getPos(){
		return restPos + new Vector3 (xSpacing * card.orderInHand, 0, card.orderInHand * -0.1f)
	}


	IEnumerator doMoveAnimation(Vector3 target, float time, bool deactivateWhenDone){
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

		if (deactivateWhenDone) {
			deactivate ();
		}
	}

	IEnumerator doDeathAnimation(float time){
		doingAnimation = true;

		yield return new WaitForSeconds (0.05f);

		float timer = time  * card.Owner.GM.debugAnimationTimeMod;
		float startScale = transform.localScale.x;

		while (timer > 0) {
			timer -= Time.deltaTime;
			timer = Mathf.Max (0, timer);

			float newScale = timer / time;
			newScale = Mathf.Pow (newScale, 2);
			transform.localScale = new Vector3 (newScale, newScale, newScale);

			yield return null;
		}

		deck.removeCardFromHand (this);


		transform.localScale = new Vector3 (1, 1, 1);	

		if (markAsPlayedWhenDone) {
			owner.markCardPlayed (this);
		}

		if (!permanentlyDestroyCard) {
			deck.discardCard (this);
		} else {
			deck.destroyCard (this);
		}

		doingAnimation = false;
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
