using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm  {

	public enum CharmClass{ PatrolStatus, Charm, ExtraCard, HealRing, OneTimeShield, WeaponBonus, BasicWeapon, GlassCannon, Web, LatchOn };
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

	public int actionMod;
	public int damageAtTurnStart;

	public float sightRangeMod;

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
			offsetID = owner.Charms.Count-1;
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

		actionMod = 0;
		if (node["action_mod"] != null){
			actionMod = int.Parse(node["action_mod"].InnerText);
		}

		damageAtTurnStart = 0;
		if (node["damage_at_turn_start"] != null){
			damageAtTurnStart = int.Parse(node["damage_at_turn_start"].InnerText);
		}

		sightRangeMod = 0;
		if (node["sight_range_mod"] != null){
			sightRangeMod = float.Parse(node["sight_range_mod"].InnerText);
			//this may change what the unit can see at the moment they get this charm
			if (owner != null) {
				Debug.Log ("love to do it " + owner.getSightRange ());
				owner.setVisibleTiles ();
			}
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

		actionMod = parent.actionMod;
		damageAtTurnStart = parent.damageAtTurnStart;


		selfDestructsAfterTurns = parent.selfDestructsAfterTurns;
		turnsLeftBeforeSelfDestruct = parent.turnsLeftBeforeSelfDestruct;

		expiresAfterAttack = parent.expiresAfterAttack;

		sightRangeMod = parent.sightRangeMod;

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

		if (damageAtTurnStart != 0) {
			Owner.takeDamage (damageAtTurnStart, null, null);
		}
		if (actionMod != 0) {
			Owner.gainActions (actionMod);
		}

		resetRoundCustom ();
	}
	public virtual void resetRoundCustom(){}

	public void startAITurn(){
		startAITurnCustom ();
	}
	public virtual void startAITurnCustom(){}

	public void unitWakesUp(){
		unitWakesUpCustom ();
	}
	public virtual void unitWakesUpCustom(){
	}

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
		if (card.ignoreCasterCharms) {
			return;
		}
		cardPlayedCustom (card);
		if (expiresAfterAttack && card.type == Card.CardType.Attack) {
			Owner.removeCharm (this);
		}
	}
	public virtual void cardPlayedCustom(Card card){}

	public void takeDamage (Card card, Unit source){
		if (card != null) {
			if (card.ignoreTargetCharms) {
				return;
			}
		}
		takeDamageCustom (card, source);
	}
	public virtual void takeDamageCustom (Card card, Unit source){}

	public void dealWeaponDamage(Card card, Unit target, int damage){
		if (card.ignoreCasterCharms) {
			return;
		}
		dealWeaponDamageCustom (card, target, damage);

	}
	public virtual void dealWeaponDamageCustom(Card card, Unit target, int damage){}

	//this will fire for all charms after any action of any type. Right now I'm using it for things that need to be checked veyr reguraly
	public void anyActionTaken(){
		anyActionTakenCustom ();
	}
	public virtual void anyActionTakenCustom(){}

	//AI
	public virtual AIProfile checkReplaceAIProfile(){
		return null;
	}

	public virtual float getAIMoveValue(Board oldBoard, Board newBoard, Unit curUnit, ref TurnInfo info, bool printInfo){
		return 0;
	}

	//modifiers
	public int getCardActionCostMod(Card card){
		if (card.ignoreCasterCharms) {
			return 0;
		}
		return getCardActionCostModCustom (card);
	}
	public virtual int getCardActionCostModCustom(Card card){return 0;}

	public int getGeneralDamageMod(Card card, Unit target){
		if (card.ignoreCasterCharms) {
			return 0;
		}
		return getGeneralDamageModCustom (card, target);
	}
	public virtual int getGeneralDamageModCustom(Card card, Unit target){return 0;}

	public int getWeaponDamageMod(Card card, Unit target){
		if (card.ignoreCasterCharms) {
			return 0;
		}
		return getWeaponDamageModCustom (card, target);
	}
	public virtual int getWeaponDamageModCustom(Card card, Unit target){return 0;}

	public int getWeaponRangeMod(Card card){
		if (card.ignoreCasterCharms) {
			return 0;
		}
		return getWeaponRangeModCustom (card);
	}
	public virtual int getWeaponRangeModCustom(Card card){return 0;}

	public int getDamageTakenMod(Card card, Unit source){
		if (card == null) {
			return 0;
		}
		if (card.ignoreTargetCharms) {
			return 0;
		}
		return getDamageTakenModCustom (card, source);
	}
	public virtual int getDamageTakenModCustom(Card card, Unit source){return generalTakeDamageMod;}

	public int getHealMod(Card card, Unit target){
		if (card.ignoreCasterCharms) {
			return 0;
		}
		return getHealModCustom (card, target);
	}
	public virtual int getHealModCustom(Card card, Unit target){return 0;}

	public int getHandSizeMod(){
		return getHandSizeModCustom();
	}
	public virtual int getHandSizeModCustom(){return handSizeMod;}

	public float getSightRangeMod(){
		return getSightRangeModCustom ();
	}
	public virtual float getSightRangeModCustom(){return sightRangeMod;}

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
