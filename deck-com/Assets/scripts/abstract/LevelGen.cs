using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen {

	private const int chunkSize = 5;

	public LevelGen(){
	}

	public Tile[,] getTestLevel(string mapName){
		return loadLevelFromText ( Resources.Load<TextAsset>("maps/"+mapName) );
	}

	public Tile[,] getLevel(){
		TextAsset[] files = GameManagerTacticsInterface.instance.mapChunks;

		//create a grid
		int chunkCols = GameManagerTacticsInterface.instance.mapChunkCols;
		int chunkRows = GameManagerTacticsInterface.instance.mapChunkRows;
		int gridW = chunkCols * chunkSize;
		int gridH = chunkRows * chunkSize;
		Tile[,] grid = new Tile[gridW, gridH];

		//determine where the players will be
		int playerCol = (int)Random.Range(0, chunkCols);


		//go through and make cunks
		for (int chunkX = 0; chunkX < chunkCols; chunkX++) {
			for (int chunkY = 0; chunkY < chunkRows; chunkY++) {
				//grab a file
				TextAsset thisFile = files[(int)Random.Range(0,files.Length)];
				Tile[,] chunk = loadChunkFromText (thisFile, chunkX, chunkY);

				//add or remove one or two cover objects randomly
				greeble(chunk, chunkSize, chunkSize);

				//possibly rotate it
				int numRots = (int)Random.Range(0,4);
				for (int i = 0; i < numRots; i++) {
					chunk = rotateCCW (chunk, chunkSize, chunkSize);
				}

				//possibly flip it
				if (Random.value < 0.5f) {
					chunk = flipHorz(chunk, chunkSize, chunkSize);
				}
				if (Random.value < 0.5f) {
					chunk = flipVert(chunk, chunkSize, chunkSize);
				}

				//add it
				for (int x = 0; x < chunkSize; x++) {
					for (int y = 0; y < chunkSize; y++) {
						if (chunkX == playerCol && chunkY == 0 && chunk [x, y].CoverVal == Tile.Cover.None) {
							chunk[x,y].spawnProperty = Tile.SpawnProperty.Player;
						}

						if (chunkY >  0 && chunk [x, y].CoverVal == Tile.Cover.None) {
							chunk[x,y].spawnProperty = Tile.SpawnProperty.Foe;
						}

						grid [chunkX * chunkSize + x, chunkY * chunkSize + y] = chunk [x, y];
					}
				}
			}
		}

		//clean up the whole perimeter
		for (int x = 0; x < gridW; x++) {
			grid [x, 0].spawnProperty = Tile.SpawnProperty.None;
			grid [x, gridH-1].spawnProperty = Tile.SpawnProperty.None;
		}
		for (int y = 0; y < gridH; y++) {
			grid [0, y].spawnProperty = Tile.SpawnProperty.None;
			grid [gridW-1, 0].spawnProperty = Tile.SpawnProperty.None;
		}

		//setup all tiles
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				grid [x, y].finalizeSetup (x,y);
			}
		}

		return grid;
	}

	public void greeble(Tile[,] grid, int gridW, int gridH){
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
//				if (Random.value < 0.05f) {
//					grid [x, y].setCover (Tile.Cover.Full);
//				}
//				if (Random.value < 0.03f) {
//					grid [x, y].setCover (Tile.Cover.Part);
//				}
				if (Random.value < 0.05f) {
					grid [x, y].setCover (Tile.Cover.None);
				}
			}
		}
	}

	public Tile[,] rotateCCW(Tile[,] orig, int gridW, int gridH){
		Tile[,] grid = new Tile[gridW, gridH];
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				int sourceX = y;
				int sourceY = gridW - x - 1;

				grid [x, y] = orig [sourceX, sourceY];
			}
		}
		return grid;
	}

	public Tile[,] flipHorz(Tile[,] orig, int gridW, int gridH){
		Tile[,] grid = new Tile[gridW, gridH];
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				int sourceX = gridW - x - 1;
				int sourceY = y;

				grid [x, y] = orig [sourceX, sourceY];
			}
		}
		return grid;
	}

	public Tile[,] flipVert(Tile[,] orig, int gridW, int gridH){
		Tile[,] grid = new Tile[gridW, gridH];
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				int sourceX = x;
				int sourceY = gridH - y - 1;

				grid [x, y] = orig [sourceX, sourceY];
			}
		}
		return grid;
	}

	public Tile[,] loadChunkFromText(TextAsset file, int chunkX, int chunkY){
		Tile[,] chunk = new Tile[chunkSize, chunkSize];

		string[] lines = file.text.Split ('\n');

		for (int y = 0; y < chunkSize; y++) {
			for (int x = 0; x < chunkSize; x++) {
				char thisChar = lines [chunkSize-y-1][x];	//in unity, higher Y means up, but in text higher y means down so we need ot reverse it

				//cover
				Tile.Cover cover = Tile.Cover.None;
				if (thisChar == '#')	cover = Tile.Cover.Full;
				if (thisChar == '*')	cover = Tile.Cover.Part;

				chunk [x, y] = new Tile(cover, Tile.SpawnProperty.None, GameManagerTacticsInterface.instance.gm);
			}
		}

		return chunk;
	}

	public Tile[,] loadLevelFromText(TextAsset file){
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
				grid [x, y] = new Tile(cover, spawnProperty, GameManagerTacticsInterface.instance.gm);
				grid [x, y].finalizeSetup (x, y);
			}
		}

		return grid;
	}
}
