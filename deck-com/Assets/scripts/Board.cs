using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	public GameManager gm;

	public int cols, rows;

	public float diagonalVal;	//1.41 is geometry 

	private Tile[,] grid = null;

	public GameObject tilePrefab;

	// Use this for initialization
	void Start () {
		reset ();
	}

	public void reset(){
		clear ();
		grid = new Tile[cols, rows];

		//spawn tiles
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				GameObject tileObj = Instantiate (tilePrefab, new Vector3 (x, y, 0), Quaternion.identity) as GameObject;
				tileObj.transform.parent = transform;
				grid [x, y] = tileObj.GetComponent<Tile> ();
			}
		}

		//give them some relevant info
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				Tile[] adjacent = new Tile[] {null, null, null, null};
				if (x < cols-1)	adjacent [(int)Tile.Direction.Right]= grid [x+1, y];
				if (x > 0)		adjacent [(int)Tile.Direction.Left] = grid [x-1, y];
				if (y < rows-1)	adjacent [(int)Tile.Direction.Up] 	= grid [x, y + 1];
				if (y > 0)		adjacent [(int)Tile.Direction.Down] = grid [x, y - 1];
				grid [x, y].setup (adjacent, x, y, gm);
			}
		}

		clearHighlights ();
	}

	private void clear(){
		if (grid != null){
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					Destroy (grid [x, y].gameObject);
				}
			}
		}
	}

	public void clearHighlights(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].setHighlighted (false);
			}
		}
		foreach (Unit unit in gm.units) {
			unit.setHighlighted (false);
		}
	}

	public void checkClick(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].checkClick ();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void highlightUnitsInRange(Tile source, float range, bool includePlayer, bool includeAI, Color col){
		List<Tile> selectable = getTilesInRange (source, range, true, true);
		foreach (Tile tile in selectable) {
			foreach (Unit unit in gm.units) {
				if (unit.CurTile == tile) {
					if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
						unit.setHighlighted (true, col);
					}
				}
			}
		}
	}

	public void highlightTilesInRange(Tile source, float range, bool includeWalls, bool includeOccupied, Color col){
		List<Tile> selectable = getTilesInRange (source, range, includeWalls, includeOccupied);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInRange(Tile source, float range, bool includeWalls, bool includeOccupied){
		return getTilesInRange (source.Pos.x, source.Pos.y, range, includeWalls, includeOccupied);
	}

	public List<Tile> getTilesInRange(int sourceX, int sourceY, float range, bool includeWalls, bool includeOccupied){
		List<TileSearchInfo> active = new List<TileSearchInfo>();
		List<TileSearchInfo> searched = new List<TileSearchInfo>();

		active.Add( new TileSearchInfo(grid[sourceX, sourceY], null) );

		while (active.Count > 0) {
			TileSearchInfo cur = active [0];
			active.RemoveAt (0);
			searched.Add (cur);

			//check all surrounding tiles
			for (int x = cur.pos.x-1; x <= cur.pos.x+1; x++) {
				for (int y = cur.pos.y - 1; y <= cur.pos.y + 1; y++) {
					if (x >= 0 && x < cols && y >= 0 && y < rows) {

						//how far is it?
						float distVal = (x==cur.pos.x || y==cur.pos.y) ? 1 : diagonalVal;
						float newDist = cur.distFromStart + distVal;

						//is it passable? none or partial cover are both passable
						Tile.Cover maxCover = includeWalls ? Tile.Cover.Full : Tile.Cover.Part;
						bool passable =  (int)grid[x,y].CoverVal <= (int)maxCover;

						//corners need to have one of the cardinal direcitons open as well
						if (passable){
							//top left
							if (x == cur.pos.x - 1 && y == cur.pos.y + 1) {
								passable = (int)grid[cur.pos.x-1,cur.pos.y].CoverVal <= (int)maxCover || (int)grid[cur.pos.x,cur.pos.y+1].CoverVal <= (int)maxCover;
							}
							//bottom left
							if (x == cur.pos.x - 1 && y == cur.pos.y - 1) {
								passable = (int)grid[cur.pos.x-1,cur.pos.y].CoverVal <= (int)maxCover || (int)grid[cur.pos.x,cur.pos.y-1].CoverVal <= (int)maxCover;
							}
							//top right
							if (x == cur.pos.x + 1 && y == cur.pos.y + 1) {
								passable = (int)grid[cur.pos.x+1,cur.pos.y].CoverVal <= (int)maxCover || (int)grid[cur.pos.x,cur.pos.y+1].CoverVal <= (int)maxCover;
							}
							//bottom right
							if (x == cur.pos.x + 1 && y == cur.pos.y - 1) {
								passable = (int)grid[cur.pos.x+1,cur.pos.y].CoverVal <= (int)maxCover || (int)grid[cur.pos.x,cur.pos.y-1].CoverVal <= (int)maxCover;
							}
						}

						//is it unoccupied (or are we allowing occupied tiles?
						bool unfilled = true;
						if (!includeOccupied) {
							foreach (Unit unit in gm.units) {
								if (unit.CurTile == grid [x, y]) {
									unfilled = false;
								}
							}
						}

						if ( passable && unfilled && newDist <= range){
							
							//if we are still in range, we may want to add this tile
							bool shouldAdd = true;

							//check the searched list. If the tile is already in, don't add it
							for (int i = 0; i < searched.Count; i++) {
								if (searched [i].pos.x == x && searched[i].pos.y == y) {
									shouldAdd = false;
								}
							}

							//if it is already in the active list, we may want to update it
							for (int i = 0; i < active.Count; i++) {
								if (active [i].pos.x == x && active [i].pos.y == y) {
									shouldAdd = false;
									if (active [i].distFromStart > newDist) {
										active [i].distFromStart = newDist;
										active [i].parent = cur.tile;

									}
								}
							}

							//add it if we should
							if (shouldAdd) {
								TileSearchInfo newInfo = new TileSearchInfo (grid [x, y], cur.tile);
								newInfo.distFromStart = newDist;
								//Debug.Log ("search tile " + cur.pos.x + "," + cur.pos.y+" with dist "+newInfo.distFromStart);
								active.Add (newInfo);
							}
						}
					}
				}
			}

			//we're done checking all nearby tiles, so sort the active list so that the next one we check has the next shortest dist
			active.Sort();
		}

		//compile a list of all tiles in the searched llist
		List<Tile> returnTiles = new List<Tile>();
		foreach (TileSearchInfo info in searched) {
			returnTiles.Add (info.tile);
			//testing
			//info.tile.debugText.text = info.distFromStart.ToString();
		}

		return returnTiles;

	}


	//setters and getters

	public Tile[,] Grid{
		get{
			return this.grid;
		}
	}

}
