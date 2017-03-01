using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectManager : MonoBehaviour {

	public GameObject tilePrefab;

	private List<TileGO> tiles = new List<TileGO>();

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

}
