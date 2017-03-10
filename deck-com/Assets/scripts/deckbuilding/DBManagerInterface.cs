using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class DBManagerInterface : MonoBehaviour {

	public static DBManagerInterface instance;

	public DBManager manager;

	public GameObject deckButtonPrefab;
	private List<DBDeckButtonGO> deckButtons = new List<DBDeckButtonGO> ();

	public GameObject charmButtonPrefab;
	private List<DBCharmGO> charmButtons = new List<DBCharmGO> ();

	public GameObject unusedCharmButtonPrefab;
	private List<DBUnusedCharmGO> unusedCharmButtons = new List<DBUnusedCharmGO> ();

	public GameObject cardButtonPrefab;
	private List<DBCardGO> cardButtons = new List<DBCardGO> ();

	public GameObject[] deckViewButtons;
	public GameObject startButton;

	public GameObject unusedCardSelectionBG;

	public TextMesh moneyText;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		manager = new DBManager ();
	}
	
	// Update is called once per frame
	void Update () {
		if (manager.activeDeck != null) {
			for (int i = 0; i < deckViewButtons.Length; i++) {
				bool shouldShow = true;
				//hide all buttons but cancel if the unused deck is the only one open
				if (manager.activeDeck == manager.unusedCardsDeck && i != 0){
					shouldShow = false;
				}
				//when the unused list is open for selected, only cancel should appear
				if (manager.unusedCardsOpen) {
					shouldShow = i == 0;
				}
				deckViewButtons [i].SetActive ( shouldShow );
			}
		} else {
			for (int i = 0; i < deckViewButtons.Length; i++) {
				deckViewButtons [i].SetActive (false);
			}
		}

		startButton.SetActive (manager.activeDeck == null);

		//turn the card selector backgorund on if that is active
		unusedCardSelectionBG.SetActive( manager.unusedCardsOpen || manager.unusedCharmsOpen);
			
		//update the money
		moneyText.text = "$"+manager.money;

		//some keyboard stuff
		if (Input.GetKeyDown (KeyCode.Escape)) {
			manager.cancel ();
		}
	}

	public void clickDeck(DBDeck deck){
		manager.setDeckActive (deck);
	}

	public void cancel(){
		manager.cancel ();
	}

	public void openAddCardScreen(){
		if (manager.activeDeck != null && manager.activeDeck != manager.unusedCardsDeck) {
			manager.openUnusedCards ();
		}
	}

//	public void saveChanges(){
//		manager.saveChanges();
//	}

	public void startGame(){
		UnityEngine.SceneManagement.SceneManager.LoadScene ("tactics_game");
	}

	//supplying the shell objects
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

	public DBCharmGO getCharmGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < charmButtons.Count; i++) {
			if (charmButtons [i].IsActive == false) {
				return charmButtons [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(charmButtonPrefab) as GameObject;
		DBCharmGO GO = obj.GetComponent<DBCharmGO> ();
		charmButtons.Add (GO);
		return GO;
	}

	public DBUnusedCharmGO getUnusedCharmGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < unusedCharmButtons.Count; i++) {
			if (unusedCharmButtons [i].IsActive == false) {
				return unusedCharmButtons [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(unusedCharmButtonPrefab) as GameObject;
		DBUnusedCharmGO GO = obj.GetComponent<DBUnusedCharmGO> ();
		unusedCharmButtons.Add (GO);
		return GO;
	}
}
