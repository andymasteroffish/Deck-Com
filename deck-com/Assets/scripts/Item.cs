using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour {

	public enum ItemType {Weapon, Charm};

	public string name, description;
	public ItemType type;

	public int baseDamage, baseRange;

	private Unit owner;

	//display
	[System.NonSerialized]
	public Text nameField;
	[System.NonSerialized]
	public Text textField;
	[System.NonSerialized]
	public SpriteRenderer spriteRend;

	//sliding in and out
	private Vector3 startPos, endPos;
	private bool doingAnimation;

	public void setup(Unit _owner, int offsetID){

		owner = _owner;

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

		//putting the test in
		nameField.text = name;
		textField.text = description;

		//setting up movement
		setPos(offsetID);

		transform.position = startPos;

		doingAnimation = false;

		setupCustom ();
	}
	public virtual void setupCustom(){}

	public void setPos(int offsetID){
		float ySpacing = 1.4f;
		Transform startTrans = GameObject.Find ("itemStartPos").transform;
		startPos = new Vector3 (startTrans.position.x, startTrans.position.y + offsetID * ySpacing, startTrans.position.z);
		Transform endTrans = GameObject.Find ("itemEndPos").transform;
		endPos = new Vector3 (endTrans.position.x, endTrans.position.y + offsetID * ySpacing, endTrans.position.z);
	}
	
	public void setActive(bool isActive){
		float moveTime = 0.2f;
		if (isActive) {
			StartCoroutine (doMoveAnimation (startPos, endPos, moveTime));
		} else {
			StartCoroutine (doMoveAnimation (endPos, startPos, moveTime));
		}
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


	//animations
	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time){
		doingAnimation = true;

		float timer = 0;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			prc = Mathf.Pow (prc, 0.75f);
			transform.position = Vector3.Lerp (start, target, prc);
			yield return null;
		}

		doingAnimation = false;
		transform.position = target;
	}

	//setters getters

	public Unit Owner{
		get{
			return this.owner;
		}
	}
}
