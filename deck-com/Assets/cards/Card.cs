using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Profiling;

public class Card : IComparable<Card> {

	public enum CardType{Loot, Movement, Skill, Attack, AttackSpecial, Spell, Equipment, Other};

	public string name, idName;
	public string description;
	public int cardLevel;

	public XmlNode node;
	public string scriptName;
	public Card blueprint;

	public CardType type;
	public int baseActionCost;

	private bool isActive;
	private bool waitingForTile, waitingForUnit, waitingForChoice;

	public bool mouseIsOver;
	private bool isDisabled;

	public int orderInHand;

	private Unit owner;
	private Deck deck;

	//shit for spells
	public int manaCost;	//how many other spells need to be in hand to play this

	//bonus things the card might give to the unit that plays it
	public int bonusActions;
	public int bonusCards;
	public int bonusHeal;
	public string bonusCardID;

	public bool selfDestructWhenPlayed;

	//modifying cost
	public int numMoveCardsPlayedToMakeThisFree;

	//ignoring charms
	public bool ignoreTargetCharms;
	public bool ignoreCasterCharms;

	//adding the card to a deck
	private int costToAddToDeck;

	//some state shit
	public bool isDead;

	//some colors
	public Dictionary<CardType, Color> highlightColors;
	public Color baseHighlightColor;

	public bool showVisibilityIconsWhenHighlighting;

	//this constructor should not be used. The derived cards are the only thing that should be used
	public Card(){}

	//a few flags for display
	public bool revealAICardFlag;

	public void setupBlueprint(){
		
		idName = node.Attributes ["idName"].Value;

		scriptName = node ["script"].InnerText;
		name = node ["name"].InnerText;

		cardLevel = int.Parse(node ["level"].InnerText);
		if (cardLevel < 0) {
			cardLevel = 0;
		}

		//default values
		type = CardType.Other;
		if (node ["card_type"] != null) {
			type = (CardType) Enum.Parse(typeof(CardType), node ["card_type"].InnerText);  
		}

		baseActionCost = 1;
		if (node ["action_cost"] != null) {
			baseActionCost = int.Parse(node ["action_cost"].InnerText);
		}

		costToAddToDeck = 2 * cardLevel;
		if (node ["add_to_deck_cost"] != null) {
			costToAddToDeck = int.Parse(node ["add_to_deck_cost"].InnerText);
		}

		manaCost = 0;
		if (node ["mana_cost"] != null) {
			manaCost = int.Parse(node ["mana_cost"].InnerText);
		}

		//any bonuses?
		bonusActions = 0;
		if (node ["bonus_actions"] != null) {
			bonusActions = int.Parse (node ["bonus_actions"].InnerText);
		}

		bonusCards = 0;
		if (node ["bonus_cards"] != null) {
			bonusCards = int.Parse (node ["bonus_cards"].InnerText);
		}

		bonusHeal = 0;
		if (node ["bonus_heal"] != null) {
			bonusHeal = int.Parse (node ["bonus_heal"].InnerText);
		}

		bonusCardID = "none";
		if (node ["bonus_card_id"] != null) {
			bonusCardID = node ["bonus_card_id"].InnerText;
		}

		//cost effects?
		numMoveCardsPlayedToMakeThisFree = -1;
		if (node ["num_move_cards_played_to_make_free"] != null) {
			numMoveCardsPlayedToMakeThisFree = int.Parse (node ["num_move_cards_played_to_make_free"].InnerText);
		}

		//will this ignore any charms
		ignoreTargetCharms = false;
		if (node["ignore_target_charms"] != null){
			ignoreTargetCharms = bool.Parse (node ["ignore_target_charms"].InnerText);
		}

		ignoreCasterCharms = false;
		if (node["ignore_user_charms"] != null){
			ignoreCasterCharms = bool.Parse (node ["ignore_user_charms"].InnerText);
		}

		selfDestructWhenPlayed = false;
		if (node ["self_destruct"] != null) {
			selfDestructWhenPlayed = bool.Parse (node ["self_destruct"].InnerText);
		}


		//set the colors
		highlightColors = new Dictionary<CardType, Color>();
		highlightColors.Clear();
		highlightColors.Add(CardType.Attack, new Color(1f, 0.5f, 0.5f));
		highlightColors.Add(CardType.AttackSpecial, new Color(1f, 0.5f, 0.5f));
		highlightColors.Add(CardType.Movement, new Color(0.5f, 0.5f, 1f));
		highlightColors.Add(CardType.Skill, new Color(0.5f, 1f, 0.5f));
		highlightColors.Add(CardType.Loot, new Color(0.5f, 0.5f, 0.5f));
		highlightColors.Add(CardType.Equipment, new Color(0.8f, 0.8f, 0.8f));
		highlightColors.Add(CardType.Other, new Color(0.5f, 1f, 1f));
		highlightColors.Add(CardType.Spell, new Color(1f, 0.5f, 1f));

		showVisibilityIconsWhenHighlighting = false;

		//custom setup
		setupBlueprintCustom ();

		//add bonus stuff to the description
		if (bonusActions > 0) {
			description += "\n+" + bonusActions + " action(s)";
		}
		if (bonusCards > 0) {
			description += "\n+" + bonusCards + " card(s)";
		}
		if (bonusHeal > 0) {
			description += "\n+heal" + bonusHeal;
		}

		baseHighlightColor = highlightColors [type];

		//check if the description needs to be overwritten
		if (node ["desc"] != null) {
			setDescription ();
		}
	}
	public virtual void setupBlueprintCustom(){}

