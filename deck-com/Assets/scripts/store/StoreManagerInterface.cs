using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreManagerInterface : MonoBehaviour {

	public static StoreManagerInterface instance;

	public StoreManager manager;

	public TextMesh moneyText;

	public GameObject cardPrefab;
	public Transform[] spawnPoints;


	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}


	// Use this for initialization
	void Start () {
		manager = new StoreManager ();

		//make a bunch of card objects
		for (int i = 0; i < spawnPoints.Length; i++) {
			GameObject go = Instantiate (cardPrefab, spawnPoints [i].position, Quaternion.identity);
			StoreCardGO card = go.GetComponent<StoreCardGO> ();
			card.setup (manager.cards [i], i);
		}


	}
	
	// Update is called once per frame
	void Update () {

		//update the money text
		moneyText.text = "$"+manager.money;
		
	}


	public void goBack(){
		UnityEngine.SceneManagement.SceneManager.LoadScene ("deck_building");
	}
}
