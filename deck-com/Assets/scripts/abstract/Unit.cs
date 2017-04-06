using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;

public class Unit {

	//some basic info
	public string unitName;

	private GameManager gm;
	public Board board;

	public bool isPlayerControlled;

	private Tile curTile;
	private bool isActive;
	public bool isExausted;	//meaning 0 actions left & no cards that can be played

	public bool useGO;

	//stats
	public int baseHealth = 6;
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

	//line of sight
	public float sightRange;
	public List<Tile> visibleTiles = null;

	//loot
	private int challengeLevel;
	private bool canPickUpLoot;

	//utility
	public bool mouseIsOver;

	//some stuff for AI work
	public string aiProfileName;
	public AIProfile aiProfile;

	public bool isAISimUnit;
	public bool isActingAIUnit;		//gets cards and deck when an AI board is made
	public int  aiHandSize;
	public int aiSimHasBeenAidedCount;

	public TurnInfo aiTurnInfo;
	public int curAITurnStep;

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

		challengeLevel = 1;
		if (node ["level"] != null) {
			challengeLevel = int.Parse(node ["level"].InnerText);
		}

		aiProfileName = "none";
		if (node ["ai_profile"] != null) {
			aiProfileName = node ["ai_profile"].InnerText;
		}

		sightRange = 8;

		charmIDs = new List<string> ();
		XmlNodeList childNodes = node["charms"].ChildNodes;
		foreach (XmlNode n in childNodes) {
			if (n.Name == "charm") {
				charmIDs.Add (n.InnerXml);
			}
		}

		useGO = true;

		isActingAIUnit = false;
		isAISimUnit = false;
	}

	public void setup(GameManager _gm, Board _board, Tile startTile){
		gm = _gm;
		board = _board;
		curTile = startTile;

		health = baseHealth;
		isDead = false;

		canPickUpLoot = false;

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

		//aiProfile might reference the weapon, so we should make it after setting the charms
		aiProfile = new AIProfile (this, aiProfileName);	//this should load from XML

		//create the game object shell
		if (useGO) {
			GameObjectManager.instance.getUnitGO ().activate (this);
		}
	}

	//creating a duplicate unit for AI
	public Unit(Unit parent, Board _board, Tile _curTile){
		gm = parent.gm;
		board = _board;
		curTile = _curTile;

		useGO = false;

		unitName = parent.unitName;

		isActingAIUnit = parent.isActingAIUnit;
		aiProfile = parent.aiProfile;
		isAISimUnit = true;

		isPlayerControlled = parent.isPlayerControlled;
		sprite = null;

		baseHealth = parent.baseHealth;
		health = parent.health;
		isDead = parent.isDead;

		challengeLevel = parent.ChallengeLevel;
		canPickUpLoot = parent.canPickUpLoot;

		baseHandSize = parent.baseHandSize;

		if (!isActingAIUnit) {
			deck = null;
			aiHandSize = parent.aiHandSize;
			if (parent.deck != null) {
				aiHandSize = parent.deck.Hand.Count;
			}
		} else {
			deck = new Deck (parent.deck, this);
		}

		baseActions = parent.baseActions;
		actionsLeft = parent.actionsLeft;

//		visibleTiles = new List<Tile> ();
//		for (int i = 0; i < parent.visibleTiles.Count; i++) {
//			Tile tile = board.Grid [parent.visibleTiles [i].Pos.x, parent.visibleTiles [i].Pos.y];
//			visibleTiles.Add (tile);
//		}

		//this is ugly
		charms = new List<Charm> ();
		for (int i = 0; i < parent.charms.Count; i++) {
			addCharm (parent.charms [i].idName);
		}
		weapon = charms[0];

		isHighlighted = parent.isHighlighted;
		highlightCol = parent.highlightCol;

	}

	public void addCharm(string idName){
		Charm thisCharm = CharmManager.instance.getCharmFromIdName (idName);
		thisCharm.setup (this, useGO, idName);
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
		getVisibleTiles ();
	}

	public void resetRound(){
		actionsLeft = baseActions;
		setActive (false);
		isExausted = false;
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].resetRound ();
		}
	}

	public void resetAISimFlags(){
		aiSimHasBeenAidedCount = 0;
	}

	public void setActive(bool _isActive){
		isActive = _isActive;
		deck.setActive (isActive);
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].setActive (isActive);
		}
		if (isActive) {
			GameObjectManager.instance.getActionMarkerGO ().activate (this, 1);
			//if this is the player, maybe there is delicious loot!
			if (isPlayerControlled){
				canPickUpLoot = board.checkIfUnitIsCloseToLoot (this);
			}
		}

		if (!isPlayerControlled && isActive){
			gm.resetAllAIFlags ();
			aiTurnInfo = gm.getAIMove(board.getUnitID(this), board, board, 0);
			curAITurnStep = 0;	//flag to hlp the display interface
		}
	}

	//line of sight
	void getVisibleTiles(){
		if (visibleTiles == null) {
			visibleTiles = new List<Tile> ();
		}
		visibleTiles.Clear ();
		visibleTiles = board.getTilesInVisibleRange (curTile, sightRange);
		board.updateVisible ();
	}


	//ending the turn
	public void endTurn(){
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

//		if (isPlayerControlled) {
//			gm.tabActivePlayerUnit (1);
//		} else {
//			gm.tabActiveAIUnit (1);
//		}
	}

	public void turnEndCleanUp(){
		isActingAIUnit = false;
		if (isDead){
			//Debug.Log ("clean up dead " + unitName);
			board.removeUnit (this);
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

		//Debug.Log ("I just played " + card.name + " and have " + actionsLeft + " actions left");

		//check which cards can still be played
		deck.updateCardsDisabled();
		checkExausted ();

		//if this is the player, maybe there is delicious loot!
		if (isPlayerControlled){
			canPickUpLoot = board.checkIfUnitIsCloseToLoot (this);
		}
	}

	//a unit is exausted if they have no acitons and no cards they can play
	public void checkExausted(){
		isExausted = false;

		if (actionsLeft <= 0 && deck.isWholeHandDisabled()){
			isExausted = true;
		}
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

	public void pickUpLoot(){
		board.collectLootNearUnit (this);
	}
		

	//movement
	public void moveTo(Tile target){
		curTile = target;
		getVisibleTiles ();
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
		board.checksWhenUnitDies (this);
	}

	//other effects
	public void gainActions(int num){
		//give the action(s)
		actionsLeft += num;
		//throw in an action marker. If we don't need it it will be removed
		GameObjectManager.instance.getActionMarkerGO ().activate (this, 1);
		//check which cards can be played
		deck.updateCardsDisabled();
		checkExausted ();

		Debug.Log ("actions: " + actionsLeft);
	}

	//highlighting
	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		highlightCol = col;
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}

	//saving and cleaning up at the end of the game

	public void endGame(){
		//move all cards to the deck
		deck.discardHand();
		deck.putDiscardInDrawPile ();
	}

	public List<Card_Loot> syphonLoot(){
		return deck.syphonLootFromDrawPile ();
	}

	public void saveDeckFile(){
		deck.saveDrawPile (deckListPath);
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

	public int ActionsLeft{
		get{
			return this.actionsLeft;
		}
		set{
			actionsLeft = value;
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

	public bool CanPickupLoot{
		get{
			return this.canPickUpLoot;
		}
		set{
			canPickUpLoot = value;
		}
	}

	public int ChallengeLevel{
		get{
			return this.challengeLevel;
		}
	}
}
