using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm  {

	public enum CharmClass{ Charm, ExtraCard, HealRing, OneTimeShield, WeaponBonus, BasicWeapon, Web };
	public enum CharmType {Weapon, Equipment, StatusEffect};

	public string name, description;
	public CharmClass className;
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

	//bonus values that can be used ot make simple tweaks to charms
	public bool selfDestructsAfterTurns;
	public int turnsLeftBeforeSelfDestruct;

	public bool expiresAfterAttack;

	public int handSizeMod;
	public int generalTakeDamageMod;

	public bool protectDuringAISim;	//KILL THIS

	//if a charm came from a card (most likely equipment) that card should be stored in limbo until the charm is destroyed
	public Card storedCard;

	//ai stuff
	public float aiGoodCharmPoints, aiBadCharmPoints;


	//this should never be
	public Charm(){}

	//this should be used
	public Charm(XmlNode _node){
		node = _node;
	}
	public Charm(Charm parent){
		setFromParent (parent);
	}

	public void setup(Unit _owner, bool _useGameObject, string _idName){
		owner = _owner;
		useGameObject = _useGameObject;
		idName = _idName;

		if (owner != null) {
			offsetID = owner.Charms.Count;
		}

		isDead = false;
		hasChangedPos = false;

		protectDuringAISim = false;

		storedCard = null;

		costToAddToDeck = 3;

		name = node ["name"].InnerText;

		className = CharmClass.Charm;
		type = CharmType.Equipment;	//default to equipment for no solid reason

		//check general values

		handSizeMod = 0;
		if (node ["hand_size_mod"] != null) {
			handSizeMod = int.Parse (node ["hand_size_mod"].InnerText);
		}

		generalTakeDamageMod = 0;
		if (node["general_take_damage_mod"] != null){
			generalTakeDamageMod = int.Parse(node["general_take_damage_mod"].InnerText);
		}
				

		selfDestructsAfterTurns = false;
		turnsLeftBeforeSelfDestruct = -1;
		if (node ["turns_to_self_destruct"] != null) {
			selfDestructsAfterTurns = true;
			turnsLeftBeforeSelfDestruct = int.Parse (node ["turns_to_self_destruct"].InnerText);
		}

		expiresAfterAttack = false;
		if (node ["expires_after_attack"] != null) {
			expiresAfterAttack = bool.Parse (node ["expires_after_attack"].InnerXml);
		}

		aiGoodCharmPoints = 0;
		aiBadCharmPoints = 0;

		//do the charm's own setup
		setupCustom ();

		//if a description was specified, overwrite whatever was going on
		if (node ["desc"] != null) {
			description = node ["desc"].InnerText;
		}

		//check if an alternate ai value was provided
		if (node ["ai_good_points"] != null) {
			aiGoodCharmPoints = float.Parse(node ["ai_good_points"].InnerText);
		}
		if (node ["ai_bad_points"] != null) {
			aiBadCharmPoints = float.Parse(node ["ai_bad_points"].InnerText);
		}
	}
	public virtual void setupCustom(){}

	public void setFromParent(Charm parent){
		owner = parent.owner;
		useGameObject = false;
		idName = parent.idName;

		if (owner != null) {
			offsetID = owner.Charms.Count;
		}

		isDead = parent.isDead;
		hasChangedPos = parent.hasChangedPos;

		storedCard = null;

		//protectDuringAISim = false;

		aiGoodCharmPoints = parent.aiGoodCharmPoints;
		aiBadCharmPoints = parent.aiBadCharmPoints;

		costToAddToDeck = parent.costToAddToDeck;

		name = parent.name;

		className = parent.className;
		type = parent.type;

		baseDamage = parent.baseDamage;
		baseRange = parent.baseRange;

		handSizeMod = parent.handSizeMod;

		generalTakeDamageMod = parent.generalTakeDamageMod;


		selfDestructsAfterTurns = parent.selfDestructsAfterTurns;
		turnsLeftBeforeSelfDestruct = parent.turnsLeftBeforeSelfDestruct;

		expiresAfterAttack = parent.expiresAfterAttack;

		//do the charm's own setup
		setFromParentCustom (parent);

		//if a description was specified, overwrite whatever was going on
		description = parent.description;
	}
	public virtual void setFromParentCustom(Charm parent){
	}

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

	public void storeCard(Card card){
		storedCard = card;
	}

	//general play
	public void turnEndPreDiscard(){
		turnEndPreDiscardCustom ();
		if (selfDestructsAfterTurns) {
			turnsLeftBeforeSelfDestruct--;
			if (turnsLeftBeforeSelfDestruct <= 0) {
				Owner.removeCharm (this);
			}
		}
	}
	public virtual void turnEndPreDiscardCustom(){}
	public virtual void turnEndPostDiscard(){}

	public void cardPlayed(Card card){
		cardPlayedCustom (card);
		if (expiresAfterAttack && card.type == Card.CardType.Attack) {
			Owner.removeCharm (this);
		}
	}
	public virtual void cardPlayedCustom(Card card){
	}

	public virtual void takeDamage (Card card, Unit source){}
	public virtual void dealWeaponDamage(Unit target, int damage){}

	//modifiers
	public virtual int getCardActionCostMod(Card card){return 0;}

	public virtual int getGeneralDamageMod(Card card, Unit target){return 0;}

	public virtual int getWeaponDamageMod(Card card, Unit target){return 0;}

	public virtual int getWeaponRangeMod(Card card){return 0;}

	public int getDamageTakenMod(Card card, Unit source){
		if (card.ignoreTargetCharms) {
			return 0;
		}
		return getDamageTakenModCustom (card, source);
	}
	public virtual int getDamageTakenModCustom(Card card, Unit source){return generalTakeDamageMod;}

	public virtual int getHealMod(Card card, Unit target){return 0;}

	public virtual int getHandSizeMod(){return handSizeMod;}

	//writing modifiers in the info box
	public string getWeaponDamageModifierText(Card card, Unit target){
		string returnVal = "";
		int damageMod = getWeaponDamageMod (card, target) + getGeneralDamageMod(card, target);
		if (damageMod != 0) {
			string symbol = damageMod >= 0 ? " +" : " ";
			returnVal += name +symbol + damageMod + "\n";
		}

		return returnVal;
	}

	public string getGeneralDamageModifierText(Card card, Unit target){
		string returnVal = "";
		int damageMod = getGeneralDamageMod (card, target);
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
