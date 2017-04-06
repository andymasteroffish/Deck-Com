using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm  {

	public enum CharmType {Weapon, Charm};

	public string name, description;
	public CharmType type;

	public int costToAddToDeck;

	public int baseDamage, baseRange;	//only used for weapons. All other charms will ignore these values

	private Unit owner;
	public int offsetID;

	public bool isDead;

	private bool useGameObject;

	public XmlNode node;
	public string idName;

	//info for game display
	public bool hasChangedPos;

	//this should never be used as only the derived classes should be used in game
	public Charm(){}

	public void setup(Unit _owner, bool _useGameObject, string _idName){

		owner = _owner;
		useGameObject = _useGameObject;
		idName = _idName;

		if (owner != null) {
			offsetID = owner.Charms.Count;
		}

		isDead = false;
		hasChangedPos = false;

		costToAddToDeck = 3;

		name = node ["name"].InnerText;

		type = CharmType.Charm;	//default to char, so wepaons should change this in setupCustom

		setupCustom ();

		//if a description was specified, overwrite whatever was going on
		if (node ["desc"] != null) {
			description = node ["desc"].InnerText;
		}

	}
	public virtual void setupCustom(){}

	public void setActive(bool isActive){
		//when we first activate, get a new shell game object to display the charm
		if (isActive && useGameObject) {
			GameObjectManager.instance.getCharmGO ().activate(this);
		}
		//the charm GO will handle turning itself off the player is no longer active
	}

	public void resetRound(){
		resetRoundCustom ();
	}
	public virtual void resetRoundCustom(){}

	//general play
	public virtual void turnEndPreDiscard(){}
	public virtual void turnEndPostDiscard(){}

	public virtual void cardPlayed(Card card){}
	public virtual void takeDamage (Card card, Unit source){}

	//modifiers
	public virtual int getCardActionCostMod(Card card){return 0;}
	public virtual int getWeaponDamageMod(Card card, Unit target){return 0;}
	public virtual int getWeaponRangeMod(Card card){return 0;}
	public virtual int getDamageTakenMod(Card card, Unit source){return 0;}
	public virtual int getHealMod(Card card, Unit target){return 0;}

	//writing modifiers in the info box
	public string getDamageModifierText(Card card, Unit target){
		string returnVal = "";
		int damageMod = getWeaponDamageMod (card, target);
		if (damageMod != 0) {
			string symbol = damageMod >= 0 ? " +" : " ";
			returnVal += name +symbol + damageMod + "\n";
		}

		return returnVal;
	}

	public string getDamagePreventionText(Card card, Unit source){
		string returnVal = "";
		int damageMod = getDamageTakenMod (card, source);
		if (damageMod != 0) {
			string symbol = damageMod >= 0 ? " +" : " ";
			returnVal += name +symbol + damageMod + "\n";
		}

		return returnVal;
	}

	//setters getters

	public Unit Owner{
		get{
			return this.owner;
		}
	}
}