	void setDescription(){
		//description = node ["desc"].InnerText;

		string innerText = node ["desc"].InnerText;

		//scrape out all of the words to replace
		List<string> replacementWords = new List<string> ();
		List<int>	replacementPos = new List<int> ();

		string newDesc = "";

		bool addingReplacementWord = false;
		for (int i = 0; i < innerText.Length; i++) {

			if (innerText [i] == '{') {
				addingReplacementWord = true;
				replacementWords.Add ("");
				replacementPos.Add (newDesc.Length);
			} else if (innerText [i] == '}') {
				addingReplacementWord = false;
			} else {

				if (addingReplacementWord) {
					replacementWords [replacementWords.Count - 1] += innerText [i];
				} else {
					newDesc += innerText [i];
				}
			}
		}

		//go through and add em
		//reverse order otherwise adding the new value messes up the insert position
		for (int i = replacementWords.Count-1; i>=0; i--) {

			string val = "NODE " + replacementWords [i] + " NOT FOUND";
			if (node [replacementWords [i]] != null) {
				val = node [replacementWords [i]].InnerText;
			}

			//Debug.Log ("putting " + replacementWords [i] + " at " + replacementPos [i]+ " :"+val);
			newDesc = newDesc.Insert (replacementPos [i], val);
		}

		description = newDesc;

//		if (replacementWords.Count > 0) {
//			for (int i = 0; i < replacementWords.Count; i++) {
//				Debug.Log (replacementWords [i]);
//			}
//
//			Debug.Log("new: "+newDesc);
//		}


	}

