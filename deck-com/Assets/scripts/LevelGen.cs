using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen : MonoBehaviour {

	//public GameObject tilePrefab;

	public TextAsset testLevelFile;
	public GameManager gm;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public Tile[,] getTestLevel(){
		return loadLevelFromText (testLevelFile);
	}

	public Tile[,] loadLevelFromText(TextAsset file){
		Debug.Log ("get it now pls");
		string[] lines = file.text.Split ('\n');
		int gridW = lines [0].Length;
		int gridH = lines.Length;

		Tile[,] grid = new Tile[gridW, gridH];

		for (int y = 0; y < gridH; y++) {
			for (int x = 0; x < gridW; x++) {

				char thisChar = lines [gridH-y-1][x];	//in unity, higher Y means up, but in text higher y means down so we need ot reverse it

				//cover
				Tile.Cover cover = Tile.Cover.None;
				if (thisChar == '#')	cover = Tile.Cover.Full;
				if (thisChar == '*')	cover = Tile.Cover.Part;

				//should anything be able to spawn on this tiles? (or is it the exit?)
				Tile.SpawnProperty spawnProperty = Tile.SpawnProperty.None;
				if (thisChar == 'P')	spawnProperty = Tile.SpawnProperty.Player;
				if (thisChar == 'F')	spawnProperty = Tile.SpawnProperty.Foe;
				if (thisChar == 'G')	spawnProperty = Tile.SpawnProperty.Exit;

				//make the tile
				grid [x, y] = new Tile(x, y, cover, spawnProperty, true, gm);

//				GameObject tileObj = Instantiate (tilePrefab, new Vector3 (x, y, 0), Quaternion.identity) as GameObject;
//				tileObj.transform.parent = transform;
//				grid [x, y] = tileObj.GetComponent<Tile> ();
//				grid [x, y].setup (cover, spawnProperty);
			}
		}

		return grid;
	}
}
