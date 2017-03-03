using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Board : MonoBehaviour {

	public GameManager gm;
	public LevelGen levelGen;

	private int cols, rows;

	public float diagonalVal;	//1.41 is geometry 

	private Tile[,] grid = null;

	//public GameObject tilePrefab;

	public int partialCoverDamageReduction;
	public float fullCoverDamagePrc;


	public void reset(){
		//clear ();
		grid = levelGen.getTestLevel ();// new Tile[cols, rows];

		cols = grid.GetLength (0);
		rows = grid.GetLength (1);

		//give them some relevant info
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				Tile[] adjacent = new Tile[] {null, null, null, null};
				if (x < cols-1)	adjacent [(int)Tile.Direction.Right]= grid [x+1, y];
				if (x > 0)		adjacent [(int)Tile.Direction.Left] = grid [x-1, y];
				if (y < rows-1)	adjacent [(int)Tile.Direction.Up] 	= grid [x, y + 1];
				if (y > 0)		adjacent [(int)Tile.Direction.Down] = grid [x, y - 1];
				grid [x, y].setInfo (adjacent);
				//grid[x,y].transform.parent = transform;
			}
		}

		clearHighlights ();
	}

//	private void clear(){
//		if (grid != null){
//			for (int x = 0; x < cols; x++) {
//				for (int y = 0; y < rows; y++) {
//					Destroy (grid [x, y].gameObject);
//				}
//			}
//		}
//	}

	public void clearHighlights(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].setHighlighted (false);
			}
		}
		foreach (Unit unit in gm.Units) {
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

	//**********************
	//Hihglighting tiles and units
	//**********************
	public void highlightUnitsInRange(Tile source, float range, bool includePlayer, bool includeAI, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInMoveRange (source, range, true, true);
		foreach (Tile tile in selectable) {
			foreach (Unit unit in gm.Units) {
				if (unit.CurTile == tile) {
					if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
						unit.setHighlighted (true, col);
					}
				}
			}
		}
	}

	public void highlightUnitsInVisibleRange(Tile source, float range, bool includePlayer, bool includeAI, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInVisibleRange (source, range);
		foreach (Tile tile in selectable) {
			foreach (Unit unit in gm.Units) {
				if (unit.CurTile == tile) {
					if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
						unit.setHighlighted (true, col);
					}
				}
			}
		}
	}

	public void highlightAllUnits(bool includePlayer, bool includeAI, Color col){
		foreach (Unit unit in gm.Units) {
			if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
				unit.setHighlighted (true, col);
			}
		}
	}

	//**********************
	//VISIBLE RANGE IS AS THE CROW FLIES, BUT NOT OBSCURED
	//**********************
	public void highlightTilesInVisibleRange(Tile source, float range, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInVisibleRange (source, range);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInVisibleRange(Tile source, float range){
		List<Tile> returnTiles = new List<Tile> ();

		//figure out what range could work in a square
		int startX = (int)Mathf.Max(source.Pos.x - range, 0);
		int startY = (int)Mathf.Max(source.Pos.y - range, 0);
		int endX = (int)Mathf.Min(source.Pos.x + range, cols-1);
		int endY = (int)Mathf.Min(source.Pos.y + range, rows-1);

		//go through each one
		for (int x = startX; x <= endX; x++) {
			for (int y = startY; y <= endY; y++) {
				//check if it is in range
				if (source.Pos.getDist (grid [x, y].Pos) <= range) {

					//if it is, check if a line can be drawn between it and any adjacent, unnocupied tiles
					List<Tile> tiles = getAdjacentTiles (source, false, Tile.Cover.Part);
					tiles.Add (source);	//get adjacent does not include the source by default
					bool doneChecking = false;

					foreach (Tile tile in tiles) {
						if (!doneChecking) {
							if (checkIfTilesAreVisibleToEachother (tile, grid [x, y])) {
								returnTiles.Add (grid [x, y]);
								doneChecking = true;
							}
						}
					}
				}
			}
		}

		return returnTiles;
	}

	//draws a ray between two tiles and returns true if no full cover blocked it
	public bool checkIfTilesAreVisibleToEachother(Tile a, Tile b){
		bool returnVal = true;
		//get the direction
		float dist = a.Pos.getDist(b.Pos);// Vector3.Distance (a.transform.position, b.transform.position);
		Vector3 dir3 = b.Pos.getV3() - a.Pos.getV3();// b.transform.position - a.transform.position;
		Vector2 dir = new Vector2(dir3.x, dir3.y);
		//turn off tiles with no cover
		turnOffAllTileCollidersExcept(Tile.Cover.Full);
		//turn off the colliders on these tiles too
		a.GO.collider.enabled = false;
		b.GO.collider.enabled = false;
		//shoot the ray!
		RaycastHit2D hit = Physics2D.Raycast(a.GO.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
		if (hit.collider != null) {
			returnVal = false;
		}

		turnOnAllTileColliders ();
		return returnVal;
	}

	//**********************
	//IN MOVE RANGE MEANS YOU CAN WALK THERE
	//**********************
	public void highlightTilesInMoveRange(Tile source, float range, bool includeWalls, bool includeOccupied, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInMoveRange (source, range, includeWalls, includeOccupied);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInMoveRange(Tile source, float range, bool includeWalls, bool includeOccupied){
		return getTilesInMoveRange (source.Pos.x, source.Pos.y, range, includeWalls, includeOccupied);
	}

	public List<Tile> getTilesInMoveRange(int sourceX, int sourceY, float range, bool includeWalls, bool includeOccupied){
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
							foreach (Unit unit in gm.Units) {
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

	//**********************
	//IN RANGE IGNORES COVER AND JUST CHECKS DISTANCE
	//**********************
	public void highlightTilesInRange(Tile source, float range, Tile.Cover maxCoverVal, bool includeOccupied, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInRange (source, range, maxCoverVal, includeOccupied);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInRange(Tile source, float range, Tile.Cover maxCoverVal, bool includeOccupied){
		List<Tile> returnTiles = new List<Tile> ();

		//figure out what range could work in a square
		int startX = (int)Mathf.Max(source.Pos.x - range, 0);
		int startY = (int)Mathf.Max(source.Pos.y - range, 0);
		int endX = (int)Mathf.Min(source.Pos.x + range, cols-1);
		int endY = (int)Mathf.Min(source.Pos.y + range, rows-1);

		//go through each one
		for (int x = startX; x <= endX; x++) {
			for (int y = startY; y <= endY; y++) {
				//check if the cover is OK
				if ((int)grid [x, y].CoverVal <= (int)maxCoverVal) {
					//check if it is in range
					if (source.Pos.getDist (grid [x, y].Pos) <= range) {

						//is it unoccupied (or are we allowing occupied tiles)?
						bool unfilled = true;
						if (!includeOccupied) {
							foreach (Unit unit in gm.Units) {
								if (unit.CurTile == grid [x, y]) {
									unfilled = false;
								}
							}
						}

						if (unfilled) {
							returnTiles.Add (grid [x, y]);
						}
					}
				}
			}
		}

		return returnTiles;
	}

	//********************
	//getting tiles and units from a tile
	//********************

	public void highlightAdjacentTiles(Tile start, bool includeDiagonal, Tile.Cover maxCover, Color col){
		List<Tile> tiles = getAdjacentTiles (start, includeDiagonal, maxCover);
		Debug.Log("tile coutn "+tiles.Count);
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].setHighlighted (true, col);
		}
	}

	public List<Tile> getAdjacentTiles(Tile start, bool includeDiagonal, Tile.Cover maxCover){
		List<Tile> tiles = new List<Tile> ();

		for (int xOffset = - 1; xOffset <= 1; xOffset++) {
			for (int yOffset = - 1; yOffset <= 1; yOffset++) {
				int x = start.Pos.x + xOffset;
				int y = start.Pos.y + yOffset;
				if (x >= 0 && x < cols && y >= 0 && y < rows && (xOffset != 0 || yOffset !=0)) {
					if (includeDiagonal || (xOffset != yOffset)) {
						if ((int)grid [x, y].CoverVal <= (int)maxCover) {
							tiles.Add (grid [x, y]);
						}
					}
				}
			}
		}

		return tiles;
	}

	public List<Unit> getAdjacentUnits(Tile start, bool includeDiagonal){
		List<Tile> tiles = getAdjacentTiles (start, includeDiagonal, Tile.Cover.Full);
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
		for (int i=0; i<gm.Units.Count; i++){
			if (gm.Units[i].CurTile == tile) {
				return gm.Units[i];
			}
		}
		return null;
	}

	public Tile GetUnoccupiedTileWithSpawnProperty(Tile.SpawnProperty property){
		List<Tile> matches = new List<Tile> ();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid [x, y].spawnProperty == property) {
					if (getUnitOnTile (grid [x, y]) == null) {
						matches.Add (grid [x, y]);
					}
				}
			}
		}

		return matches [(int)Random.Range (0, matches.Count)];
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
		float dist = start.Pos.getDist(end.Pos);//  Vector3.Distance (start.transform.position, end.transform.position);
		Vector3 dir3 = end.Pos.getV3() - start.Pos.getV3();// end.transform.position - start.transform.position;
		Vector2 dir = new Vector2(dir3.x, dir3.y);
		//turn off tiles with no cover
		turnOffAllTileCollidersBelow(Tile.Cover.Part);
		//shoot the ray!
		RaycastHit2D hit = Physics2D.Raycast(start.GO.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
		if (hit.collider != null) {
			returnVal = hit.collider.gameObject.GetComponent<TileGO> ().MyTile;
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
		float dist = sourceTile.Pos.getDist(targetTile.Pos);// Vector3.Distance (sourceTile.transform.position, targetTile.transform.position);
		Vector3 dir3 = targetTile.Pos.getV3() - sourceTile.Pos.getV3();	// targetTile.transform.position - sourceTile.transform.position;
		Vector2 dir = new Vector2(dir3.x, dir3.y);

		//debug line
		//Debug.DrawLine(sourceTile.transform.position, sourceTile.transform.position+new Vector3(dir.x, dir.y, 0), Color.red);

		//check for full cover
		turnOffAllTileCollidersExcept(Tile.Cover.Full);

		RaycastHit2D fullHit = Physics2D.Raycast(sourceTile.GO.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
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
					targetTile.Adjacent [i].GO.collider.enabled = true;
				}
			}
		}

		RaycastHit2D partHit = Physics2D.Raycast(sourceTile.GO.transform.position, dir, dist, 1 <<  LayerMask.NameToLayer ("Tile"));
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
				grid [x, y].GO.collider.enabled = false;
			}
		}
	}
	void turnOffAllTileCollidersExcept(Tile.Cover coverLevelToKeepOn){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].GO.collider.enabled = grid [x, y].CoverVal == coverLevelToKeepOn;
			}
		}
	}
	void turnOffAllTileCollidersBelow(Tile.Cover coverLevelToKeepOn){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].GO.collider.enabled = (int)grid [x, y].CoverVal >= (int)coverLevelToKeepOn;
			}
		}
	}
	void turnOnAllTileColliders(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].GO.collider.enabled = true;
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
