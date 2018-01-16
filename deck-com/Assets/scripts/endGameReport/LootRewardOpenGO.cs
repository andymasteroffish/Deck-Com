using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootRewardOpenGO : MonoBehaviour {

	[System.NonSerialized]
	public Card card;
	[System.NonSerialized]
	public int moneyVal;
	private EndGameReportInterface endGameInterface;

	public Text moneyField;
	public Text nameField;
	public Text descField;
	public Text typeField;
	public Text levelTextField, levelNumField;

	public SpriteRenderer spriteRend, cardColorSpriteRend;
	public Sprite cardFrameSprite, coinFrameSprite;

	[System.NonSerialized]
	public LootRewardOpenGO pairedLoot;

	private Vector3 targetPos;

	public float lerpSpeed;

	//public float cardAngleSpread;
	public float cardDistX, cardDistY;
	public Vector2 secondCardOffset;

	public float cardY;
	public float moneyY;

	public float danceSpeed, danceSpeedRange;
	public float cardDanceRange, moneyDanceRange;

	//selecting
	public Vector3 mouseOverAdjust;
	[System.NonSerialized]
	public bool mouseIsOver;


	void setupGeneral(){
		transform.localScale = new Vector3 (0, 0, 0);
		pairedLoot = null;
	}

	//for cards
	public void setup(Card _card, float orderPrc, int cardOrder, EndGameReportInterface _endGameInterface){
		card = _card;
		moneyVal = 0;
		endGameInterface = _endGameInterface;

		//info
		spriteRend.sprite = cardFrameSprite;
		moneyField.text = "";

		nameField.text = card.name;
		descField.text = card.description;
		levelTextField.text = "lvl";
		levelNumField.text = card.cardLevel.ToString ();
		typeField.text = CardManager.instance.TypeNames [card.type];

		//danceAngleRange = 6;

		danceSpeed += Random.Range (-danceSpeedRange, danceSpeedRange);

		cardColorSpriteRend.enabled = true;
		cardColorSpriteRend.color = new Color(card.baseHighlightColor.r, card.baseHighlightColor.g, card.baseHighlightColor.b, 0.3f);

		//placememt
		Vector3 centerPos = GameObject.Find ("lootCenterPos").transform.position;
		targetPos.x = centerPos.x + (orderPrc * 2 - 1) * cardDistX;
		targetPos.y = centerPos.y + cardY - Mathf.Abs((orderPrc-0.5f) * cardDistY);// 1f;
		targetPos.z = (float)cardOrder * -0.1f;

		targetPos.x += secondCardOffset.x * (float)cardOrder;
		targetPos.y += secondCardOffset.y * (float)cardOrder;

		//general stuff
		setupGeneral ();
	}

	//for money
	public void setup(int money, EndGameReportInterface _endGameInterface){
		moneyVal = money;
		card = null;
		endGameInterface = _endGameInterface;

		targetPos = GameObject.Find ("lootCenterPos").transform.position;
		targetPos.y += moneyY;
		spriteRend.sprite = coinFrameSprite;
		moneyField.text = "$" + money;
		nameField.text = "";
		descField.text = "";
		levelNumField.text = "";
		levelTextField.text = "";
		typeField.text = "";

		//danceAngleRange = 30;
		cardColorSpriteRend.enabled = false;
		setupGeneral ();
	}
	
	// Update is called once per frame
	void Update () {
		float danceAngleRange = spriteRend.sprite == coinFrameSprite ? moneyDanceRange : cardDanceRange;

		float angle = Mathf.Sin(Time.time * danceSpeed + targetPos.x) * danceAngleRange;
		transform.localEulerAngles = new Vector3 (0, 0, angle);

		transform.position = Vector3.Lerp (transform.position, targetPos, lerpSpeed);
		transform.localScale = Vector3.Lerp (transform.localScale, Vector3.one, lerpSpeed);

		//mouse over effects

		Vector3 spritePos = new Vector3 (0, 0, 0);
		if (card != null) {
			Color baseCol = new Color (card.baseHighlightColor.r, card.baseHighlightColor.g, card.baseHighlightColor.b, 0.3f);
			cardColorSpriteRend.color = Color.Lerp (baseCol, new Color (0, 0, 0, 1), 0.5f);
		} else {
			spriteRend.color = new Color (1, 1, 1, 0.5f);
		}
		if (mouseIsOver ) {
			spritePos += mouseOverAdjust;
		}
		spriteRend.transform.localPosition = spritePos;
		//update the color if this loot or a paired loot is moused over
		if (mouseIsOver || (pairedLoot != null && pairedLoot.mouseIsOver)) {
			if (card != null) {
				cardColorSpriteRend.color = new Color (card.baseHighlightColor.r, card.baseHighlightColor.g, card.baseHighlightColor.b, 0.3f);
			} else {
				spriteRend.color = new Color (1, 1, 1, 1);
			}
		}


		if (mouseIsOver && Input.GetMouseButtonDown (0)) {
			endGameInterface.selectLoot (this);
			if (pairedLoot != null) {
				endGameInterface.selectLoot (pairedLoot);
			}
			endGameInterface.continueToNext ();
		}
	}



	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}
}