	public void setup(Unit _owner, Deck _deck){
		owner = _owner;
		deck = _deck;
		isDead = false;
		revealAICardFlag = false;

		//grab eveything from the blueprint
		description = blueprint.description;
		idName = blueprint.idName;
		scriptName = blueprint.scriptName;
		name = blueprint.name;
		type = blueprint.type;
		cardLevel = blueprint.cardLevel;

		baseActionCost = blueprint.baseActionCost;
		costToAddToDeck = blueprint.costToAddToDeck;

		manaCost = blueprint.manaCost;

		//any bonuses?
		bonusActions = blueprint.bonusActions;

		bonusCards = blueprint.bonusCards;
		bonusHeal = blueprint.bonusHeal;
		bonusCardID = blueprint.bonusCardID;

		selfDestructWhenPlayed = blueprint.selfDestructWhenPlayed;

		//cost reductions
		numMoveCardsPlayedToMakeThisFree = blueprint.numMoveCardsPlayedToMakeThisFree;

		//will this ignore any charms
		ignoreTargetCharms = blueprint.ignoreTargetCharms;
		ignoreCasterCharms = blueprint.ignoreCasterCharms;

		//default values
		baseActionCost = blueprint.baseActionCost;
		type = blueprint.type;

		//set the colors
		highlightColors = blueprint.highlightColors;
		showVisibilityIconsWhenHighlighting = blueprint.showVisibilityIconsWhenHighlighting;
		baseHighlightColor = blueprint.baseHighlightColor;

		//have cards check their blueprints for any custom values
		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void resetCard(){
		updateIsDisabled ();
		cancel ();
	}

	public void setOwnerActive(){
		GameObjectManager.instance.getCardGO ().activate (this);
	}


	public void updateIsDisabled () {
		//check if this can be played
		isDisabled = !checkIfCanBePlayed();
	}

	//by default, cards just need an action to be played
	public  bool checkIfCanBePlayed(){
		//custom checks
		if (checkIfCanBePlayedCustom () == false) {
			return false;
		}

		//action cost
		if (Owner.ActionsLeft < getNumActionsNeededToPlay ()) {
			return false;
		}

		//mana cost if aplicable
		if (manaCost > 0) {
			//Debug.Log("this fool has "+owner.deck.getCardsInHandOfType (CardType.Spell) +" in hand");
			if (owner.deck.getCardsInHandOfType (CardType.Spell) - 1 < manaCost) {
				return false;
			}
		}

		//if everything else is legit, we're good!
		return true;
	}
	public virtual bool checkIfCanBePlayedCustom(){
		return true;
	}

	public int getNumActionsNeededToPlay(){
		int actionCost = baseActionCost;

		actionCost += getCustomActionCostMod ();

		for (int i=Owner.Charms.Count-1; i>=0; i--){
			actionCost += Owner.Charms[i].getCardActionCostMod (this);
		}
		return actionCost;
	}
	public virtual int getCustomActionCostMod(){
		int costMod = 0;

		if (numMoveCardsPlayedToMakeThisFree >= 0) {
			if (owner.getCardsOfTypePlayedThisTurn (CardType.Movement) >= numMoveCardsPlayedToMakeThisFree) {
				costMod = -baseActionCost;
			}
		}

		return costMod;
	}
		
	public void selectCard(bool isForAI=false){
		if (!isForAI) {
			isActive = true;
			owner.GM.setCardActive (this);
		}
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
			owner.GM.targetInfoText.turnOff ();
		}
		revealAICardFlag = false;
		cancelCustom ();
	}
	public virtual void cancelCustom(){}

