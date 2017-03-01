using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Card : MonoBehaviour {

	public enum CardType{Attack, AttackSpecial, Movement, Aid, Other};

	public string name;

	[System.NonSerialized]
	public CardType type;
	[System.NonSerialized]
	public int baseActionCost;

	[System.NonSerialized]
	public Text nameField;
	[System.NonSerialized]
	public Text textField;
	[System.NonSerialized]
	public SpriteRenderer spriteRend;

	private bool isActive;
	private bool waitingForTile, waitingForUnit, waitingForChoice;

	private bool mouseIsOver;
	private bool isDisabled;

	private float order;

	private Unit owner;
	private Deck deck;

	//some colors
	[System.NonSerialized]
	public Color moveHighlightColor = new Color(0.5f, 0.5f, 1f);
	[System.NonSerialized]
	public Color attackHighlightColor  = new Color(1f, 0.5f, 0.5f);
	[System.NonSerialized]
	public Color aidHighlightColor  = new Color(0.5f, 1f, 0.5f);
	[System.NonSerialized]
	public Color otherHighlightColor  = new Color(1f, 0.5f, 1f);

	private bool doingAnimation;


	public void setup(Unit _owner, Deck _deck){
		owner = _owner;
		deck = _deck;

		doingAnimation = false;

		//find all the parts
		Transform[] ts = gameObject.transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in ts){
			if (t.gameObject.name == "NameText") {
				nameField = t.gameObject.GetComponent<Text> ();
			}
			if (t.gameObject.name == "BodyText") {
				textField = t.gameObject.GetComponent<Text> ();
			}
			if (t.gameObject.name == "sprite") {
				spriteRend = t.gameObject.GetComponent<SpriteRenderer> ();
			}
		}

		nameField.text = name;
		//textField.text = "Hey, I'm testin over here! "+Random.Range(0,999).ToString();

		//default values
		baseActionCost = 1;
		type = CardType.Other;

		//custom stiff
		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void reset(){
		isDisabled = false;
		cancel ();
	}

	public void setPos(Vector3 newPos, int _order){
		order = (float)_order;
		transform.localPosition = newPos;
		cancel ();
	}

	
	// Update is called once per frame
	void Update () {
		//check if this can be played
		setDisabled (checkIfCanBePlayed() == false);

		Vector3 spritePos = new Vector3 (0, 0, order * -0.1f);
		Color col = new Color (1, 1, 1, 1);
		if (mouseIsOver) {
			spritePos.z += -2;
			spritePos.y = 0.5f;
		}

		if (isActive) {
			spritePos.z += -5;
			spritePos.y = 2;
		}

		if (isDisabled) {
			col = new Color (1, 1, 1, 0.4f);
		}

		spriteRend.transform.localPosition = spritePos;
		spriteRend.color = col;
			
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

	public void setDisabled(bool val){
		isDisabled = val;
	}

	public void finish(bool destroyCard = false){
		owner.GM.targetInfoText.turnOff ();
		StartCoroutine(doDeathAnimation(0.5f, true, destroyCard));
	}
	public void discard(){
		StartCoroutine(doDeathAnimation(0.5f, false));
	}

	public bool isWaitingForInput(){
		return waitingForTile || waitingForUnit || waitingForChoice;
	}

	//**********************************
	//utility
	//**********************************

	void OnMouseEnter(){
		mouseIsOver = true;
		if (Owner.GM.ActiveCard == null) {
			mouseEnterEffects ();
		}
	}
	public virtual void mouseEnterEffects(){}

	void OnMouseExit(){
		mouseIsOver = false;
		if (!isActive && Owner.GM.ActiveCard == null) {
			owner.GM.board.clearHighlights ();
		}
		mosueExitEffects ();
	}
	public virtual void mosueExitEffects(){}

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
	//animations
	//**********************************

	IEnumerator doDeathAnimation(float time, bool markAsPlayedWhenDone, bool permanentlyDestroyCard = false){
		doingAnimation = true;

		yield return new WaitForSeconds (0.05f);

		float timer = time  * Owner.GM.debugAnimationTimeMod;
		float startScale = transform.localScale.x;

		while (timer > 0) {
			timer -= Time.deltaTime;
			timer = Mathf.Max (0, timer);

			float newScale = timer / time;
			newScale = Mathf.Pow (newScale, 2);
			transform.localScale = new Vector3 (newScale, newScale, newScale);

			yield return null;
		}

		deck.removeCardFromHand (this);


		transform.localScale = new Vector3 (1, 1, 1);	

		if (markAsPlayedWhenDone) {
			owner.markCardPlayed (this);
		}

		if (!permanentlyDestroyCard) {
			deck.discardCard (this);
		} else {
			deck.destroyCard (this);
		}

		doingAnimation = false;
	}

	public void startDrawAnimation(){
		StartCoroutine( doDrawAnimaiton (0.5f, transform.localPosition));
	}

	IEnumerator doDrawAnimaiton(float time, Vector3 targetPos){
		doingAnimation = true;
		float timer = 0;

		time *= Owner.GM.debugAnimationTimeMod;

		Vector3 startPos = targetPos;
		startPos.y -= 2;	//guessing


		while (timer < time) {
			timer += Time.deltaTime;
			float prc = timer / time;

			transform.localPosition = Vector3.Lerp (startPos, targetPos, prc);

			yield return null;
		}

		transform.localPosition = targetPos;


		doingAnimation = false;
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

	public bool DoingAnimation{
		get{
			return this.doingAnimation;
		}
	}
}
