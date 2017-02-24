using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Unit : MonoBehaviour {

	//some basic info
	public string unitName;

	private GameManager gm;

	public bool isPlayerControlled;

	private Tile curTile;
	private bool isActive;
	private bool turnIsDone;

	//stats
	public int baseHealth;
	private int health;

	public int baseHandSize;

	public int baseActions;
	private int actionsLeft;

	//display
	public SpriteRenderer spriteRend;

	public GameObject canvasPrefab;
	private Text healthText;

	public GameObject spriteOutlinePrefab;
	private SpriteOutline outline = null;

	private bool isHighlighted;

	private bool doingAnimation;

	//decks and cards
	public GameObject deckPrefab;
	[System.NonSerialized]
	public Deck deck;

	//weapons and charms
	public GameObject[] itemPrefabs;
	private Item weapon;
	private List<Item> charms = new List<Item>();

	//utility
	private bool mouseIsOver;


	public void setup(GameManager _gm,Tile startTile){
		gm = _gm;
		curTile = startTile;
		transform.position = curTile.transform.position;

		health = baseHealth;

		//spawn the canvas for displaying info
		GameObject canvasObj = Instantiate(canvasPrefab, transform.position, Quaternion.identity) as GameObject;
		canvasObj.transform.parent = transform;
		healthText = canvasObj.GetComponentInChildren<Text> ();

		//spawn deck
		GameObject deckObj = Instantiate (deckPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		deckObj.gameObject.name = unitName + "_deck";
		deck = deckObj.GetComponent<Deck> ();

		//spawn the items
		for (int i = 0; i < itemPrefabs.Length; i++) {
			addCharm (itemPrefabs [i]);
		}

		//the first item is the weapon
		weapon = charms[0];

		//set them up
		deck.setup (this);
		//hand.setup (this);

		setHighlighted (false);

		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void addCharm(GameObject charmPrefab){
		GameObject itemObj = Instantiate (charmPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		Item thisItem =  itemObj.GetComponent<Item> ();
		thisItem.setup (this, charms.Count);
		itemObj.name = unitName + "_" + thisItem.name;
		thisItem.setActive (isActive);
		charms.Add (thisItem);
	}

	public void removeCharm(Item charmToRemove){
		//kill this one
		Destroy (charmToRemove.gameObject);
		charms.Remove (charmToRemove);

		//space out the others
		for (int i = 0; i < charms.Count; i++) {
			charms[i].setPos (i);
		}
	}

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
		turnIsDone = false;
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].resetRound ();
		}
	}

	public void setActive(bool _isActive){
		isActive = _isActive;
		deck.setActive (isActive);
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].setActive (isActive);
		}
	}

	// Update is called once per frame
	void Update () {
		//bounce the sprite when it is active
		float spriteScale = 1;
		if (isActive && !turnIsDone) {
			spriteScale = 1.0f + Mathf.Abs(Mathf.Sin (Time.time * 2) * 0.1f);
		}
		spriteRend.gameObject.transform.localScale = new Vector3 (spriteScale, spriteScale, spriteScale);

		//greying when turn is done
		if (!isHighlighted) {
			Color colThisFrame = new Color (1, 1, 1, 1);
			if (turnIsDone && gm.IsPlayerTurn == isPlayerControlled) {
				colThisFrame = new Color (0.3f, 0.3f, 0.3f, 0.5f);
			}
			spriteRend.color = colThisFrame;
		}

		//show health
		healthText.text = "HP: " + health+"/"+baseHealth;

		//keeping them raised up for clicking
		transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
	}

	//ending the turn
	public void endTurn(){
		StartCoroutine (doEndTurn ());
	}

	IEnumerator doEndTurn(){
		doingAnimation = true;
		turnIsDone = true;

		for (int i=charms.Count-1; i>=0; i--){
			charms[i].turnEndPreDiscard ();
		}

		//clear remaining actions
		actionsLeft = 0;
		//discard the hand
		deck.discardHand();

		yield return new WaitForSeconds (0.5f * gm.debugAnimationTimeMod);

		//draw to hand size
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}

		while (deck.areAnimationsHappening ()) {
			yield return null;
		}

		yield return new WaitForSeconds (1f * gm.debugAnimationTimeMod);	//a second to see the new cards

		for (int i=charms.Count-1; i>=0; i--){
			charms[i].turnEndPostDiscard ();
		}

		yield return new WaitForSeconds (0.5f * gm.debugAnimationTimeMod);

		if (isPlayerControlled) {
			gm.tabActivePlayerUnit ();
		} else {
			gm.tabActiveAIUnit ();
		}

		doingAnimation = false;
	}

	//playing a cards
	public void markCardPlayed(Card card){
		//check if any item does something
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].cardPlayed (card);
		}

		//reduce the actions
		actionsLeft -= card.getNumActionsNeededToPlay();
	}

	//input
	public void checkGeneralClick(){
		if (mouseIsOver && isHighlighted) {
			gm.unitClicked (this);
		}
	}

	public void checkActiveClick(){
		deck.checkClick ();
	}
		

	//movement
	public void moveTo(Tile target){
		curTile = target;
		StartCoroutine(doMoveAnimation(target.transform.position, 0.5f));
	}

	//damage and health
	public void heal(int amount){
		health += amount;
		if (health > baseHealth) {
			health = baseHealth;
		}
	}

	public void takeDamage(int amount, Card card, Unit source){
		int totalDamage = amount;

		//check if any charms will do anything
		for (int i = charms.Count - 1; i >= 0; i--) {
			totalDamage += charms [i].getDamageTakenMod (card, source);
		} 


		//if modifiers would cause the unit to gain life, they instead just take 0 damage
		if (totalDamage < 0) {
			totalDamage = 0;
		}

		//do the damage
		health -= totalDamage;

		//check if any charms trigger on taking damage
		for (int i = charms.Count - 1; i >= 0; i--) {
			charms [i].takeDamage (card, source);
		} 


		//is the unit dead?
		if (health <= 0) {
			killUnit ();
		}

	}

	public void killUnit(){
		StartCoroutine(doDeathAnimation(0.5f));
	}

	//other effects
	public void gainActions(int num){
		actionsLeft += num;
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

		float timer = time * gm.debugAnimationTimeMod;
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

	public bool areAnimationsHappening(){
		if (doingAnimation){
			return true;
		}

		if (deck.areAnimationsHappening()){
			return true;
		}

		return false;
	}

	//highlighting
	public void setHighlighted(bool val, Color col){
		if (outline == null) {
			createOutlineObj ();
		}

		isHighlighted = val;
		if (isHighlighted) {
			outline.turnOn (col);
			//spriteRend.color = col;
		} else {
			outline.turnOff ();
			//spriteRend.color = new Color (1, 1, 1);
		}
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}

	private void createOutlineObj(){
		GameObject outlineObj = Instantiate(spriteOutlinePrefab, spriteRend.gameObject.transform.position, Quaternion.identity) as GameObject;
		outlineObj.transform.parent = spriteRend.gameObject.transform;
		outline = outlineObj.GetComponent<SpriteOutline> ();
		outline.setup ();
	}

	//utility

	void OnMouseEnter(){
		mouseIsOver = true;
		if (isHighlighted) {
			GM.ActiveCard.potentialTargetMouseOver (this);
		}
	}
	void OnMouseExit(){
		mouseIsOver = false;
		GM.targetInfoText.unitRollOff(this);
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

	public bool TurnIsDone{
		get{
			return this.turnIsDone;
		}
	}

	public bool IsHighlighted{
		get{
			return this.isHighlighted;
		}
	}

	public Item Weapon{
		get{
			return this.weapon;
		}
	}

	public List<Item> Charms{
		get{
			return this.charms;
		}
	}
}