	public void passInTile(Tile tile){
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


	public void finish(bool removeFromPlay = false){
		owner.GM.targetInfoText.turnOff ();
		revealAICardFlag = false;

		//check if this card grants any bonus
		if (bonusActions > 0) {
			owner.gainActions (bonusActions);
		}
		if (bonusCards > 0) {
			owner.deck.drawCards (bonusCards);
		}
		if (bonusHeal > 0) {
			owner.heal (bonusHeal);
		}
		if (bonusCardID != "none") {
			Card gift = CardManager.instance.getCardFromIdName (bonusCardID);
			gift.setup (owner, owner.deck);
			owner.deck.addCardToHand (gift);
		}


		deck.removeCardFromHand (this);

		owner.markCardPlayed (this);

		if (!selfDestructWhenPlayed && !removeFromPlay) {
			deck.discardCard (this);
		} else if (selfDestructWhenPlayed) {
			deck.destroyCard (this);
		} else if (removeFromPlay) {
			owner.GM.clearActiveCard ();
		}
	}
	public void discard(){
		deck.removeCardFromHand (this);
		deck.discardCard (this);
	}

	public bool isWaitingForInput(){
		return waitingForTile || waitingForUnit || waitingForChoice;
	}

	public CardType cardTypeFromString(string input){
		// https://msdn.microsoft.com/en-us/library/essfb559(v=vs.110).aspx
		try {
			CardType val = (CardType) Enum.Parse(typeof(CardType), input);      
			return val;
		}
		catch (ArgumentException) {
			Debug.Log ("CONVERSION DID NOT WORK");
			return CardType.Other;
		}
	}

	//**********************************
	//Dealing with AI
	//**********************************

	//some cards (like movement cards) can result in lots and lots of potential moves, most of which are obviously bad
	//rather than allow these to create tons of branching trees, let's try and filter out obviousl bad calls
	//this works with filterBadMovesIfApplicable in Board.cs
	public virtual int checkMoveVal(MoveInfo move, Board board){
		return 0;	//default value if the card does not do any move validity checking
	}


	//**********************************
	//utility
	//**********************************

//	void OnMouseEnter(){
//		Debug.Log ("ever at all???");
//		mouseIsOver = true;
//		if (Owner.GM.activeCard == null) {
//			mouseEnterEffects ();
//		}
//	}
	public virtual void mouseEnterEffects(){}

//	void OnMouseExit(){
//		mouseIsOver = false;
//		if (!isActive && Owner.GM.activeCard == null) {
//			Owner.board.clearHighlights ();
//		}
//		mouseExitEffects ();
//	}
	public virtual void mouseExitEffects(){}

	public string getInfoStringForCover(Tile.Cover coverVal){
		if (coverVal == Tile.Cover.Full) {
			return "Full Cover: x0.5";
		}
		if (coverVal == Tile.Cover.Part) {
			return "Part Cover: -1";
		}

		return "No Cover";
	}

	public virtual void resolveFromMove(MoveInfo move){
		Debug.Log ("problem with: "+idName+" CARD HAS NOT BEEN SETUP TO RESOLVE FROM MOVE. THIS RESULT WILL PROBABLY BE NONSENSE");
		finish ();
	}

	//for sorting
	public int CompareTo(Card other) {
		//sort by level and then type
		if (cardLevel != other.cardLevel) {
			return other.cardLevel - cardLevel;
		} else {
			if (type == other.type) {
				return name.CompareTo (other.name);
			} else {
				return (int)type - (int)other.type;
			}
		}
	}

	//**********************************
	//showing information when a potential target is moused over
	//**********************************
	public void potentialTargetMouseOver(Unit unit){
		setPotentialTargetInfo (unit);
	}
	public virtual void setPotentialTargetInfo(Unit unit){}

	//**********************************
	//Dealing damage
	//**********************************
	public void doDamageToUnit(Unit unit, int damage){
		unit.takeDamage (damage, this, Owner);
	}

	//**********************************
	//some actions that are common enough to standardize
	//**********************************

	public int getRangeForWeapon(int rangeMod){
		Debug.Log ("my wep " + Owner.Weapon.baseRange);
		int range = Owner.Weapon.baseRange + rangeMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			range += owner.Charms [i].getWeaponRangeMod (this);
		}

		return range;
	}

	public void mouseEnterForWeapon(int rangeMod){
		int range = getRangeForWeapon (rangeMod);

		Owner.board.highlightTilesInVisibleRange (Owner.CurTile, range, highlightColors[CardType.Attack]);
	}

	public void selectCardForWeapon(int rangeMod){
		WaitingForUnit = true;
		int range = Owner.Weapon.baseRange + rangeMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			range += owner.Charms [i].getWeaponRangeMod (this);
		}

