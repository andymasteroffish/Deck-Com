using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGen {

	private const int chunkSize = 5;

	//these values get altered in a few functions. Better to use pass-by-reference, but I got lazy
	private int playerStartChunkX = -1;
	private int playerEndChunkX = -1;

	public LevelGen(){
	}

	public Tile[,] getTestLevel(string mapName){
		return loadLevelFromText ( Resources.Load<TextAsset>("maps/"+mapName) );
	}

	public Tile[,] getLevel(int curLevelNum, int curAreaNum){
		TextAsset[] files = GameManagerTacticsInterface.instance.mapChunks;

		bool [,] chunkPath = generateChunkPath (curAreaNum);
		//if it returns an empty array, that means we hit a dead end and should try again
		while (chunkPath.GetLength (0) < 2) {
			chunkPath = generateChunkPath (curAreaNum);
		}

		//create a grid
		int chunkCols = chunkPath.GetLength(0);
		int chunkRows = chunkPath.GetLength(1);

		Debug.Log ("level is " + chunkCols + " x " + chunkRows + " chunks");
		int gridW = chunkCols * chunkSize;
		int gridH = chunkRows * chunkSize;
		Tile[,] grid = new Tile[gridW, gridH];

		//determine where the players will be
		Debug.Log ("end chunk: " + playerEndChunkX);

		//go through and make cunks
		for (int chunkX = 0; chunkX < chunkCols; chunkX++) {
			for (int chunkY = 0; chunkY < chunkRows; chunkY++) {
				Tile[,] chunk;
				//if it is on the path, make a room
				if (chunkPath [chunkX, chunkY]) {
					TextAsset thisFile = files [(int)Random.Range (0, files.Length)];
					chunk = loadChunkFromText (thisFile, chunkX, chunkY);

					//add or remove one or two cover objects randomly
					greeble (chunk, chunkSize, chunkSize);

					//possibly rotate it
					int numRots = (int)Random.Range (0, 4);
					for (int i = 0; i < numRots; i++) {
						chunk = rotateCCW (chunk, chunkSize, chunkSize);
					}

					//possibly flip it
					if (Random.value < 0.5f) {
						chunk = flipHorz (chunk, chunkSize, chunkSize);
					}
					if (Random.value < 0.5f) {
						chunk = flipVert (chunk, chunkSize, chunkSize);
					}
				}

				//otherwise just fill it in
				else{
					chunk = getFilledInChunk ();
				}

				//add it to the grid
				for (int x = 0; x < chunkSize; x++) {
					for (int y = 0; y < chunkSize; y++) {
						if (chunkX == playerStartChunkX && chunkY == 0 && chunk [x, y].CoverVal == Tile.Cover.None) {
							chunk [x, y].spawnProperty = Tile.SpawnProperty.Player;
						}

						if ((chunkY > 0 || chunkX != playerStartChunkX) && chunk [x, y].CoverVal == Tile.Cover.None) {
							chunk [x, y].spawnProperty = Tile.SpawnProperty.Foe;
						}

						if (chunkX == playerEndChunkX && chunkY == chunkRows - 1 && y >= chunkSize-2) {
							chunk [x, y].spawnProperty = Tile.SpawnProperty.Exit;
						}


						grid [chunkX * chunkSize + x, chunkY * chunkSize + y] = chunk [x, y];
					}
				}
			}
		}


		//add a store key
		TilePos storePos = new TilePos(0,0);
		bool goodStoreKeyPos = false;
		while (!goodStoreKeyPos) {
			storePos = new TilePos ((int)Random.Range (2, gridW - 2), (int)Random.Range (2, gridH - 2));
			if (grid [storePos.x, storePos.y].CoverVal == Tile.Cover.None) {
				goodStoreKeyPos = true;
			}
		}
		grid [storePos.x, storePos.y].spawnProperty = Tile.SpawnProperty.StoreKey;
		grid [storePos.x, storePos.y].setCover(Tile.Cover.None);

		//setup all tiles
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				grid [x, y].finalizeSetup (x,y);
			}
		}

		return grid;
	}

	private bool[,] generateChunkPath(int curAreaNum){
		int maxSize = 16;
		bool[,] grid = new bool[maxSize, maxSize];

		//determine how long the path should be
		int targetPathLength = 8 + curAreaNum*3;

		Debug.Log ("target path lenght: " + targetPathLength);

		//set them all to be off
		for (int x = 0; x < maxSize; x++) {
			for (int y = 0; y < maxSize; y++) {
				grid [x, y] = false;
			}
		}

		//move through it
		TilePos curPos = new TilePos(maxSize/2,0);
		playerStartChunkX = curPos.x;
		for (int i=0; i<targetPathLength; i++){
			grid [curPos.x, curPos.y] = true;
			playerEndChunkX = curPos.x;

			bool goodDir = false;
			int numTries = 0;
			while (goodDir == false) {
				numTries++;
				goodDir = true;	//asume this one will work
				int nextDir = (int)Random.Range (0, 3);
				TilePos nextPos = new TilePos (curPos);
				//left
				if (nextDir == 0 && curPos.x > 0) {
					nextPos.x--;
				}
				//right
				if (nextDir == 1 && curPos.x < maxSize-1) {
					nextPos.x++;
				}
				//up
				if (nextDir == 2 && curPos.y < maxSize-1) {
					nextPos.y++;
				}

				//this spot must not currently be part of the path
				if (grid [nextPos.x, nextPos.y]) {
					goodDir = false;
				}

				//this spot should only have 1 neighbor in the path (the one leading to it)
				int numNeighbors = 0;
				if (nextPos.x > 0 && grid [nextPos.x - 1, nextPos.y])			numNeighbors++;
				if (nextPos.x < maxSize-1 && grid [nextPos.x + 1, nextPos.y])	numNeighbors++;
				if (nextPos.y > 0 && grid [nextPos.x, nextPos.y - 1])			numNeighbors++;
				if (nextPos.y < maxSize-1 && grid [nextPos.x, nextPos.y + 1])	numNeighbors++;

				if (numNeighbors > 1) {
					goodDir = false;
				}

				//if it passes the checks, add it
				if (goodDir) {
					curPos = new TilePos (nextPos);
				}
				//otherwise chekc if we failed
				else if (numTries > 50) {
					Debug.Log ("PATH DONE GOT GOOFED, TRYING AGAIN");
					return new bool[0, 0];
				}
			}
		}

		//print it
//		string levelString = "";
//		for (int y = maxSize - 1; y >= 0; y--) {
//			for (int x = 0; x < maxSize; x++) {
//				levelString += grid [x, y] ? "-" : "X";
//			}
//			levelString += '\n';
//		}
//		Debug.Log (levelString);

		//make some rooms bigger by finding corner spaces and expanding them
		List<TilePos> roomsToAdd = new List<TilePos>();
		float chanceOfAddingRoom = 0.75f;
		for (int x = 0; x < maxSize; x++) {
			for (int y = 0; y < maxSize; y++) {
				//get num neighbors for this spot
				int numNeighbors = 0;
				if (x > 0 && grid [x - 1, y])			numNeighbors++;
				if (x < maxSize-1 && grid [x + 1, y])	numNeighbors++;
				if (y > 0 && grid [x, y - 1])			numNeighbors++;
				if (y < maxSize-1 && grid [x, y + 1])	numNeighbors++;

				if (numNeighbors == 2 && Random.value < chanceOfAddingRoom) {
					roomsToAdd.Add (new TilePos (x, y));
				}	
			}
		}

		//add 'em
		for (int i = 0; i < roomsToAdd.Count; i++) {
			grid [roomsToAdd [i].x, roomsToAdd [i].y] = true;
		}

		//print it again
//		levelString = "";
//		for (int y = maxSize - 1; y >= 0; y--) {
//			for (int x = 0; x < maxSize; x++) {
//				levelString += grid [x, y] ? "-" : "X";
//			}
//			levelString += '\n';
//		}
//		Debug.Log (levelString);


		bool[,] trimGrid = trimBoolGrid (grid, maxSize);

		//print it again
//		levelString = "";
//		for (int y = trimGrid.GetLength(1) - 1; y >= 0; y--) {
//			for (int x = 0; x < trimGrid.GetLength(0); x++) {
//				levelString += trimGrid [x, y] ? "-" : "X";
//			}
//			levelString += '\n';
//		}
//		Debug.Log (levelString);

		return trimGrid;
	}

	public bool[,] trimBoolGrid(bool[,] source, int maxSize){
		//figure out the offsets
		int firstX = maxSize;
		int lastX = 0;
		int gridH = 0;
		for (int x = 0; x < maxSize; x++) {
			for (int y = 0; y < maxSize; y++) {
				if (source [x, y] == true) {
					if (x < firstX) {
						firstX = x;
					}
					if (x > lastX) {
						lastX = x;
					}
					if (y + 1 > gridH) {
						gridH = y + 1;
					}
				}
			}
		}
		int gridW = lastX + 1 - firstX;
//		Debug.Log ("firstX " + firstX + "  lastX " + lastX);
//		Debug.Log ("width: "+gridW + "  height " + gridH);

		//make a new grid
		bool[,] trimGrid = new bool[gridW,gridH];
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				int sourceX = x + firstX;
				trimGrid [x, y] = source [sourceX, y];
			}
		}

		//adjust starting pos
		playerStartChunkX -= firstX;
		playerEndChunkX -= firstX;

		return trimGrid;
	}

	public void greeble(Tile[,] grid, int gridW, int gridH){
		for (int x = 0; x < gridW; x++) {
			for (int y = 0; y < gridH; y++) {
				if (Random.value < 0.05f) {
					grid [x, y].setCover (Tile.Cover.None);
				}
			}
		}
	}

//	public void fillIn(Tile[,] grid, int gridW, int gridH){
//		for (int x = 0; x < gridW; x++) {
//			for (int y = 0; y < gridH; y++) {
//				grid [x, y].setCover (Tile.Cover.Full);
//			}
//		}
//	}

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

	public Tile[,] getFilledInChunk(){
		Tile[,] chunk = new Tile[chunkSize, chunkSize];
		for (int y = 0; y < chunkSize; y++) {
			for (int x = 0; x < chunkSize; x++) {
				chunk [x, y] = new Tile(Tile.Cover.Full, Tile.SpawnProperty.None, GameManagerTacticsInterface.instance.gm);
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
				if (thisChar == '$')	spawnProperty = Tile.SpawnProperty.StoreKey;

				//make the tile
				grid [x, y] = new Tile(cover, spawnProperty, GameManagerTacticsInterface.instance.gm);
				grid [x, y].finalizeSetup (x, y);
			}
		}

		return grid;
	}
}
