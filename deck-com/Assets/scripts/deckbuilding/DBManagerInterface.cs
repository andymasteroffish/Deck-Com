using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBManagerInterface : MonoBehaviour {

	public static DBManagerInterface instance;

	public TextAsset unitList;

	public DBManager manager;

	public GameObject deckButtonPrefab;
	private List<DBDeckButtonGO> deckButtons = new List<DBDeckButtonGO> ();

	public GameObject cardButtonPrefab;
	private List<DBCardGO> cardButtons = new List<DBCardGO> ();

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		manager = new DBManager (unitList);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void click(DBDeck deck){
		manager.click (deck);
	}


	public DBDeckButtonGO getDeckButtonGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < deckButtons.Count; i++) {
			if (deckButtons [i].IsActive == false) {
				return deckButtons [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(deckButtonPrefab) as GameObject;
		DBDeckButtonGO GO = obj.GetComponent<DBDeckButtonGO> ();
		deckButtons.Add (GO);
		return GO;
	}

	public DBCardGO getCardGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < cardButtons.Count; i++) {
			if (cardButtons [i].IsActive == false) {
				return cardButtons [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(cardButtonPrefab) as GameObject;
		DBCardGO GO = obj.GetComponent<DBCardGO> ();
		cardButtons.Add (GO);
		return GO;
	}
}
