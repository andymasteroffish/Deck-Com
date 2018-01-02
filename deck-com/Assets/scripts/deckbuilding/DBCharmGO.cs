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

	public Vector3 smallOffsetFromDeckButton;
	public Vector3 normalOffsetFromDeckButton;
	public float ySpacing;

	public float smallScale;

	public SpriteRenderer frameRend;
	public Text nameText, descText;

	private bool mouseIsOver;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);
	public Color newCharmColor = Color.cyan;

	private bool isActive;

	private bool isNew;

	
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
		isNew = false;
		string nameString = "Open";
		string descString = "no desc";
		if (type == Charm.CharmType.Weapon) {
			if (deck.weaponToAdd != null) {
				nameString = deck.weaponToAdd.name;
				descString = deck.weaponToAdd.description;
				isNew = true;
			} else {
				nameString = deck.curWeapon.name;
				descString = deck.curWeapon.description;
			}
		} 
		if (type == Charm.CharmType.Equipment){
			if (deck.charmToAdd != null) {
				nameString = deck.charmToAdd.name;
				descString = deck.charmToAdd.description;
				isNew = true;
			}
			else if (deck.curCharm != null) {
				nameString = deck.curCharm.name;
				descString = deck.curCharm.description;
			}
		}

		//shrink if we are not editting this unit's deck
		bool useSmallScale = manager.activeDeck == null;

		nameText.text = nameString;
		descText.text = descString;

		Vector3 offsetThisFrame = useSmallScale ? smallOffsetFromDeckButton : normalOffsetFromDeckButton;
		transform.position = deckButton.transform.position + offsetThisFrame  + new Vector3 (0, ySpacing, 0) * (int)type;

		if (useSmallScale){
			transform.localScale = new Vector3 (smallScale, smallScale, smallScale);
		}else{
			transform.localScale = Vector3.one;
		}

	
		//update the color
		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;
		if (isNew) {
			frameRend.color = newCharmColor;
		}

		//were we clicked?
		if (Input.GetMouseButtonDown (0) && mouseIsOver) {
			Debug.Log ("luv 2 fuk");
			manager.openUnusedWeapons();
			mouseIsOver = false;
		}
	}


	void OnMouseEnter(){
		mouseIsOver = true;

		//no clicking this on standard select screen
		if (manager.activeDeck == null || isNew) {
			mouseIsOver = false;
		}
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
