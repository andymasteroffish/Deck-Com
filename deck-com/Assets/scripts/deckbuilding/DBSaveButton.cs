using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBSaveButton : MonoBehaviour {

	private bool mouseIsOver;

	public GameObject displayHolder;

	public SpriteRenderer frameRend;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);
	public Color badColor = new Color(1f, 0.75f,0.75f);

	private DBManager manager = null;

	public Text text;

	// Use this for initialization
	void Start () {
		mouseIsOver = false;
	}

	// Update is called once per frame
	void Update () {

		if (manager == null) {
			manager = DBManagerInterface.instance.manager;
		}

		bool gotThatCash = false;
		if (manager.activeDeck != null) {
			gotThatCash = manager.money >= manager.activeDeck.getCurrentSaveCost ();
		}

		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;

		//figure out when this thing should be on
		bool shouldShow = false;
		if (manager.activeDeck != null && !manager.unusedCardsOpen && (manager.activeDeck.cardsToAdd.Count > 0 || manager.activeDeck.weaponToAdd != null || manager.activeDeck.charmToAdd != null)) {
			shouldShow = true;

			//and if it is on, set the text
			text.text = "Save ($"+manager.activeDeck.getCurrentSaveCost()+")";

			if (!gotThatCash) {
				frameRend.color = badColor;
			}

		}

		displayHolder.SetActive (shouldShow);

		if (shouldShow && Input.GetMouseButtonDown (0) && mouseIsOver && gotThatCash) {
			manager.saveChanges ();
			mouseIsOver = false;
		}

	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}
}
