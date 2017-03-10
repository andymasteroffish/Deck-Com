using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

public class Unit {

	//some basic info
	public string unitName;

	private GameManager gm;

	public bool isPlayerControlled;

	private Tile curTile;
	private bool isActive;
	private bool turnIsDone;

	//stats
	public int baseHealth = 5;
	public int health;
	public bool isDead;

	public int baseHandSize = 4;

	public int baseActions = 2;
	private int actionsLeft;

	//display
	private bool isHighlighted;
	public Color highlightCol;
	public Sprite sprite;

	//decks and cards
	public Deck deck;

	public string deckListPath;

	//weapons and charms
	private List<string> charmIDs;
	private Charm weapon;
	private List<Charm> charms = new List<Charm>();

	//utility
	public bool mouseIsOver;

	public Unit(XmlNode node){
		unitName = node ["name"].InnerXml;
		isPlayerControlled = bool.Parse(node["player_controlled"].InnerXml);

		deckListPath = Application.dataPath + "/external_data/";
		deckListPath += isPlayerControlled ? "player/" : "foes/";
		deckListPath += "decks/";
		deckListPath += node ["deck"].InnerXml + ".txt";

		sprite = Resources.Load<Sprite> (node ["sprite"].InnerXml);

		if (node ["base_health"] != null) {
			baseHealth = int.Parse(node ["base_health"].InnerText);
		}
		if (node ["hand_size"] != null) {
			baseHandSize = int.Parse(node ["hand_size"].InnerText);
		}
		if (node ["actions"] != null) {
			baseActions = int.Parse(node ["actions"].InnerText);
		}


		charmIDs = new List<string> ();
		XmlNodeList childNodes = node["charms"].ChildNodes;
		foreach (XmlNode n in childNodes) {
			if (n.Name == "charm") {
				charmIDs.Add (n.InnerXml);
			}
		}
	}

	public void setup(GameManager _gm, Tile startTile){
		gm = _gm;
		curTile = startTile;

		health = baseHealth;
		isDead = false;

		//spawn deck
		deck = new Deck();

		for (int i = 0; i < charmIDs.Count; i++) {
			addCharm (charmIDs [i]);
		}

		//the first item is the weapon
		weapon = charms[0];

		//set them up
		deck.setup (this, deckListPath);

		setHighlighted (false);

		setupCustom ();

		//create the game object shell
		GameObjectManager.instance.getUnitGO().activate(this);
	}
	public virtual void setupCustom(){}

	public void addCharm(string idName){
		Charm thisCharm = CharmManager.instance.getCharmFromIdName (idName);
		thisCharm.setup (this, true, idName);
		if (isActive) {
			thisCharm.setActive (true);
		}
		charms.Add (thisCharm);
	}

	public void removeCharm(Charm charmToRemove){
		charmToRemove.isDead = true;
		charms.Remove (charmToRemove);

		//space out the others
		for (int i = 0; i < charms.Count; i++) {
			charms [i].offsetID = i;
		}
	}

	public void reset(){
		//draw first hand
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}
		actionsLeft = 0;
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
		if (isActive) {
			GameObjectManager.instance.getActionMarkerGO ().activate (this, 1);
		}
	}


	//ending the turn
	public void endTurn(){
		turnIsDone = true;

		for (int i=charms.Count-1; i>=0; i--){
			charms[i].turnEndPreDiscard ();
		}

		//clear remaining actions
		actionsLeft = 0;
		//discard the hand
		deck.discardHand();

		//draw to hand size
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}

		for (int i=charms.Count-1; i>=0; i--){
			charms[i].turnEndPostDiscard ();
		}

		if (isPlayerControlled) {
			gm.tabActivePlayerUnit (1);
		} else {
			gm.tabActiveAIUnit (1);
		}
	}

	//playing a cards
	public void markCardPlayed(Card card){
		//check if any item does something
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].cardPlayed (card);
		}

		//reduce the actions
		actionsLeft -= card.getNumActionsNeededToPlay();

		//check which cards can still be played
		deck.updateCardsDisabled();
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
		isDead = true;
		gm.removeUnit (this);
		//StartCoroutine(doDeathAnimation(0.5f));
	}

	//other effects
	public void gainActions(int num){
		actionsLeft += num;
	}

	//highlighting
	public void setHighlighted(bool val, Color col){
//		if (outline == null) {
//			createOutlineObj ();
//		}

		isHighlighted = val;
		highlightCol = col;
//		if (isHighlighted) {
//			outline.turnOn (col);
//			//spriteRend.color = col;
//		} else {
//			outline.turnOff ();
//			//spriteRend.color = new Color (1, 1, 1);
//		}
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}



	//utility

//	void OnMouseEnter(){
//		mouseIsOver = true;
//		if (isHighlighted) {
//			GM.ActiveCard.potentialTargetMouseOver (this);
//		}
//	}
//	void OnMouseExit(){
//		mouseIsOver = false;
//		GM.targetInfoText.unitRollOff(this);
//	}


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

	public Charm Weapon{
		get{
			return this.weapon;
		}
	}

	public List<Charm> Charms{
		get{
			return this.charms;
		}
	}
}
