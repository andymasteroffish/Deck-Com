using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Card : MonoBehaviour {

	public enum CardType{Attack, Movement, Other};

	public string name;
	public CardType type;

	[System.NonSerialized]
	public Text nameField;
	[System.NonSerialized]
	public Text textField;
	[System.NonSerialized]
	public SpriteRenderer spriteRend;

	private bool isActive;
	private bool waitingForTile, waitingForUnit, waitingForChoice;

	private bool mouseIsOver;
	private bool isDisabled;

	private float order;

	private Unit owner;
	private Deck deck;

	//some colors
	[System.NonSerialized]
	public Color moveHighlightColor = new Color(0.5f, 0.5f, 1f);
	[System.NonSerialized]
	public Color attackHighlightColor  = new Color(1f, 0.5f, 0.5f);

	private bool doingAnimation;


	public void setup(Unit _owner, Deck _deck){
		owner = _owner;
		deck = _deck;

		doingAnimation = false;

		//find all the parts
		Transform[] ts = gameObject.transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in ts){
			if (t.gameObject.name == "NameText") {
				nameField = t.gameObject.GetComponent<Text> ();
			}
			if (t.gameObject.name == "BodyText") {
				textField = t.gameObject.GetComponent<Text> ();
			}
			if (t.gameObject.name == "sprite") {
				spriteRend = t.gameObject.GetComponent<SpriteRenderer> ();
			}
		}

		nameField.text = name;
		//textField.text = "Hey, I'm testin over here! "+Random.Range(0,999).ToString();


		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void reset(){
		isDisabled = false;
		cancel ();
	}

	public void setPos(Vector3 newPos, int _order){
		order = (float)_order;
		transform.position = newPos;
		cancel ();
	}

	
	// Update is called once per frame
	void Update () {
		Vector3 spritePos = new Vector3 (0, 0, order * -0.1f);
		Color col = new Color (1, 1, 1, 1);
		if (mouseIsOver) {
			spritePos.z += -2;
			spritePos.y = 0.5f;
		}

		if (isActive) {
			spritePos.z += -5;
			spritePos.y = 2;
		}

		if (isDisabled) {
			col = new Color (1, 1, 1, 0.4f);
		}

		spriteRend.transform.localPosition = spritePos;
		spriteRend.color = col;
			
	}

	public void selectCard(){
		isActive = true;
		owner.GM.setCardActive (this);
		selectCardCustom ();
	}
	public virtual void selectCardCustom(){}

	public void cancel(){
		waitingForTile = false;
		waitingForUnit = false;
		waitingForChoice = false;
		if (isActive) {
			isActive = false;
			owner.GM.clearActiveCard ();
		}
	}

	public void passInTile(Tile tile){
		Debug.Log ("Love it");
		if (waitingForTile) {
			passInTileCustom (tile);
		}
	}
	public virtual void passInTileCustom(Tile tile){}

	public void passInUnit(Unit unit){
		if (waitingForUnit) {
			passInUnitCustom (unit);
		}
	}
	public virtual void passInUnitCustom(Unit unit){}

	public void setDisabled(bool val){
		isDisabled = val;
	}

	public void finish(){
		StartCoroutine(doDeathAnimation(0.5f, true));
	}
	public void discard(){
		StartCoroutine(doDeathAnimation(0.5f, false));
	}

	public bool isWaitingForInput(){
		return waitingForTile || waitingForUnit || waitingForChoice;
	}

	//utility
	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}

	//animations
	IEnumerator doDeathAnimation(float time, bool markAsPlayedWhenDone){
		doingAnimation = true;

		float timer = time;
		float startScale = transform.localScale.x;

		while (timer > 0) {
			timer -= Time.deltaTime;
			timer = Mathf.Max (0, timer);

			float newScale = timer / time;
			newScale = Mathf.Pow (newScale, 2);
			transform.localScale = new Vector3 (newScale, newScale, newScale);

			yield return null;
		}


		transform.localScale = new Vector3 (1, 1, 1);	

		if (markAsPlayedWhenDone) {
			owner.markCardPlayed (this);
		}

		deck.discardCardFromHand (this);

	}


	//setters getters

	public bool MouseIsOver{
		get{
			return this.mouseIsOver;
		}
	}

	public bool IsDisabled{
		get{
			return this.isDisabled;
		}
	}

	public bool WaitingForTile{
		get{
			return this.waitingForTile;
		}
		set{
			waitingForTile = value;
		}
			
	}
	public bool WaitingForUnit{
		get{
			return this.waitingForUnit;
		}
		set{
			waitingForUnit = value;
		}
	}
	public bool WaitingForChoice{
		get{
			return this.waitingForChoice;
		}
		set{
			waitingForChoice = value;
		}
	}

	public Unit Owner {
		get {
			return this.owner;
		}
	}

	public bool IsActive{
		get{
			return this.isActive;
		}
		set{
			isActive = value;
		}
	}

	public bool DoingAnimation{
		get{
			return this.doingAnimation;
		}
	}
}
