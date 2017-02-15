﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	public GameManager gm;

	public int cols, rows;

	public float diagonalVal;	//1.41 is geometry 

	private Tile[,] grid = null;

	public GameObject tilePrefab;

	public int partialCoverDamageReduction;
	public float fullCoverDamagePrc;

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

	public void highlightAllUnits(bool includePlayer, bool includeAI, Color col){
		foreach (Unit unit in gm.units) {
			if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
				unit.setHighlighted (true, col);
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

	public List<Tile> getAdjacentTiles(Tile start, bool includeDiagonal){
		List<Tile> tiles = new List<Tile> ();

		for (int xOffset = - 1; xOffset <= 1; xOffset++) {
			for (int yOffset = - 1; yOffset <= 1; yOffset++) {
				int x = start.Pos.x + xOffset;
				int y = start.Pos.y + yOffset;
				if (x >= 0 && x < cols && y >= 0 && y < rows && (xOffset != 0 || yOffset !=0)) {
					if (includeDiagonal || (xOffset != yOffset)) {
						tiles.Add (grid [x, y]);
					}
				}
			}
		}

		return tiles;
	}

	public List<Unit> getAdjacentUnits(Tile start, bool includeDiagonal){
		List<Tile> tiles = getAdjacentTiles (start, includeDiagonal);
		List<Unit> units = new List<Unit> ();

		for (int i = 0; i < tiles.Count; i++) {
			Unit unit = getUnitOnTile (tiles [i]);
			if (unit != null) {
				units.Add (unit);
			}
		}
		return units;
	}

	public Unit getUnitOnTile(Tile tile){
		for (int i=0; i<gm.units.Count; i++){
			if (gm.units[i].CurTile == tile) {
				return gm.units[i];
			}
		}
		return null;
	}

	//this can be optimized a lot
	public List<Tile> getTilesInDist(Tile start, float dist){
		List<Tile> tiles = new List<Tile> ();

		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (start.Pos.getDist (grid [x, y].Pos) <= dist) {
					tiles.Add (grid [x, y]);
				}
			}
		}

		return tiles;
	}

	public Tile getFirstTileWithCover(Tile start, Tile end){
		Tile returnVal = null;
		//get the direction
		float dist = Vector3.Distance (start.transform.position, end.transform.position);
		Vector3 dir3 = end.transform.position - start.transform.position;
		Vector2 dir = new Vector2(dir3.x, dir3.y);
		//turn off tiles with no cover
		turnOffAllTileCollidersBelow(Tile.Cover.Part);
		//shoot the ray!
		RaycastHit2D hit = Physics2D.Raycast(start.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
		if (hit.collider != null) {
			returnVal = hit.collider.gameObject.GetComponent<Tile> ();
		}

		turnOnAllTileColliders ();
		return returnVal;
	}

	//checking cover
	public Tile.Cover getCover(Unit sourceUnit, Unit targetUnit){

		//check the line from the player, but also the adjacent squares that are not cover themselves
		List<Tile.Cover> coverVals = new List<Tile.Cover>();

		coverVals.Add( getCover (sourceUnit.CurTile, targetUnit.CurTile) );
		for (int i = 0; i < 4; i++) {
			if (sourceUnit.CurTile.Adjacent [i] != null) {
				if (sourceUnit.CurTile.Adjacent [i].CoverVal == Tile.Cover.None) {
					coverVals.Add (getCover (sourceUnit.CurTile.Adjacent [i], targetUnit.CurTile));
				}
			}
		}

		//go through and find the best possible shot (lowest cover val)
		Tile.Cover returnVal = Tile.Cover.Full;
		foreach (Tile.Cover cover in coverVals) {
			if ((int)cover < (int)returnVal) {
				returnVal = cover;
			}
		}

		return returnVal;
	}

	public Tile.Cover getCover(Tile sourceTile, Tile targetTile){
		//get the direction
		float dist = Vector3.Distance (sourceTile.transform.position, targetTile.transform.position);
		Vector3 dir3 = targetTile.transform.position - sourceTile.transform.position;
		Vector2 dir = new Vector2(dir3.x, dir3.y);

		//debug line
		//Debug.DrawLine(sourceTile.transform.position, sourceTile.transform.position+new Vector3(dir.x, dir.y, 0), Color.red);

		//check for full cover
		turnOffAllTileCollidersExcept(Tile.Cover.Full);

		RaycastHit2D fullHit = Physics2D.Raycast(sourceTile.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
		if (fullHit.collider != null) {
			turnOnAllTileColliders ();
			return Tile.Cover.Full;
		}

		//check for partial cover. Only tiles adjacent to the target count for this
		turnOffAllTileColliders();
		//turn on coliders for tiles adjacent to our target
		for (int i = 0; i < 4; i++) {
			if (targetTile.Adjacent [i] != null) {
				if (targetTile.Adjacent [i].CoverVal == Tile.Cover.Part) {
					targetTile.Adjacent [i].collider.enabled = true;
				}
			}
		}

		RaycastHit2D partHit = Physics2D.Raycast(sourceTile.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
		if (partHit.collider != null) {
			turnOnAllTileColliders ();
			return Tile.Cover.Part;
		}



		turnOnAllTileColliders ();
		return Tile.Cover.None;
	}

	void turnOffAllTileColliders(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].collider.enabled = false;
			}
		}
	}
	void turnOffAllTileCollidersExcept(Tile.Cover coverLevelToKeepOn){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].collider.enabled = grid [x, y].CoverVal == coverLevelToKeepOn;
			}
		}
	}
	void turnOffAllTileCollidersBelow(Tile.Cover coverLevelToKeepOn){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].collider.enabled = (int)grid [x, y].CoverVal >= (int)coverLevelToKeepOn;
			}
		}
	}
	void turnOnAllTileColliders(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].collider.enabled = true;
			}
		}
	}

	public int getNewDamageValFromCover(int origDamage, Tile.Cover cover){
		int newDamage = origDamage;
		if (cover == Tile.Cover.Part) {
			newDamage -= partialCoverDamageReduction;
		}
		if (cover == Tile.Cover.Full) {
			newDamage = (int) Mathf.Floor( (float)origDamage * fullCoverDamagePrc );
			//make sure it is at least as good as partial cover
			if (newDamage > origDamage - partialCoverDamageReduction) {
				newDamage = origDamage - partialCoverDamageReduction;
			}
		}
		return newDamage;
	}


	//setters and getters

	public Tile[,] Grid{
		get{
			return this.grid;
		}
	}

}
