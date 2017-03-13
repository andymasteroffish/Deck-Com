using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class EndGameInfoHolder : MonoBehaviour {

	public static EndGameInfoHolder instance;

	public bool bCreateDummyLoot;

	public List<Card_Loot> lootList;

	void Awake() {
		//if this already exists, destory it
		if (instance == null) {
			instance = this;
		} else {
			Destroy (gameObject);
		}

		DontDestroyOnLoad(transform.gameObject);
	}


	public List<Card_Loot> getLoot(){
		if (bCreateDummyLoot) {
			createDummyLoot ();
		}

		return lootList;
	}


	void kill(){
		instance = null;
		Destroy (gameObject);
	}

	void createDummyLoot(){
		lootList = new List<Card_Loot> ();
		for (int i = 0; i < 4; i++) {
			Loot.Type type = i % 2 == 0 ? Loot.Type.booster :  Loot.Type.money;
			int level = 1;
			Card_Loot card = (Card_Loot) CardManager.instance.getCardFromIdName ("loot");
			card.lootSetup (type, level);
			card.setup (null, null);
			lootList.Add (card);
		}

		Debug.Log ("did it");
	}
}
