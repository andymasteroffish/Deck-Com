﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBDeckButtonGO : MonoBehaviour {

	private bool isActive;

	private DBDeck deck;

	private bool mouseIsOver;

	public SpriteRenderer frameSpriteRend, iconSpriteRend;

	public Text nameText, descText;

	private bool needsPosInfo = true;
	private Vector3 topLeftPoint;
	private Vector3 homePos;

	public Vector3 spacing;

	public void activate(DBDeck _deck){
		isActive = true;
		deck = _deck;
		gameObject.SetActive (true);

		if (needsPosInfo){
			needsPosInfo = false;
			topLeftPoint = GameObject.Find("deckButtonTopLeft").transform.position;
		}

		homePos = topLeftPoint + spacing * deck.order;

		iconSpriteRend.sprite = deck.sprite;
		nameText.text = deck.displayName;
		descText.text = deck.cards.Count + " cards";

	}

	public void deactivate(){
		isActive = false;
		deck = null;
		gameObject.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {

		//tint the frame when ghighlighted
		if (mouseIsOver) {
			frameSpriteRend.color = new Color (0.75f, 0.75f, 0.75f);
		} else {
			frameSpriteRend.color = new Color (1f, 1f, 1f);
		}

		if (Input.GetMouseButtonDown (0) && mouseIsOver) {
			DBManagerInterface.instance.click (deck);
		}

		//keeping it where it needs to be
		if (DBManagerInterface.instance.manager.activeDeck == null) {
			transform.position = homePos;
		} else {
			transform.position = homePos - new Vector3 (10, 0, 0);
		}
	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}


	public bool IsActive{
		get{
			return this.isActive;
		}
	}
}
