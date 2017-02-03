using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

	public string unitName;

	private GameManager gm;

	public bool isPlayerControlled;

	private Tile curTile;

	public int baseHealth;
	private int health;

	public int baseHandSize;

	private bool isActive;

	public int baseActions;
	private int actionsLeft;

	public SpriteRenderer spriteRend;
	public Text healthText;

	private bool doingAnimation;

	public GameObject handPrefab, deckPrefab;
	[System.NonSerialized]
	public Hand hand;
	[System.NonSerialized]
	public Deck deck;

	private bool mouseIsOver;

	private bool isHighlighted;

	public void setup(GameManager _gm, int startX, int startY){
		gm = _gm;
		curTile = gm.board.Grid [startX, startY];
		transform.position = curTile.transform.position;

		health = baseHealth;

		//spawn deck
		GameObject deckObj = Instantiate (deckPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		deckObj.gameObject.name = unitName + "_deck";
		deck = deckObj.GetComponent<Deck> ();

		//spawn hand
		GameObject handObj = Instantiate (handPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		handObj.gameObject.name = unitName + "_hand";
		hand = handObj.GetComponent<Hand> ();

		//set them up
		deck.setup (this);
		hand.setup (this);

		setHighlighted (false);

		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void reset(){
		//draw first hand
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}
		actionsLeft = 0;

		healthText.text = "HP: " + health;
	}

	public void resetRound(){
		actionsLeft = baseActions;
		setActive (false);
	}

	public void setActive(bool _isActive){
		isActive = _isActive;
		hand.setActive (isActive);
	}

	// Update is called once per frame
	void Update () {
		//bounce the sprite when it is active
		float spriteScale = 1;
		if (isActive) {
			spriteScale = 1.0f + Mathf.Abs(Mathf.Sin (Time.time * 1) * 0.2f);
		}
		spriteRend.gameObject.transform.localScale = new Vector3 (spriteScale, spriteScale, spriteScale);
	}

	//ending the turn
	public void endTurn(){
		//clear remaining actions
		actionsLeft = 0;
		//discard the hand
		hand.discardHand();
		//draw to hand size
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}
	}

	//playing a cards
	public void markCardPlayed(Card card){
		actionsLeft--;
		hand.markCardPlayed (card);
	}

	//input
	public void checkGeneralClick(){
		if (mouseIsOver && isHighlighted) {
			gm.unitClicked (this);
		}
	}

	public void checkActiveClick(){
		hand.checkClick ();
	}
		

	//movement
	public void moveTo(Tile target){
		curTile = target;
		StartCoroutine(doMoveAnimation(target.transform.position, 0.5f));
	}

	//damage and health
	public void takeDamage(int amount){
		health -= amount;
		healthText.text = "HP: " + health;
		//Debug.Log (unitName + " has " + health + " health");

		if (health <= 0) {
			killUnit ();
		}
	}

	public void killUnit(){
		StartCoroutine(doDeathAnimation(0.5f));
	}


	//animations
	IEnumerator doMoveAnimation(Vector3 target, float time){
		doingAnimation = true;

		Vector3 startPos = transform.position;
		float timer = 0;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			prc = Mathf.Pow (prc, 0.75f);
			transform.position = Vector3.Lerp (startPos, target, prc);
			yield return null;
		}

		doingAnimation = false;
		transform.position = target;
	}

	IEnumerator doDeathAnimation(float time){
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

		Destroy (gameObject);
		gm.removeUnit (this);

	}

	//highlighting
	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		if (isHighlighted) {
			spriteRend.color = col;
		} else {
			spriteRend.color = new Color (1, 1, 1);
		}
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}

	//utility

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}


	//stters getters

	public Tile CurTile{
		get{
			return this.curTile;
		}
	}

	public GameManager GM{
		get{
			return this.gm;
		}
	}

	public float ActionsLeft{
		get{
			return this.actionsLeft;
		}
	}


	public bool DoingAnimation{
		get{
			return this.doingAnimation;
		}
	}

	public bool IsActive{
		get{
			return this.isActive;
		}
	}

	public bool IsHighlighted{
		get{
			return this.isHighlighted;
		}
	}
}
