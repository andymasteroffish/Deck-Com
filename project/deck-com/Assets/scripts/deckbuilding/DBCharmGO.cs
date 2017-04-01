using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBCharmGO : MonoBehaviour {

	private DBManager manager;

	private DBDeck deck;
	private DBDeckButtonGO deckButton;

	private Charm.CharmType type;
	private Charm charm;

	public Vector3 startOffsetFromDeckButton;
	public float ySpacing;

	public SpriteRenderer frameRend;
	public Text nameText;

	private bool mouseIsOver;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);
	public Color newCharmColor = Color.cyan;

	private bool isActive;

	
	public void activate(DBDeck _deck, DBDeckButtonGO _deckButton, Charm.CharmType _type){
		deck = _deck;
		deckButton = _deckButton;

		type = _type;

		isActive = true;
		gameObject.SetActive (true);

		gameObject.name = "charm "+deck.displayName+" "+type;

		gameObject.SetActive (true);

		mouseIsOver = false;
	}

	public void deactivate(){
		isActive = false;
		deck = null;
		deckButton = null;
		gameObject.SetActive (false);
		gameObject.name = "charm unused";
	}

	void Update(){
		manager = DBManagerInterface.instance.manager;

		//time to die?
		if (!deckButton.IsActive) {
			deactivate ();
			return;
		}
		if (deck == manager.unusedCardsDeck) {
			deactivate ();
			return;
		}



		//update this thing
		bool isNew = false;
		string nameString = "Open";
		if (type == Charm.CharmType.Weapon) {
			if (deck.weaponToAdd != null) {
				nameString = deck.weaponToAdd.name;
				isNew = true;
			} else {
				nameString = deck.curWeapon.name;
			}
		} 
		if (type == Charm.CharmType.Charm){
			if (deck.charmToAdd != null) {
				nameString = deck.charmToAdd.name;
				isNew = true;
			}
			else if (deck.curCharm != null) {
				nameString = deck.curCharm.name;
			}
		}
			
		nameText.text = nameString;
		transform.position = deckButton.transform.position + startOffsetFromDeckButton + new Vector3 (0, ySpacing, 0) * (int)type;
	
		//update the color
		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;
		if (isNew) {
			frameRend.color = newCharmColor;
		}

		//no clicking this on standard select screen
		if (manager.activeDeck == null || isNew) {
			mouseIsOver = false;
		}

		//were we clicked?
		if (Input.GetMouseButtonDown (0) && mouseIsOver) {
			manager.openUnusedCharms (type);
			mouseIsOver = false;
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
