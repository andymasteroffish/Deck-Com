using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	public GameObject startButton, storeButton;
	public GameObject[] unusedCardsScrollButtons;

	public GameObject unusedCardSelectionBG;

	public TextMesh moneyText;

	public int maxUnusedCardsShownAtOnce;
	public Text unusedCardWindowPageText;

	public LevelProgressionMap levelMap;

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

		levelMap.setup (manager.curLevel);
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
		storeButton.SetActive (startButton.activeSelf);

		//unused card window UI
//		unusedCardsScrollButtons[0].SetActive(manager.unusedCardsOpen && manager.curUnusedCardWindowPage > 0);	//prev
//		unusedCardsScrollButtons[1].SetActive(manager.unusedCardsOpen && manager.curUnusedCardWindowPage < manager.maxUnusedCardWindowPage);	//next
		foreach (GameObject thisButton in unusedCardsScrollButtons) {
			thisButton.SetActive (manager.unusedCardsOpen);
		}
		unusedCardWindowPageText.text = "";
		if (manager.unusedCardsOpen) {
			unusedCardWindowPageText.text = (manager.curUnusedCardWindowPage+1).ToString () + "/" + (manager.maxUnusedCardWindowPage+1).ToString ();
		}

		//turn the card selector backgorund on if that is active
		unusedCardSelectionBG.SetActive( manager.unusedCardsOpen || manager.unusedWeaponsOpen);
			
		//update the text
		moneyText.text = "$"+manager.money;

		levelMap.gameObject.SetActive (manager.activeDeck == null);

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

	public void scrollUnusedCardsForward(){
		manager.scrollUnusedCards (1);
	}
	public void scrollUnusedCardsBack(){
		manager.scrollUnusedCards (-1);
	}

//	public void saveChanges(){
//		manager.saveChanges();
//	}

	public void startGame(){
		UnityEngine.SceneManagement.SceneManager.LoadScene ("tactics_game");
	}

	public void goToStore(){
		UnityEngine.SceneManagement.SceneManager.LoadScene ("store");
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
				Debug.Log ("gimme unused");
				return unusedCharmButtons [i];
			}
		}

		//otherwise make one
		Debug.Log("gimme new");
		GameObject obj = Instantiate(unusedCharmButtonPrefab) as GameObject;
		DBUnusedCharmGO GO = obj.GetComponent<DBUnusedCharmGO> ();
		unusedCharmButtons.Add (GO);
		return GO;
	}
}
