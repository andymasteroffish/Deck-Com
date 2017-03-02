using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectManager : MonoBehaviour {

	public static GameObjectManager instance = null;

	public GameObject tilePrefab;
	public GameObject charmPrefab;
	public GameObject cardPrefab;

	private List<TileGO> tiles = new List<TileGO>();
	private List<CharmGO> charms = new List<CharmGO>();
	private List<CardGO> cards = new List<CardGO> ();

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	public TileGO getTileGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < tiles.Count; i++) {
			if (tiles [i].IsActive == false) {
				return tiles [i];
			}
		}

		//otherwise make one
		GameObject tileObj = Instantiate(tilePrefab) as GameObject;
		TileGO tileGO = tileObj.GetComponent<TileGO> ();
		tiles.Add (tileGO);
		return tileGO;
	}

	public CharmGO getCharmGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < charms.Count; i++) {
			if (charms [i].IsActive == false) {
				return charms [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(charmPrefab) as GameObject;
		CharmGO GO = obj.GetComponent<CharmGO> ();
		charms.Add (GO);
		return GO;
	}

	public CardGO getCardGO(){
		//check if we have any inactive in the list
		for (int i = 0; i < cards.Count; i++) {
			if (cards [i].IsActive == false) {
				return cards [i];
			}
		}

		//otherwise make one
		GameObject obj = Instantiate(cardPrefab) as GameObject;
		CardGO GO = obj.GetComponent<CardGO> ();
		cards.Add (GO);
		return GO;
	}

}
