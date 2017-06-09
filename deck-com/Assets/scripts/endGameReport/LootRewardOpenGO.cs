using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LootRewardOpenGO : MonoBehaviour {

	public Text nameField;
	public Text descField;
	public Text moneyField;

	public SpriteRenderer spriteRend;
	public Sprite cardFrameSprite, coinFrameSprite;

	private Vector3 targetPos;

	public float lerpSpeed;

	//public float cardAngleSpread;
	public float cardDistX, cardDistY;

	public float cardY;
	public float moneyY;

	public float danceSpeed, danceSpeedRange;
	private float danceAngleRange;

	void setupGeneral(){
		transform.localScale = new Vector3 (0, 0, 0);
	}

	public void setup(Card card, float orderPrc){
		//info
		spriteRend.sprite = cardFrameSprite;
		nameField.text = card.name;
		descField.text = card.description;
		moneyField.text = "";
		danceAngleRange = 6;

		danceSpeed += Random.Range (-danceSpeedRange, danceSpeedRange);

		//placememt
		Vector3 centerPos = GameObject.Find ("lootCenterPos").transform.position;
		targetPos.x = centerPos.x + (orderPrc * 2 - 1) * cardDistX;
		targetPos.y = centerPos.y + cardY - Mathf.Abs((orderPrc-0.5f) * cardDistY);// 1f;
		targetPos.z = 0;

		//general stuff
		setupGeneral ();
	}

	public void setup(int money){
		targetPos = GameObject.Find ("lootCenterPos").transform.position;
		targetPos.y += moneyY;
		spriteRend.sprite = coinFrameSprite;
		nameField.text = "";
		descField.text = "";
		moneyField.text = "$" + money;
		danceAngleRange = 30;
		setupGeneral ();
	}
	
	// Update is called once per frame
	void Update () {
		float angle = Mathf.Sin(Time.time * danceSpeed + targetPos.x) * danceAngleRange;
		transform.localEulerAngles = new Vector3 (0, 0, angle);

		transform.position = Vector3.Lerp (transform.position, targetPos, lerpSpeed);
		transform.localScale = Vector3.Lerp (transform.localScale, Vector3.one, lerpSpeed);
	}
}
