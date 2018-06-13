using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreCardGO : MonoBehaviour {

	private StoreManager manager;

	private StoreCard storeCard;
	private int order;

	public Text nameField;
	public Text descField;
	public Text typeField;
	public Text levelNumField;
	public SpriteRenderer spriteRend;
	public SpriteRenderer colorSprite;

	public Image purchasedStamp;

	public GenericButton buyButton;

	private int _order;

	public void setup(StoreCard _card, int _order){
		
		manager = StoreManagerInterface.instance.manager;

		storeCard = _card;
		order = _order;

		gameObject.name = "card "+storeCard.card.name;

		colorSprite.color = new Color(storeCard.card.baseHighlightColor.r, storeCard.card.baseHighlightColor.g, storeCard.card.baseHighlightColor.b, 0.3f);

		//set the text
		nameField.text = storeCard.card.name;
		descField.text = storeCard.card.description;
		typeField.text = CardManager.instance.TypeNames [storeCard.card.type];
		levelNumField.text = storeCard.card.cardLevel.ToString();

		buyButton.text.text = "Buy $" + storeCard.cost.ToString ();
	}

	void Update () {

		buyButton.isDisabled = manager.money < storeCard.cost || storeCard.hasBeenBought;

		purchasedStamp.enabled = storeCard.hasBeenBought;
		
	}

	public void clickBuy(){
		Debug.Log ("buy "+storeCard.card.name);
		manager.buyCard (order);
	}
}
