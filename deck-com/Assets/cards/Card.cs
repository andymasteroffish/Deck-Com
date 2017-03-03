using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card {

	public enum CardType{Attack, AttackSpecial, Movement, Aid, Other};

	public string name;
	public string description;

	public XmlNode node;

	public CardType type;
	public int baseActionCost;

	private bool isActive;
	private bool waitingForTile, waitingForUnit, waitingForChoice;

	public bool mouseIsOver;
	private bool isDisabled;

	public int orderInHand;

	private Unit owner;
	private Deck deck;

	//some state shit
	public bool isDead;

	//some colors
	public Color moveHighlightColor = new Color(0.5f, 0.5f, 1f);
	public Color attackHighlightColor  = new Color(1f, 0.5f, 0.5f);
	public Color aidHighlightColor  = new Color(0.5f, 1f, 0.5f);
	public Color otherHighlightColor  = new Color(1f, 0.5f, 1f);

	//this constructor should not be used. The derived cards are the only thing that should be used
	public Card(){}

	public void setup(Unit _owner, Deck _deck){
		owner = _owner;
		deck = _deck;

		isDead = false;

		name = node ["name"].InnerText;

		baseActionCost = 1;
		if (node ["action_cost"] != null) {
			baseActionCost = int.Parse(node ["action_cost"].InnerText);
		}

		//default values
		baseActionCost = 1;
		type = CardType.Other;

		//custom setup
		setupCustom ();

		//check if the description needs to be overwritten
		if (node ["desc"] != null) {
			description = node ["desc"].InnerText;
		}
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

	//by default, cards just need an aciton to be played
	public virtual bool checkIfCanBePlayed(){
		return Owner.ActionsLeft >= getNumActionsNeededToPlay();
	}

	public int getNumActionsNeededToPlay(){
		int actionCost = baseActionCost;
		for (int i=Owner.Charms.Count-1; i>=0; i--){
			actionCost += Owner.Charms[i].getCardActionCostMod (this);
		}
		return actionCost;
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
			owner.GM.targetInfoText.turnOff ();
		}
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


	public void finish(bool destroyCard = false){
		owner.GM.targetInfoText.turnOff ();
		//StartCoroutine(doDeathAnimation(0.5f, true, destroyCard));

		owner.markCardPlayed (this);

		deck.removeCardFromHand (this);

		if (!destroyCard) {
			deck.discardCard (this);
		} else {
			deck.destroyCard (this);
		}
	}
	public void discard(){
		deck.removeCardFromHand (this);
		deck.discardCard (this);
	}

	public bool isWaitingForInput(){
		return waitingForTile || waitingForUnit || waitingForChoice;
	}

	//**********************************
	//utility
	//**********************************

	void OnMouseEnter(){
		mouseIsOver = true;
		if (Owner.GM.activeCard == null) {
			mouseEnterEffects ();
		}
	}
	public virtual void mouseEnterEffects(){}

	void OnMouseExit(){
		mouseIsOver = false;
		if (!isActive && Owner.GM.activeCard == null) {
			owner.GM.board.clearHighlights ();
		}
		mouseExitEffects ();
	}
	public virtual void mouseExitEffects(){}

	public string getInfoStringForCover(Tile.Cover coverVal){
		if (coverVal == Tile.Cover.Full) {
			return "Full Cover: *0.5";
		}
		if (coverVal == Tile.Cover.Part) {
			return "Part Cover: -1";
		}

		return "No Cover";
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

	public void mouseEnterForWeapon(int rangeMod){
		int range = Owner.Weapon.baseRange + rangeMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			range += owner.Charms [i].getWeaponRangeMod (this);
		}

		owner.GM.board.highlightTilesInVisibleRange (Owner.CurTile, range, attackHighlightColor);
	}

	public void selectCardForWeapon(int rangeMod){
		WaitingForUnit = true;
		int range = Owner.Weapon.baseRange + rangeMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			range += owner.Charms [i].getWeaponRangeMod (this);
		}

		Owner.GM.board.highlightUnitsInVisibleRange (Owner.CurTile, range, true, true, attackHighlightColor);
	}

	public void setPotentialTargetInfoTextForWeapon(Unit unit, int damageMod){
		//start with the weapon
		string text = "Weapon +"+Owner.Weapon.baseDamage+"\n";

		//then the damage mod for this card
		string cardSymbol = damageMod >= 0 ? "+" : "";
		text += "Card "+cardSymbol+damageMod+"\n";

		//check my charms
		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			text += owner.Charms [i].getDamageModifierText (this, unit);
		}

		//check if the unit has any charms that would alter damage values
		int totalPrevention = 0;
		for (int i = unit.Charms.Count - 1; i >= 0; i--) {
			text += unit.Charms [i].getDamagePreventionText (this, Owner);
			totalPrevention += unit.Charms [i].getDamageTakenMod (this, owner);
		}

		//calculate cover
		Tile.Cover coverVal = Owner.GM.board.getCover (Owner, unit);
		text += "\n"+getInfoStringForCover (coverVal)+"\n";

		//print the total
		text += "\nDAMAGE: "+(getWeaponDamageToUnit(unit, damageMod)+totalPrevention);

		//set the target info text
		owner.GM.targetInfoText.turnOn(text, unit);
	}

	public int getWeaponDamageToUnit(Unit unit, int damageMod){
		int damageVal = Owner.Weapon.baseDamage + damageMod;

		for (int i = owner.Charms.Count - 1; i >= 0; i--) {
			damageVal += owner.Charms [i].getWeaponDamageMod (this, unit);
		}

		Tile.Cover coverVal = Owner.GM.board.getCover (Owner, unit);
		damageVal = Owner.GM.board.getNewDamageValFromCover (damageVal, coverVal);

		if (damageVal < 0) {
			damageVal = 0;
		}

		//Debug.Log ("cover: " + coverVal + "  damage: " + damageVal);

		return damageVal;
	}

	//setters getters

	public bool MouseIsOver{
		get{
			return this.mouseIsOver;
		}
		set{
			mouseIsOver = value;
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

//	public bool DoingAnimation{
//		get{
//			return this.doingAnimation;
//		}
//	}


}