		bool includePlayer = true;//!owner.isPlayerControlled;
		bool includeAI = true;//owner.isPlayerControlled;
		Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, range, includePlayer, includeAI, highlightColors[CardType.Attack]);
	}

	public void setPotentialTargetInfoTextForWeapon(Unit unit, int damageMod){
		//start with the weapon
		string text = "Weapon +"+Owner.Weapon.baseDamage+"\n";

		//then the damage mod for this card
		string cardSymbol = damageMod >= 0 ? "+" : "";
		text += "Card "+cardSymbol+damageMod+"\n";

		//check my charms
		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			text += owner.Charms [i].getWeaponDamageModifierText (this, unit);
		}

		int damage = getWeaponDamageToUnit (unit, damageMod);

		//check if the unit has any charms that would alter damage values
		int totalPrevention = 0;
		for (int i = unit.Charms.Count - 1; i >= 0; i--) {
			text += unit.Charms [i].getDamagePreventionText (this, Owner);
			totalPrevention += unit.Charms [i].getDamageTakenMod (this, owner);
		}
		if (totalPrevention < -damage) {
			totalPrevention = -damage;
		}

		//calculate cover
		Tile.Cover coverVal = Owner.board.getCover (Owner, unit);
		text += getInfoStringForCover (coverVal);

		//print the total
		//text += "\nDAMAGE: "+(damage+totalPrevention);
		int totalDamage = damage+totalPrevention;

		//set the target info text
		owner.GM.targetInfoText.turnOn(text, totalDamage, unit);
	}

	public int getWeaponDamageToUnit(Unit unit, int damageMod){
		int damageVal = Owner.Weapon.baseDamage + damageMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			damageVal += owner.Charms [i].getWeaponDamageMod (this, unit);
		}

		Tile.Cover coverVal = Owner.board.getCover (Owner, unit);
		damageVal = Owner.board.getNewDamageValFromCover (damageVal, coverVal);

		if (damageVal < 0) {
			damageVal = 0;
		}

		//Debug.Log ("cover: " + coverVal + "  damage: " + damageVal);

		return damageVal;
	}


	//checking move values

	public int GenericMovementCardCheckMoveVal(MoveInfo move, Board board){
		Unit unit = board.units [move.unitID];
		int moveVal = 0;

		Tile targetTile = board.Grid [move.targetTilePos.x, move.targetTilePos.y];
		float highestPreferedDist = unit.aiProfile.preferedDistToClosestEnemy + unit.aiProfile.acceptableDistanceRangeToClosestEnemy;

		//let's figure out who our enemies are
		Profiler.BeginSample("sorting allies and foes for move");
		List<Unit> enemies = new List<Unit> ();
		bool rootingForAI = !unit.isPlayerControlled;
		foreach (Unit u in board.units) {
			if (u.isPlayerControlled == rootingForAI) {
				enemies.Add (u);
			}
		}
		Profiler.EndSample ();

		//let's get the closest distance for this target
		float newCloseDist = 99999;
		float curCloseDist = 99999;
		foreach (Unit foe in enemies) {
			float dist = board.dm.getDist (move.targetTilePos, foe.CurTile.Pos);
			if (dist < newCloseDist) {
				newCloseDist = dist;
			}

			float curDist = board.dm.getDist (unit.CurTile.Pos, foe.CurTile.Pos);
			if (curDist < curCloseDist) {
				curCloseDist = curDist;
			}
		}

		//avoid moves that are further away than the max prefered dist and further away than we are now
		//no cowards!
		if ( !(newCloseDist > highestPreferedDist && newCloseDist > curCloseDist)) {
			moveVal++;
		}

		//is there cover (or would the move put us very close to a foe since many units like that)
		Tile.Cover lowestCover = targetTile.getHighestAdjacentCover();
		bool nextToFoe = newCloseDist < 2.5f;
		if ((int)lowestCover > (int)Tile.Cover.None || nextToFoe) {
			moveVal++;
		}

		//testing
		//		if (moveVal == 2) {
		//			targetTile.setHighlighted (true, Color.green);
		//		}
		//		if (moveVal == 1) {
		//			targetTile.setHighlighted (true, Color.yellow);
		//		}
		//		if (moveVal == 0) {
		//			targetTile.setHighlighted (true, Color.red);
		//		}

		return moveVal;
	}

	//**********************************
	//setters and getters
	//**********************************


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

	public int CostToAddToDeck{
		get{
			return this.costToAddToDeck;
		}
	}

//	public bool DoingAnimation{
//		get{
//			return this.doingAnimation;
//		}
//	}


}
