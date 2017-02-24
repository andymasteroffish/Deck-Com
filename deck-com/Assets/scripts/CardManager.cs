using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour {

	public GameObject[] cardPrefabs;

	private Dictionary<string, GameObject> cards = new Dictionary<string, GameObject>();

	public void setup(){
		populateDictionary ();
	}

	private void populateDictionary(){
		for (int i = 0; i < cardPrefabs.Length; i++) {
			cards.Add (cardPrefabs [i].name, cardPrefabs [i]);
		}
	}

	public List<GameObject> getCardPrefabsFromTextFile(TextAsset file){
		List<GameObject> deck = new List<GameObject> ();

		string[] lines = file.text.Split ('\n');
		for (int i = 0; i < lines.Length; i++) {
			GameObject thisCard = getCardPrefabFromName (lines [i]);
			if (thisCard != null) {
				deck.Add (thisCard);
			}
		}

		return deck;
	}

	public GameObject getCardPrefabFromName(string name){
		if (!cards.ContainsKey (name)) {
			Debug.Log ("BAD CARD NAME: "+name);
			return null;
		}
		return cards [name];
	}
}
