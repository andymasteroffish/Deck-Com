using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Xml;
using UnityEngine.Profiling;

public class Unit {

	//some basic info
	public string unitName;

	private GameManager gm;
	public Board board;

	public bool isPlayerControlled;
	public bool isAwake;
	public List<Unit> podmates;

	private Tile curTile;
	private bool isActive;
	public bool isExausted;	//meaning 0 actions left & no cards that can be played or the unit is not awake

	public bool useGO;

	//stats
	public int baseHealth = 6;
	public int health;
	public bool isDead;

	public int baseHandSize = 4;	//PUT THIS BACK TO 4

	public int baseActions = 2;
	private int actionsLeft;

	//display
	private bool isHighlighted;
	public Color highlightCol;
	public Sprite sprite;
	public bool mouseColliderIsActive;

	public bool showVisibilityIcon;

	//decks and cards
	public Deck deck;
	public List<Card> cardsPlayedThisTurn;

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
	public int aiSimHasBeenCursedCount;

	public TurnInfo aiTurnInfo;
	public int curAITurnStep;

	public Tile.Cover oldLowestCoverForAISim;

	public Unit(){
	}
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
			//Debug.Log ("set it " + baseHandSize);
			baseHandSize = int.Parse(node ["hand_size"].InnerText);
		}
		if (node ["actions"] != null) {
			baseActions = int.Parse(node ["actions"].InnerText);
		}

		challengeLevel = 1;
		if (node ["cr"] != null) {
			challengeLevel = int.Parse(node ["cr"].InnerText);
		}

		aiProfileName = "none";
		if (node ["ai_profile"] != null) {
			aiProfileName = node ["ai_profile"].InnerText;
		}

		sightRange = 9;	//this was 9

		charmIDs = new List<string> ();
		XmlNodeList childNodes = node["charms"].ChildNodes;
		foreach (XmlNode n in childNodes) {
			if (n.Name == "charm") {
				charmIDs.Add (n.InnerXml);
			}
		}

		useGO = true;
		mouseColliderIsActive = true;

		isActingAIUnit = false;
		isAISimUnit = false;

		podmates = new List<Unit>();
	}

	public void setup(GameManager _gm, Board _board, Tile startTile){
		gm = _gm;
		board = _board;
		curTile = startTile;

		if (board.isAISim) {
			useGO = false;
		}

		health = baseHealth;
		isDead = false;

		canPickUpLoot = false;

		isAwake = isPlayerControlled;

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
		setAISimUnitFromParent(parent, _board, _curTile);
	}
	public void setAISimUnitFromParent(Unit parent, Board _board, Tile _curTile){
		Profiler.BeginSample("Unit Creation");
		gm = parent.gm;
		board = _board;
		curTile = _curTile;

		useGO = false;

		unitName = parent.unitName;

		isActingAIUnit = parent.isActingAIUnit;
		aiProfile = parent.aiProfile;
		isAISimUnit = true;

		isPlayerControlled = parent.isPlayerControlled;
		isAwake = parent.isAwake;
		//PODMATES ARE IGNORED FOR NOW
		Profiler.BeginSample("making empty pod list");
		podmates = new List<Unit>();
		Profiler.EndSample ();

		sprite = null;

		baseHealth = parent.baseHealth;
		health = parent.health;
		isDead = parent.isDead;

		challengeLevel = parent.ChallengeLevel;
		canPickUpLoot = parent.canPickUpLoot;

		baseHandSize = parent.baseHandSize;

		Profiler.BeginSample ("making that deck");
		if (!isActingAIUnit) {
			Profiler.BeginSample ("making null deck");
			deck = null;
			aiHandSize = parent.aiHandSize;
			if (parent.deck != null) {
				aiHandSize = parent.deck.Hand.Count;
			}
			Profiler.EndSample ();
		} else {
			deck = new Deck (parent.deck, this);
		}
		Profiler.EndSample ();

		baseActions = parent.baseActions;
		actionsLeft = parent.actionsLeft;

		sightRange = parent.sightRange;

		Profiler.BeginSample("Set charms");
		charms = new List<Charm> ();
		for (int i = 0; i < parent.charms.Count; i++) {
			charms.Add(CharmManager.instance.getCharmFromParent (parent.charms[i]));
			//Debug.Log ("test add: " + charms [i].idName + " to " + unitName);
		}
		weapon = charms[0];
		Profiler.EndSample ();

		isHighlighted = parent.isHighlighted;
		highlightCol = parent.highlightCol;

		aiSimHasBeenAidedCount = parent.aiSimHasBeenAidedCount;
		aiSimHasBeenCursedCount = parent.aiSimHasBeenCursedCount;

		Profiler.EndSample ();
	}

	public Charm addCharm(string idName){
		Charm thisCharm = CharmManager.instance.getCharmFromIdName (idName);
		thisCharm.setup (this, useGO, idName);
		if (isActive) {
			thisCharm.setActive (true);
		}
		charms.Add (thisCharm);
		return thisCharm;
	}

	public void removeCharm(Charm charmToRemove){
		//Debug.Log ("tim to remov "+charmToRemove.idName);
		charmToRemove.isDead = true;

		//if there is a card, discard it
		if (charmToRemove.storedCard != null) {
			Debug.Log ("ad charm dies, discard " + charmToRemove.storedCard);
			charmToRemove.storedCard.discard ();
		}

		//get it out
		charms.Remove (charmToRemove);

		//space out the others
		for (int i = 0; i < charms.Count; i++) {
			charms [i].offsetID = i;
			charms [i].hasChangedPos = true;
		}
	}

	public void reset(){
		//draw first hand
		int drawSize = baseHandSize;
		for (int i = 0; i < charms.Count; i++) {
			drawSize += charms [i].getHandSizeMod ();
		}
		for (int i = 0; i < baseHandSize; i++) {
			deck.drawCard ();
		}
		actionsLeft = 0;
		setVisibleTiles ();

		//turn-to-turn info
		if (cardsPlayedThisTurn == null) {
			cardsPlayedThisTurn = new List<Card> ();
		}
		cardsPlayedThisTurn.Clear ();
	}

	public void resetRound(){
		actionsLeft = baseActions;
		setActive (false);
		checkExausted ();
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].resetRound ();
		}
	}

	public void markAIStart(){
		//Debug.Log ("AI START");
		//reset all sim flags
		aiSimHasBeenAidedCount = 0;
		aiSimHasBeenCursedCount = 0;
		//tell all existing charms they are in AI Sim so they don't get removed or affected
		foreach (Charm charm in charms) {
			charm.protectDuringAISim = true;
		}
	}
	public void markAIEnd(){
		foreach (Charm charm in charms) {
			charm.protectDuringAISim = false;
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
			//if this is the player, maybe there is delicious loot!
			if (isPlayerControlled){
				canPickUpLoot = board.checkIfUnitIsCloseToLoot (this);
			}
		}

		//time to get the AI move?
		if (!isPlayerControlled && isActive && !gm.IsPlayerTurn){
			if (isAwake) {
				gm.markAIStart ();
				aiTurnInfo = gm.getAIMove (board.getUnitID (this), board, board, 0);
				//ObjectPooler.instance.printInfo ();
				curAITurnStep = 0;	//flag to hlp the display interface
				gm.markAIEnd();
			} else {
				Debug.Log ("not awake");
			}
		}
	}

	//line of sight
	public void setVisibleTiles(){
		Profiler.BeginSample ("set visible tiles");
		if (visibleTiles == null) {
			visibleTiles = new List<Tile> ();
		}
		visibleTiles.Clear ();
		visibleTiles = board.getTilesInVisibleRange (curTile, sightRange);
		if (isPlayerControlled && !isAISimUnit) {
			Debug.Log ("set vis for " + unitName + " with sight " + sightRange+ " on frame "+Time.frameCount);
			board.updateVisible ();
		}
		Profiler.EndSample ();
	}

	public void wakeUp(){
		//Debug.Log ("wake up " + unitName);
		isAwake = true;
		foreach (Unit mate in podmates) {
			if (!mate.isAwake) {
				mate.wakeUp ();
			}
		}
		//Debug.Log ("I'm awakw on " + curTile.Pos.x + " , " + curTile.Pos.y);
	}

	public bool getIsVisibleToPlayer(){
		return curTile.isVisibleToPlayer;
	}

	//ending the turn
	public void endTurn(){
		for (int i=charms.Count-1; i>=0; i--){
			charms[i].turnEndPreDiscard ();
		}

		//clear remaining actions
		actionsLeft = 0;
		deck.updateCardsDisabled ();
		checkExausted ();
		//discard the hand
		deck.discardHand();

		//draw to hand size
		int drawSize = baseHandSize;
		for (int i = 0; i < charms.Count; i++) {
			drawSize += charms [i].getHandSizeMod ();
		}
		for (int i = 0; i < drawSize; i++) {
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

		//add this to the list
		if (cardsPlayedThisTurn == null) {
			cardsPlayedThisTurn = new List<Card> ();
		}
		cardsPlayedThisTurn.Add(card);

		//reduce the actions
		//Debug.Log(card.idName+" has action cost "+card.getNumActionsNeededToPlay());
		//Debug.Log ("and I start with " + actionsLeft + " actions");
		actionsLeft -= card.getNumActionsNeededToPlay();
		//Debug.Log ("Now I have " + actionsLeft + " actions");

		//Debug.Log ("I just played " + card.name + " and have " + actionsLeft + " actions left");

		//if this is the player, maybe there is delicious loot!
		if (isPlayerControlled){
			canPickUpLoot = board.checkIfUnitIsCloseToLoot (this);
		}

		//check which cards can still be played
		deck.updateCardsDisabled();
		checkExausted ();



		//if this unit is player controlled and exausted, it's time to to tab on
		if (isExausted && gm.activePlayerUnit == this) {
			GameManagerTacticsInterface.instance.triggerTabAfterAnimations ();
		}
	}

	//a unit is exausted if they have no acitons and no cards they can play
	public void checkExausted(){
		isExausted = false;

		if (actionsLeft <= 0 && deck.isWholeHandDisabled() && !canPickUpLoot){
			isExausted = true;
		}

		if (!isAwake) {
			isExausted = true;
		}
	}

	//input
	public void checkGeneralClick(){
		if (mouseIsOver && isHighlighted) {
			gm.unitClicked (this);
			return;
		}
		if (isPlayerControlled && gm.activePlayerUnit != this && mouseIsOver && gm.activeCard == null) {
			gm.setActivePlayerUnit (this);
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
		//trying out only updating visible tiles on the root board.
		//this means that AI units will do their entire turn based on what they could see at the start of theri turns
		if (!board.isAISim) {
			setVisibleTiles ();
		}
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

		//Debug.Log ("starting damage " + amount);

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

		//Debug.Log (unitName+ " got hit for " + amount);

	}

	public void killUnit(){
		isDead = true;
		clearAllEquipment ();
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
	}

	//highlighting
	public void setHighlighted(bool val, Color col){
		isHighlighted = val;
		highlightCol = col;
	}
	public void setHighlighted(bool val){
		setHighlighted (val, Color.white);
	}

	public void setMouseColliderActive(bool newVal){
		mouseColliderIsActive = newVal;
	}

	//getting some info
	public int getCardsOfTypePlayedThisTurn(Card.CardType type){
		if (cardsPlayedThisTurn == null) {
			return 0;
		}
		int total = 0;
		foreach (Card card in cardsPlayedThisTurn) {
			if (card.type == type) {
				total++;
			}
		}
		return total;
	}

	//saving and cleaning up at the end of the game

	public void endGame(){
		//turn any equipment back into cards
		clearAllEquipment();

		//move all cards to the deck
		deck.discardHand();
		deck.putDiscardInDrawPile ();
	}

	public List<Card_Loot> syphonLoot(){
		return deck.syphonLootFromDrawPile ();
	}

	public void clearAllEquipment(){
		for (int i = charms.Count - 1; i >= 0; i--) {
			if (charms [i] != weapon) {
				removeCharm (charms [i]);
			}
		}
	}

	public void saveDeckFile(){
		deck.saveDrawPile (deckListPath);
	}

	//clean up
	public void returnToPool(){
		ObjectPooler.instance.retireUnit (this);
	}

	//setters getters

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