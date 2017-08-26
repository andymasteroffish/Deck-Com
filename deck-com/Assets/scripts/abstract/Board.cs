using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class Board {

	public static int debugCounter;	//testing
	
	private LevelGen levelGen;

	public int cols, rows;

	public float diagonalVal = 1.41f;

	private Tile[,] grid = null;

	public List<Unit> units;
	public List<Loot> loot;

	public int partialCoverDamageReduction = 1;
	public float fullCoverDamagePrc = 0.5f;

	public bool isAISim;

	public DistanceManager dm;

	public Board(){
		debugCounter = 0;
	}

	public void mainBoardSetup(){
		isAISim = false;
		levelGen = new LevelGen ();
		units = new List<Unit> ();
		loot = new List<Loot> ();
	}

	public void reset(int curLevelNum, int curAreaNum){
		//clear ();
		if (GameManagerTacticsInterface.instance.debugIgnoreStandardSpawns) {
			grid = levelGen.getTestLevel (GameManagerTacticsInterface.instance.debugMapName);
		} else {
			grid = levelGen.getLevel ();
		}

		loot.Clear ();

		cols = grid.GetLength (0);
		rows = grid.GetLength (1);

		dm = new DistanceManager (cols, rows);

		//give them some relevant info
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				Tile[] adjacent = new Tile[] {null, null, null, null};
				if (x < cols-1)	adjacent [(int)Tile.Direction.Right]= grid [x+1, y];
				if (x > 0)		adjacent [(int)Tile.Direction.Left] = grid [x-1, y];
				if (y < rows-1)	adjacent [(int)Tile.Direction.Up] 	= grid [x, y + 1];
				if (y > 0)		adjacent [(int)Tile.Direction.Down] = grid [x, y - 1];
				grid [x, y].setInfo (adjacent);
				grid [x, y].createVisibilityGrid (cols, rows);
			}
		}

		clearHighlights ();

		//testing
//		for (int x = 0; x < cols; x++) {
//			for (int y = 0; y < rows; y++) {
//				grid [x, y].debugText = dm.dists [x, y].ToString("N1");
//			}
//		}
	}

	public void resetUnitsAndLoot(int curAreaNum){
		foreach(Unit unit in units){
			unit.reset ();
		}

		int lootToDistribute = GameManagerTacticsInterface.instance.lootPerLevel + (int)Random.Range (0, (float)GameManagerTacticsInterface.instance.potentialBonusLootPerLevel+1);
		Debug.Log ("loot to distribute: " + lootToDistribute);

		List<Unit> potentialLootHolders = new List<Unit> ();
		for (int i = 0; i < units.Count; i++) {
			if (!units [i].isPlayerControlled) {
				potentialLootHolders.Add (units [i]);
			}
		}
		for (int i = 0; i < lootToDistribute && potentialLootHolders.Count > 0; i++) {
			int lootGetterID = (int)Random.Range (0,potentialLootHolders.Count);
			Unit lootGetter = potentialLootHolders [lootGetterID];
			potentialLootHolders.RemoveAt (lootGetterID);
			//Debug.Log (lootGetter.unitName + " got loot");
			Loot thisLoot = new Loot (lootGetter, curAreaNum);
			loot.Add (thisLoot);
		}

//		//add some loot to some of them
//		for (int i = 0; i < units.Count; i++) {
//			if (!units [i].isPlayerControlled) {
//				if (Random.value < GameManagerTacticsInterface.instance.lootDropPrc) {
//					Debug.Log (units [i].unitName + " got loot");
//					Loot thisLoot = new Loot (units [i], curAreaNum);
//					loot.Add (thisLoot);
//				}
//			}
//		}
//
//		//if straight up nobody got loot, just select one
//		while(loot.Count == 0) {
//			int randID = (int)Random.Range (0, units.Count);
//			if (!units [randID].isPlayerControlled) {
//				Loot thisLoot = new Loot (units [randID], curAreaNum);
//				loot.Add (thisLoot);
//			}
//		}
	}

	//creating a board for AI stuff
	public void setFromParent(Board parent){
		//Profiler.BeginSample ("board setup");
		isAISim = true;

		debugCounter++;
		//Debug.Log ("board num " + debugCounter);

		levelGen = null;
		//info
		cols = parent.cols;
		rows = parent.rows;
		partialCoverDamageReduction = parent.partialCoverDamageReduction;
		fullCoverDamagePrc = parent.fullCoverDamagePrc;
		diagonalVal = parent.diagonalVal;

		dm = parent.dm;

		//tiles

		if (grid == null) {
			Profiler.BeginSample ("making new tiles");
		
			grid = new Tile[cols, rows];
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					grid [x, y] = new Tile (parent.grid [x, y]);//ObjectPooler.instance.getTile (parent.grid [x, y]);
				}
			}

			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					Tile[] adjacent = new Tile[] { null, null, null, null };
					if (x < cols - 1)
						adjacent [(int)Tile.Direction.Right] = grid [x + 1, y];
					if (x > 0)
						adjacent [(int)Tile.Direction.Left] = grid [x - 1, y];
					if (y < rows - 1)
						adjacent [(int)Tile.Direction.Up] = grid [x, y + 1];
					if (y > 0)
						adjacent [(int)Tile.Direction.Down] = grid [x, y - 1];
					grid [x, y].setInfo (adjacent);
				}
			}
			Profiler.EndSample ();
		} else {
			Profiler.BeginSample ("set existing tiles");
			//JUST GOING THROUGH THIS FOR LOOP IS TAKING A HUGELY LONG TIME. LONGER THAN THE TIME SPENT RUNNING setFromParent()
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					grid [x, y].setFromParent(parent.grid [x, y]);
				}
			}
			//grid = parent.grid;
			Profiler.EndSample ();
		}

		//units
		//Profiler.BeginSample("make units");
		units = new List<Unit>();
		for (int i = 0; i < parent.units.Count; i++) {
			Tile startTile = grid [parent.units [i].CurTile.Pos.x, parent.units [i].CurTile.Pos.y];
			Unit thisUnit = ObjectPooler.instance.getUnit();// new Unit (parent.units [i], this, startTile);
			thisUnit.setAISimUnitFromParent(parent.units [i], this, startTile);
			units.Add (thisUnit);
		}
		//Profiler.EndSample ();

		//loot is not represented and can stay empty
		loot = new List<Loot>();
		//Profiler.EndSample ();


	}

	//**************************
	//game functions
	//**************************

	//killing units
	public void removeUnit(Unit deadOne){
		units.Remove (deadOne);
	}

	//resolving moves
	public void resolveMove(MoveInfo move){
		Profiler.BeginSample ("resolving move");
		if (move.passMove == false) {
			units [move.unitID].deck.getCardInHandFromID (move.cardIDName).resolveFromMove (move);
		} else {
			//if a unit passes, they lose all actions
			units [move.unitID].ActionsLeft = 0;
		}
		Profiler.EndSample ();
	}

	public Board resolveMoveAndReturnResultingBoard(MoveInfo move){
		//Debug.Log ("new resolve for unit with " + units [move.unitID].ActionsLeft + " actions left");
		units [move.unitID].isActingAIUnit = true;
		//Profiler.BeginSample ("creating board");
		Board newBoard = ObjectPooler.instance.getBoard();//new Board(this);
		newBoard.setFromParent(this);
		//Profiler.EndSample ();

		newBoard.resolveMove (move);
		return newBoard;
	}

	//setting things to be selectables
	public void clearHighlights(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].setHighlighted (false);
			}
		}
		foreach (Unit unit in units) {
			unit.setHighlighted (false);
			unit.setMouseColliderActive (true);	//make sure units can be clicked again
		}
		clearVisibilityIcons ();
	}

	public void clearVisibilityIcons(){
		foreach (Unit unit in units) {
			unit.showVisibilityIcon = false;
		}
	}

	public void turnOffUnitMouseColliders(){
		foreach (Unit unit in units) {
			unit.setMouseColliderActive (false);	
		}
	}

	public void checkClick(){
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].checkClick ();
			}
		}
		for (int i = units.Count - 1; i >= 0; i--) {
			units[i].checkGeneralClick ();
		}
	}

	//checking if anything should happen when a unit dies
	public void checksWhenUnitDies(Unit deadUnit){
		//should loot drop?
		for (int i = 0; i < loot.Count; i++) {
			loot [i].checkShouldDrop (deadUnit);
		}
		updateVisible ();
	}

	//some loot specific functions
	public bool checkIfUnitIsCloseToLoot(Unit unit){
		for (int i = 0; i < loot.Count; i++) {
			if (loot [i].canBeCollected (unit)) {
				return true;
			}
		}
		return false;
	}

	public void collectLootNearUnit(Unit unit){
		for (int i = loot.Count-1; i >= 0; i--) {
			if (loot [i].canBeCollected (unit)) {
				Debug.Log ("collecting loot " + i);
				loot [i].collect (unit);
				loot.RemoveAt (i);
			}
		}
		unit.CanPickupLoot = false;
	}

	public void changeCover(Tile target, Tile.Cover newCoverVal){
		Profiler.BeginSample("change cover");
		//check if any change is needed
		if (target.CoverVal == newCoverVal) {
			return;
		}

		//change this tile
		target.setCover (newCoverVal);

		//update visibility grids if this is the real board
		if (!isAISim) {
			foreach (Tile tile in grid) {
				tile.clearVisibilityGridCrossingTile (target.Pos.x, target.Pos.y);
			}
		}
		//otherwise just mark that we should no longer use those values
		else {
			foreach (Tile tile in grid) {
				tile.ignoreStoredRanges = true;
			}
		}

//		//redo all adjacent lists
//		foreach (Tile tile in grid) {
//			tile.setAdjacentTiles();
//		}

		//have each unit check their new visibility
		foreach (Unit unit in units) {
			unit.setVisibleTiles ();
		}

		Profiler.EndSample ();
	}

	//telling the tiles if any player unit can see them
	public void updateVisible(){
		//mark all tiles that are visible to player units
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].isVisibleToPlayer = false;
				foreach (Unit unit in units) {
					if (unit.isPlayerControlled && !unit.isDead && !grid[x,y].isVisibleToPlayer && unit.visibleTiles != null) {
						if (unit.visibleTiles.Contains (grid [x, y])) {
							grid [x, y].isVisibleToPlayer = true;
						}
					}
				}
				if (GameManagerTacticsInterface.instance.debugRemoveFOW) {
					grid [x, y].isVisibleToPlayer = true;
				}
			}
		}


		foreach (Unit unit in units) {
			//check if this would wake up any AI units
			if (!unit.isPlayerControlled) {
				if (grid [unit.CurTile.Pos.x, unit.CurTile.Pos.y].isVisibleToPlayer && !unit.isAwake) {
					unit.wakeUp ();
				}
			}
		}
	}

	//showing which units will be visible from a given tile
	public void updateUnitVisibilityIconsFromTile(Tile sourceTile, Unit sourceUnit){
		List<Tile> tiles = getTilesInVisibleRange (sourceTile, sourceUnit.sightRange);

		//go through all units and see if they are on any of those tiles
		foreach (Unit unit in units) {
			unit.showVisibilityIcon = false;
			if (unit != sourceUnit && unit.getIsVisibleToPlayer()) {
				foreach (Tile tile in tiles) {
					if (unit.CurTile == tile) {
						unit.showVisibilityIcon = true;
						break;
					}
				}
			}
		}
	}

	//**********************
	//Hihglighting tiles and units
	//**********************


	public void highlightAllUnits(bool includePlayer, bool includeAI, Color col){
		clearHighlights ();
		foreach (Unit unit in units) {
			if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
				unit.setHighlighted (true, col);
			}
		}
	}

	//**********************
	//VISIBLE RANGE IS AS THE CROW FLIES, BUT NOT OBSCURED
	//**********************
	public void highlightUnitsInVisibleRange(Tile source, float range, bool includePlayer, bool includeAI, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInVisibleRange (source, range);
		foreach (Tile tile in selectable) {
			foreach (Unit unit in units) {
				if (unit.CurTile == tile && !unit.isDead) {
					if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
						unit.setHighlighted (true, col);
					}
				}
			}
		}
	}

	public List<Tile> highlightTilesInVisibleRange(Tile source, float range, Color col){
		clearHighlights ();
		turnOffUnitMouseColliders ();
		List<Tile> selectable = getTilesInVisibleRange (source, range);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
		return selectable;
	}

	public List<Tile> getTilesInVisibleRange(Tile source, float range){
		//Debug.Log ("checking tiles from " + source.Pos.x + "," + source.Pos.y + " in range " + range);
		Profiler.BeginSample ("get visible in range");

		List<Tile> returnTiles = new List<Tile> ();

		List<Tile> bleedTiles = new List<Tile>();

		//figure out what range could work in a square
		int startX = (int)Mathf.Max(source.Pos.x - range, 0);
		int startY = (int)Mathf.Max(source.Pos.y - range, 0);
		int endX = (int)Mathf.Min(source.Pos.x + range, cols-1);
		int endY = (int)Mathf.Min(source.Pos.y + range, rows-1);

		//go through each one
		Profiler.BeginSample("Scan tiles");
		for (int x = startX; x <= endX; x++) {
			for (int y = startY; y <= endY; y++) {
				//check if we already have values for this one
				if (source.visibleRangeDists [x, y] > -1 && !source.ignoreStoredRanges) {
					if (source.visibleRangeDists [x, y] <= range) {
						returnTiles.Add (grid [x, y]);
					}
				}
				//otherwise do the calculations
				else {
					//check if it is in range
					float dist = dm.getDist(source, grid[x,y]);// source.Pos.getDist (grid [x, y].Pos);
					if (dist <= range) {

						//if it is, check if a line can be drawn between it and any adjacent, unnocupied tiles
						List<Tile> tiles = new List<Tile> ();
						//List<Tile> tiles =  getAdjacentTiles (source, false, Tile.Cover.Part);
						tiles.Add (source);	//get adjacent does not include the source by default
						bool doneChecking = false;

						//assume it will not work
						//source.setVisibleRangeDist (grid [x, y], 999);

						//check if it will work
						foreach (Tile tile in tiles) {
							if (!doneChecking) {
								if (checkIfTilesAreVisibleToEachother (tile, grid [x, y])) {
									//add this tile
									//Debug.Log ("setting " + source.Pos.x + "," + source.Pos.y + " <-> " + x + "," + y + " to " + dist); 
									source.setVisibleRangeDist (grid [x, y], dist);
									returnTiles.Add (grid [x, y]);
									doneChecking = true;

								}
							}
						}
					}
				}
			}
		}
		Profiler.EndSample ();



		//bleed once for any tile that is not full cover, adding any tiles adjacent to a currently visible tiles
		Profiler.BeginSample("get bleed");
		foreach (Tile curTile in returnTiles) {
			if (curTile.CoverVal != Tile.Cover.Full) {
				List<Tile> adjacentTiles = getAdjacentTiles (curTile, true, Tile.Cover.Full);
				foreach (Tile t in adjacentTiles) {
					bleedTiles.Add (t);
				}
			}
		}
		Profiler.EndSample ();

		//add 'em
		Profiler.BeginSample("add bleed");
		foreach (Tile t in bleedTiles) {
			float dist = dm.getDist (source, t);// source.Pos.getDist (t.Pos);
			if (dist <= range) {
				//source.setVisibleRangeDist (t, dist);
				returnTiles.Add (t);
			}
		}
		Profiler.EndSample ();

		Profiler.EndSample ();
		return returnTiles;
	}

	//draws a ray between two tiles and returns true if no full cover blocked it
	public bool checkIfTilesAreVisibleToEachother(Tile a, Tile b){
		
		if (raytraceFloat (a, b, Tile.Cover.Full) == null) {
			return true;
		} else {
			return false;
		}
	}



	//Units should have a list of tiles visible to them
	public void highlightUnitsVisibleToUnit(Unit source, bool includeAllies, bool includeFoes, Color col){
		clearHighlights ();
		if (source.visibleTiles == null) {
			Debug.Log ("set visible tiles for " + source.unitName + " with vision " + source.sightRange);
			source.setVisibleTiles ();
		}
		foreach (Unit unit in units) {
			if (!unit.isDead) {
				bool sameSide = source.isPlayerControlled == unit.isPlayerControlled;
				if ((sameSide && includeAllies) || (!sameSide && includeFoes)) {
					if (source.visibleTiles.Contains (unit.CurTile)) {
						unit.setHighlighted (true, col);
					}
				}
			}
		}
	}

	public void highlightTilesVisibleToUnit(Unit source, Color col){
		clearHighlights ();
		turnOffUnitMouseColliders ();
		if (source.visibleTiles == null) {
			source.setVisibleTiles ();
		}
		foreach (Tile tile in source.visibleTiles) {
			tile.setHighlighted (true, col);
		}
	}

	//**********************
	//IN MOVE RANGE MEANS YOU CAN WALK THERE
	//**********************
	public void highlightTilesInMoveRange(Tile source, float range, bool includeWalls, bool includeOccupied, Color col){
		clearHighlights ();
		turnOffUnitMouseColliders ();
		List<Tile> selectable = getTilesInMoveRange (source, range, includeWalls, includeOccupied);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInMoveRange(Tile source, float range, bool includeWalls, bool includeOccupied){
		return getTilesInMoveRange (source.Pos.x, source.Pos.y, range, includeWalls, includeOccupied);
	}

	public List<Tile> getTilesInMoveRange(int sourceX, int sourceY, float range, bool includeWalls, bool includeOccupied){
		Profiler.BeginSample ("get tiles in move range");
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
						Tile.Cover maxCover = includeWalls ? Tile.Cover.Full : Tile.Cover.None;
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
							foreach (Unit unit in units) {
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
		Profiler.EndSample ();
		return returnTiles;
	}

	//**********************
	//IN RANGE IGNORES COVER AND JUST CHECKS DISTANCE
	//**********************
	public void highlightTilesInRange(Tile source, float range, Tile.Cover maxCoverVal, bool includeOccupied, Color col){
		clearHighlights ();
		turnOffUnitMouseColliders ();
		List<Tile> selectable = getTilesInRange (source, range, maxCoverVal, includeOccupied);
		foreach (Tile tile in selectable) {
			tile.setHighlighted (true, col);
		}
	}

	public List<Tile> getTilesInRange(Tile source, float range, Tile.Cover maxCoverVal, bool includeOccupied){
		Profiler.BeginSample("get tiles in range");
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
					if (dm.getDist(source, grid [x, y]) <= range){			//if (source.Pos.getDist (grid [x, y].Pos) <= range) {
						//is it unoccupied (or are we allowing occupied tiles)?
						bool unfilled = true;
						if (!includeOccupied) {
							foreach (Unit unit in units) {
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
		Profiler.EndSample ();
		return returnTiles;
	}

	//********************
	//getting tiles and units from a tile
	//********************

	public void highlightAdjacentTiles(Tile start, bool includeDiagonal, Tile.Cover maxCover, Color col){
		turnOffUnitMouseColliders ();
		List<Tile> tiles = getAdjacentTiles (start, includeDiagonal, maxCover);
		Debug.Log("tile count "+tiles.Count);
		for (int i = 0; i < tiles.Count; i++) {
			tiles [i].setHighlighted (true, col);
		}
	}

	public List<Tile> getAdjacentTiles(Tile start, bool includeDiagonal, Tile.Cover maxCover){
		Profiler.BeginSample ("get adjacent tiles");

		Profiler.BeginSample ("make list for adjcent");
		List<Tile> tiles = new List<Tile> ();
		Profiler.EndSample ();

		Profiler.BeginSample ("go through tiles");
		for (int xOffset = - 1; xOffset <= 1; xOffset++) {
			for (int yOffset = - 1; yOffset <= 1; yOffset++) {
				int x = start.Pos.x + xOffset;
				int y = start.Pos.y + yOffset;
				if (x >= 0 && x < cols && y >= 0 && y < rows && (xOffset != 0 || yOffset !=0)) {
					if (includeDiagonal || (xOffset != yOffset)) {
						if ((int)grid [x, y].CoverVal <= (int)maxCover) {
							Profiler.BeginSample ("add to lsit");
							tiles.Add (grid [x, y]);
							Profiler.EndSample ();
						}
					}
				}
			}
		}
		Profiler.EndSample ();

		Profiler.EndSample ();

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

	public Unit getUnitOnTile(TilePos pos){
		return getUnitOnTile (grid [pos.x, pos.y]);
	}
	public Unit getUnitOnTile(Tile tile){
		for (int i=0; i<units.Count; i++){
			if (units[i].CurTile == tile) {
				return units[i];
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
						if (GameManagerTacticsInterface.instance.debugIgnoreStandardSpawns) {
							return grid [x, y];
						} else {
							matches.Add (grid [x, y]);
						}
					}
				}
			}
		}

		return matches [(int)Random.Range (0, matches.Count)];
	}

	public Tile getFirstTileWithCover(Tile start, Tile end){
		Tile returnVal = null;

		return raytrace (start, end, Tile.Cover.Part, null, true);
	}

	//checking cover
	public Tile.Cover getCover(Unit sourceUnit, Unit targetUnit){
		//just get the line from the source to the target
		return getCover (sourceUnit.CurTile, targetUnit.CurTile);
	}

	public Tile.Cover getCover(Tile sourceTile, Tile targetTile){
		//get the direction
		float dist = dm.getDist(sourceTile, targetTile); // sourceTile.Pos.getDist(targetTile.Pos);
		Vector3 dir3 = targetTile.Pos.getV3() - sourceTile.Pos.getV3();	
		Vector2 dir = new Vector2(dir3.x, dir3.y);

		//debug line
		//Debug.DrawLine(sourceTile.Pos.getV3(), sourceTile.Pos.getV3()+new Vector3(dir.x, dir.y, 0), Color.green);

		//check for full cover
		if (raytraceFloat (sourceTile, targetTile, Tile.Cover.Full) != null) {
			return Tile.Cover.Full;
		}
//		if (raytrace (sourceTile, targetTile, Tile.Cover.Full) != null) {
//			return Tile.Cover.Full;
//		}

		//and for partial cover
		//only tiles directly adjacent to a unit should provide partial cover
		List<Tile> tilesThatCanCoverTarget = new List<Tile>();
		for (int i = 0; i < 4; i++) {
			if (targetTile.Adjacent [i] != null) {
				if (targetTile.Adjacent [i].CoverVal == Tile.Cover.Part) {
					tilesThatCanCoverTarget.Add (targetTile.Adjacent [i]);
				}
			}
		}
		if (raytrace (sourceTile, targetTile, Tile.Cover.Part, tilesThatCanCoverTarget, false) != null) {
			return Tile.Cover.Part;
		}

		//if nothing hit, then there is no cover
		return Tile.Cover.None;
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

	//shooting rays
	//from http://playtechs.blogspot.ca/2007/03/raytracing-on-grid.html
	public Tile raytrace(Tile tileA, Tile tileB, Tile.Cover coverLevelToCheck, List<Tile> onlyBarriersToCheck = null, bool checkCoverLevelOrHigher = false)
	{
		//Debug.Log ("start ray from " + tileA.Pos.getV2 () + " to " + tileB.Pos.getV2 ());
		int dx = Mathf.Abs(tileB.Pos.x - tileA.Pos.x);
		int dy = Mathf.Abs(tileB.Pos.y - tileA.Pos.y);
		int x = tileA.Pos.x; // x0;
		int y = tileA.Pos.y; //y0;
		int n = 1 + dx + dy;
		int x_inc = (tileB.Pos.x > tileA.Pos.x) ? 1 : -1;
		int y_inc = (tileB.Pos.y > tileA.Pos.y) ? 1 : -1;
		int error = dx - dy;
		dx *= 2;
		dy *= 2;

		for (; n > 0; --n)
		{
			//grid [x, y].setHighlighted (true, Color.cyan);

			if (x >= 0 && x < cols && y >= 0 && y < rows) {
				bool coverMatches = grid [x, y].CoverVal == coverLevelToCheck;
				if (checkCoverLevelOrHigher) {
					coverMatches = (int)grid [x, y].CoverVal >= (int)coverLevelToCheck;
				}
				if (coverMatches) {

					bool canReturnThisTile = true;
					if (onlyBarriersToCheck != null) {
						canReturnThisTile = false;
						foreach (Tile t in onlyBarriersToCheck) {
							if (t.Pos.x == x && t.Pos.y == y) {
								canReturnThisTile = true;
							}
						}
					}

					//do not return either of the source tiles
					if (x == tileA.Pos.x && y == tileA.Pos.y) {
						canReturnThisTile = false;
					}
					if (x == tileB.Pos.x && y == tileB.Pos.y) {
						canReturnThisTile = false;
					}

					if (canReturnThisTile) {
						//Debug.DrawLine (new Vector3 (tileA.Pos.x, tileA.Pos.y, 0), new Vector3 (tileB.Pos.x, tileB.Pos.y, 0), Color.red);
						return grid [x, y];
					}
				}
			}

			if (error > 0)
			{
				x += x_inc;
				error -= dy;
			}
			else
			{
				y += y_inc;
				error += dx;
			}
		}

		//Debug.DrawLine (new Vector3 (tileA.Pos.x, tileA.Pos.y, 0), new Vector3 (tileB.Pos.x, tileB.Pos.y, 0), Color.green);
		return null;
	}

	//also from http://playtechs.blogspot.ca/2007/03/raytracing-on-grid.html
	Tile raytraceFloat(Tile tileA, Tile tileB, Tile.Cover coverLevelToCheck){
		Tile lastEncounteredTile = null;
		float padding = 0.2f;
		Vector2[] offsets = {new Vector2(0,0), new Vector2(padding, 0), new Vector2(-padding, 0), new Vector2(0, padding), new Vector2(0, -padding)};

		for (int i = 0; i < offsets.Length; i++) {
			Tile result = raytraceFloat (tileA.Pos.x+offsets[i].x, tileA.Pos.y+offsets[i].y, tileB.Pos.x, tileB.Pos.y, coverLevelToCheck);
			if (result == null) {
				return null;
			} else {
				lastEncounteredTile = result;
			}
		}
		return lastEncounteredTile;
	}
	Tile raytraceFloat(float x0, float y0, float x1, float y1, Tile.Cover coverLevelToCheck)
	{
		float offset = 0.5f;
		x0 += offset;
		x1 += offset;
		y0 += offset;
		y1 += offset;

		//Debug.DrawLine (new Vector3 (x0, y0, 1), new Vector3 (x1, y1, 1), Color.green);

		float dx = Mathf.Abs(x1 - x0);
		float dy = Mathf.Abs(y1 - y0);

		int x = (int)(Mathf.Floor(x0));
		int y = (int)(Mathf.Floor(y0));

		int n = 1;
		int x_inc, y_inc;
		float error;

		if (dx == 0)
		{
			x_inc = 0;
			error = Mathf.Infinity;// std::numeric_limits<double>::infinity();
		}
		else if (x1 > x0)
		{
			x_inc = 1;
			n += (int)(Mathf.Floor(x1)) - x;
			error = (Mathf.Floor(x0) + 1 - x0) * dy;
		}
		else
		{
			x_inc = -1;
			n += x - (int)Mathf.Floor(x1);
			error = (x0 - Mathf.Floor(x0)) * dy;
		}

		if (dy == 0)
		{
			y_inc = 0;
			error -= Mathf.Infinity;
		}
		else if (y1 > y0)
		{
			y_inc = 1;
			n += (int)(Mathf.Floor(y1)) - y;
			error -= (Mathf.Floor(y0) + 1 - y0) * dx;
		}
		else
		{
			y_inc = -1;
			n += y - (int)(Mathf.Floor(y1));
			error -= (y0 - Mathf.Floor(y0)) * dx;
		}

		for (; n > 0; --n)
		{
			//visit(x, y);
			if (x >= 0 && x < cols && y >= 0 && y < rows) {
				if (grid [x, y].CoverVal == coverLevelToCheck) {
					//Debug.DrawLine (new Vector3 (x0-offset, y0-offset, 0), new Vector3 (x1-offset, y1-offset, 0), Color.red);
					return grid [x, y];
				}
			}

			if (error > 0)
			{
				y += y_inc;
				error -= dx;
			}
			else
			{
				x += x_inc;
				error += dy;
			}
		}
		//Debug.DrawLine (new Vector3 (x0-offset, y0-offset, 0), new Vector3 (x1-offset, y1-offset, 0), Color.green);
		return null;
	}


	//*************
	//AI stuff
	//*************
	public List<MoveInfo> getAllMovesForUnit(int unitID){
		List<MoveInfo> moves = new List<MoveInfo> ();
		int actionsLeft = units [unitID].ActionsLeft;
		List<string> cardIDsUsed = new List<string> ();
		for (int i = 0; i < units [unitID].deck.Hand.Count; i++) {
			string idName = units [unitID].deck.Hand [i].idName;
			if (actionsLeft >= units [unitID].deck.Hand [i].getNumActionsNeededToPlay ()) {
				//make sure we have not gotten moves for the same card alread
				if (cardIDsUsed.Contains (idName) == false) {
					moves.AddRange (getAllMovesForCard (unitID, i));
					cardIDsUsed.Add (idName);
				}
			}
		}

		//if the unit has at least one action, pass is also a viable move
		if (actionsLeft >= 1) {
			MoveInfo thisMove = new MoveInfo (unitID);
			thisMove.passMove = true;
			moves.Add (thisMove);
		}

		//Debug.Log ("found " + moves.Count + " moves");
		return moves;
	}
	public List<MoveInfo> getAllMovesForCard(int unitID, int cardID){
		List<MoveInfo> moves = new List<MoveInfo> ();
		clearHighlights ();
		Card thisCard = units [unitID].deck.Hand [cardID];
		thisCard.selectCard (true);

		//go through all highlighted tiles
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid [x, y].IsHighlighted) {
					MoveInfo move = new MoveInfo (unitID, thisCard.idName, grid [x, y].Pos);
					moves.Add (move);
				}
			}
		}

		//go through all highlighted units
		for (int i = 0; i < units.Count; i++) {
			if (units [i].IsHighlighted) {
				MoveInfo move = new MoveInfo (unitID, thisCard.idName, units[i].CurTile.Pos);
				moves.Add (move);
			}
		}

		//Debug.Log ("found " + moves.Count + " moves for " + units [unitID].deck.Hand [cardID].idName);
		thisCard.cancel ();
		clearHighlights ();
		return moves;
	}

	public int getUnitID(Unit unit){
		for(int i=0; i<units.Count; i++){
			if (units[i] == unit){
				return i;
			}
		}
		Debug.Log ("UNIT NOT FOUND");
		return -1;
	}

	public void compareBoardSates(Board oldBoard, Unit curUnit, ref TurnInfo info, bool printInfo){
		Profiler.BeginSample("compare boards");

		//create unit lists. These lists should line up exactly. it is bad if they don't
		Profiler.BeginSample("making lists");
		List<Unit> curAllies = new List<Unit>();
		List<Unit> oldAllies = new List<Unit>();
		List<Unit> curEnemies = new List<Unit> ();
		List<Unit> oldEnemies = new List<Unit>();
		Profiler.EndSample ();

		bool rootingForAI = !curUnit.isPlayerControlled;

		info.val = 0;

		//sepeare them into allies on enemies depending on which side we are trying to get a move for
		Profiler.BeginSample("sepearte allies and foes");
		foreach (Unit unit in units) {
			if (unit.isPlayerControlled == rootingForAI) {
				curEnemies.Add (unit);
			} else {
				curAllies.Add (unit);
			}
		}
		foreach (Unit unit in oldBoard.units) {
			if (unit.isPlayerControlled == rootingForAI) {
				oldEnemies.Add (unit);
			} else {
				oldAllies.Add (unit);
			}
		}

		//sanity check
		if (oldEnemies.Count != curEnemies.Count || oldAllies.Count > curAllies.Count) {
			Debug.Log ("BAD BAD BAD BAD");
		}
		if (oldAllies.Count < curAllies.Count) {
			int dif = curAllies.Count - oldAllies.Count;
			info.val += curUnit.aiProfile.newAlliesWeight * dif;
			if (printInfo) {
			
				Debug.Log (dif + " new allies with weight of "+curUnit.aiProfile.newAlliesWeight);
				for (int i = oldAllies.Count; i < curAllies.Count; i++) {
					Debug.Log ("  " + curAllies [i].unitName);
				}
			}
		}
		Profiler.EndSample ();

		//going through enemies
		Profiler.BeginSample("go through enemies");
		int totalEnemyDamage = 0;
		int totalEnemyHeal = 0;
		int numEnemiesKilled = 0;
		int numEnemiesAided = 0;
		int numEnemiesCursed = 0;
		float deltaEnemyGoodCharms = 0;
		float deltaEnemyBadCharms = 0;
		for (int i = 0; i < oldEnemies.Count; i++) {
			//will enemies be damaged?
			if (curEnemies[i].health < oldEnemies[i].health){
				totalEnemyDamage += oldEnemies [i].health - curEnemies [i].health; 
			}
			//will they be healed?
			if (curEnemies [i].health > oldEnemies [i].health) {
				totalEnemyHeal +=  curEnemies [i].health - oldEnemies [i].health; 
			}

			//will enemies be killed?
			if (curEnemies[i].isDead && !oldEnemies[i].isDead){
				numEnemiesKilled++;
			}

			//were enemies aided (bad!)
			//this value gets reset at the start of each simulation, so the oldEnemy value will always be 0
			numEnemiesAided += curEnemies [i].aiSimHasBeenAidedCount;
			//what about cursed?
			numEnemiesCursed += curEnemies[i].aiSimHasBeenCursedCount;

			//tally up the charms (subtracting the old values from the current values to see if there was change) 
			foreach(Charm c in curEnemies[i].Charms){
				deltaEnemyGoodCharms += c.aiGoodCharmPoints;
				deltaEnemyBadCharms += c.aiBadCharmPoints;
			}
			foreach(Charm c in oldEnemies[i].Charms){
				deltaEnemyGoodCharms -= c.aiGoodCharmPoints;
				deltaEnemyBadCharms -= c.aiBadCharmPoints;
			}
		}
			
		//add it to the total
		info.val += totalEnemyDamage * curUnit.aiProfile.totalEnemyDamageWeight;
		info.val += totalEnemyHeal * curUnit.aiProfile.totalEnemyHealWeight;
		info.val += numEnemiesKilled * curUnit.aiProfile.numEnemiesKilledWeight;
		info.val += numEnemiesAided * curUnit.aiProfile.numEnemiesAidedWeight;
		info.val += deltaEnemyGoodCharms * curUnit.aiProfile.deltaEnemyGoodCharmWeight;
		info.val += deltaEnemyBadCharms * curUnit.aiProfile.deltaEnemyBadCharmWeight;
		Profiler.EndSample ();

		//going through allies
		Profiler.BeginSample("go through allies");
		int totalAllyDamage = 0;
		int totalAllyHeal = 0;
		int numAlliesKilled = 0;
		int numAlliesAided = 0;
		int numAlliesCursed = 0;
		float deltaAllyGoodCharms = 0;
		float deltaAllyBadCharms = 0;
		for (int i = 0; i < oldAllies.Count; i++) {
			//will allies be damaged?
			if (curAllies [i].health < oldAllies [i].health) {
				totalAllyDamage += oldAllies [i].health - curAllies [i].health;
				//Debug.Log ("i feel it for " + (oldAllies [i].health - curAllies [i].health));
			}
			//will they be healed?
			if (curAllies [i].health > oldAllies [i].health) {
				totalAllyHeal += curAllies [i].health - oldAllies [i].health;
			}

			//will allies be killed?
			if (curAllies[i].isDead && !oldAllies[i].isDead){
				numAlliesKilled++;
			}

			//were allies aided? (good!)
			numAlliesAided += curAllies[i].aiSimHasBeenAidedCount;
			//what about cursed?
			numAlliesCursed += curAllies[i].aiSimHasBeenCursedCount;

			//tally up the charms (subtracting the old values from the current values to see if there was change) 
			//Debug.Log(curAllies[i].Charms.Count+" charms for ally "+oldAllies[i].CurTile.Pos.x+","+oldAllies[i].CurTile.Pos.y);
			//if (printInfo)Debug.Log("charms for ally "+oldAllies[i].CurTile.Pos.x+","+oldAllies[i].CurTile.Pos.y);
			foreach(Charm c in curAllies[i].Charms){
				deltaAllyGoodCharms += c.aiGoodCharmPoints;
				deltaAllyBadCharms += c.aiBadCharmPoints;
				if(printInfo)Debug.Log (c.idName+" " + c.aiBadCharmPoints);
			}
			foreach(Charm c in oldAllies[i].Charms){
				deltaAllyGoodCharms -= c.aiGoodCharmPoints;
				deltaAllyBadCharms -= c.aiBadCharmPoints;
			}
		}


//		if (numAlliesAided > 0) {
//			Debug.Log (numAlliesAided + " aided and " + curUnit.ActionsLeft + " actions left");
//		}

		//add it to the total
		info.val += totalAllyDamage * curUnit.aiProfile.totalAllyDamageWeight;
		info.val += totalAllyHeal * curUnit.aiProfile.totalAllyHealWeight;
		info.val += numAlliesKilled * curUnit.aiProfile.numAlliesKilledWeight;
		info.val += numAlliesAided * curUnit.aiProfile.numAlliesAidedWeight;
		info.val += numAlliesCursed * curUnit.aiProfile.numAlliesCursedWeight;
		info.val += deltaAllyGoodCharms * curUnit.aiProfile.deltaAllyGoodCharmWeight;
		info.val += deltaAllyBadCharms * curUnit.aiProfile.deltaAllyBadCharmWeight;
		Profiler.EndSample ();

		if (printInfo) {
			Debug.Log ("AI turn info for " + curUnit.unitName);
			Debug.Log ("foe damaged " + totalEnemyDamage);
			Debug.Log ("num foes killed " + numEnemiesKilled);
			Debug.Log ("foe heal " + totalEnemyHeal);
			Debug.Log ("foes aided " + numEnemiesAided);
			Debug.Log ("foes cursed " + numEnemiesCursed);
			Debug.Log ("foe good charm delta " + deltaEnemyGoodCharms);
			Debug.Log ("foe bad charm delta " + deltaEnemyBadCharms);

			Debug.Log ("ally damaged " + totalAllyDamage);
			Debug.Log ("num allies killed " + numAlliesKilled);
			Debug.Log ("ally heal " + totalAllyHeal);
			Debug.Log ("allies aided " + numAlliesAided+ "  weight " + curUnit.aiProfile.numAlliesAidedWeight);
			Debug.Log ("allies cursed " + numAlliesCursed);
			Debug.Log ("ally good charm delta " + deltaAllyGoodCharms);
			Debug.Log ("ally bad charm delta " + deltaAllyBadCharms);
		}

		//checking distance stuff
		Profiler.BeginSample("distance stuff");
		for (int i = 0; i < oldAllies.Count; i++) {

			float minDistFromClosest = curAllies [i].aiProfile.preferedDistToClosestEnemy - curAllies [i].aiProfile.acceptableDistanceRangeToClosestEnemy;
			float maxDistFromClosest = curAllies [i].aiProfile.preferedDistToClosestEnemy + curAllies [i].aiProfile.acceptableDistanceRangeToClosestEnemy;

			float oldClosestDistToEnemy = 9999;
			float newClosestDistToEnemy = 9999;

			foreach (Unit enemy in oldEnemies) {
				float dist = dm.getDist (enemy.CurTile, oldAllies [i].CurTile);		// enemy.CurTile.Pos.getDist (oldAllies [i].CurTile.Pos);
				if (dist < oldClosestDistToEnemy)		oldClosestDistToEnemy = dist;
			}
			foreach (Unit enemy in curEnemies) {
				float dist = dm.getDist (enemy.CurTile, curAllies [i].CurTile);		// enemy.CurTile.Pos.getDist (curAllies [i].CurTile.Pos);
				if (dist < newClosestDistToEnemy)		newClosestDistToEnemy = dist;
			}

			float oldVal = 0;
			if (oldClosestDistToEnemy < minDistFromClosest) {
				oldVal = oldClosestDistToEnemy - minDistFromClosest;
			}
			if (oldClosestDistToEnemy > maxDistFromClosest) {
				oldVal = maxDistFromClosest - oldClosestDistToEnemy;
			}

			float newVal = 0;
			if (newClosestDistToEnemy < minDistFromClosest) {
				newVal = newClosestDistToEnemy - minDistFromClosest;
			}
			if (newClosestDistToEnemy > maxDistFromClosest) {
				newVal = maxDistFromClosest - newClosestDistToEnemy;
			}

			float change = newVal - oldVal;
			info.val += change * curAllies[i].aiProfile.distanceToEnemiesWeight;

//			if (printInfo) {
//				Debug.Log ("min dist: " + minDistFromClosest);
//				Debug.Log ("max dist: " + maxDistFromClosest);
//				Debug.Log ("old closest: " + oldClosestDistToEnemy);
//				Debug.Log ("new closest: " + newClosestDistToEnemy);
//				Debug.Log ("old val: " + oldVal);
//				Debug.Log ("new val: " + newVal);
//				Debug.Log (curAllies[i].unitName+" dist to enemy change: " + change);
//			}
		}
		Profiler.EndSample ();

		//how has ally cover changed?
		Profiler.BeginSample("cover checks");
		for (int i = 0; i < oldAllies.Count; i++) {
			Tile.Cover oldLowestCover = Tile.Cover.Full;
			foreach (Unit enemy in oldEnemies) {
				if (!enemy.isDead) {
					Tile.Cover thisCover = getCover (enemy.CurTile, oldAllies [i].CurTile);
					if ((int)thisCover < (int)oldLowestCover) {
						oldLowestCover = thisCover;
					}	
				}
			}

			Tile.Cover newLowestCover = Tile.Cover.Full;
			foreach (Unit enemy in curEnemies) {
				if (!enemy.isDead) {
					Tile.Cover thisCover = getCover (enemy.CurTile, curAllies [i].CurTile);
					if ((int)thisCover < (int)newLowestCover) {
						newLowestCover = thisCover;
					}	
				}
			}

			//neither cover can be considered higher than what is actually usable on that tile
			if ((int)oldLowestCover > (int)oldAllies [i].CurTile.getHighestAdjacentCover ()) {
				oldLowestCover = oldAllies [i].CurTile.getHighestAdjacentCover ();
			}
			if ((int)newLowestCover > (int)curAllies [i].CurTile.getHighestAdjacentCover ()) {
				newLowestCover = curAllies [i].CurTile.getHighestAdjacentCover ();
			}

			//float changeVal = curAllies [i].aiProfile.coverChange [(int)oldLowestCover, (int)newLowestCover];
			float changeVal = curUnit.aiProfile.coverChange [(int)oldLowestCover, (int)newLowestCover];
			info.val += changeVal;

			if (printInfo) {
				//Debug.Log ("ally " + i + " " + curAllies [i] + " was " + oldLowestCover + " is " + newLowestCover + " for val "+changeVal);
			}
		}
		Profiler.EndSample ();

		//were any prefered cards played?
		//Debug.Log("see what sparkles");
		Profiler.BeginSample("check preferred cards");
		for (int i = 0; i < info.moves.Count; i++) {
			if (!info.moves [i].passMove) {
				string cardName = info.moves [i].cardIDName;
				//Debug.Log ("  played " + info.moves [i].cardIDName);
				if (curUnit.aiProfile.preferedCardWeights.ContainsKey(cardName)){
					info.val += curUnit.aiProfile.preferedCardWeights [cardName];
					//Debug.Log("played "+cardName+" worth "+curUnit.aiProfile.preferedCardWeights [cardName]);
				}
			}
		}
		Profiler.EndSample ();

		//what is the average cover for enemies?

		//are any enemies flanked?

		//have allies retreated or advanced?

		Profiler.EndSample ();
	}

	//*************
	//testing
	//*************
	public void print(){
		string output ="";
		for (int y = rows-1; y >= 0; y--) {
			string thisLine = "";
			for (int x = 0; x < cols; x++) {
				char thisChar = '-';
				if (grid [x, y].CoverVal == Tile.Cover.Full) {
					thisChar = 'X';
				}
				if (grid [x, y].CoverVal == Tile.Cover.Part) {
					thisChar = '/';
				}
				foreach (Unit unit in units) {
					if (unit.CurTile.Pos.x == x && unit.CurTile.Pos.y == y) {
						if (unit.isPlayerControlled) {
							thisChar = 'P';
						} else {
							if (unit.isDead) {
								thisChar = 'D';
							}
							if (unit.health == 0) {
								thisChar = '0';
							}
							if (unit.health == 1) {
								thisChar = '1';
							}
							if (unit.health == 2) {
								thisChar = '2';
							}
							if (unit.health == 3) {
								thisChar = '3';
							}
							if (unit.health == 4) {
								thisChar = '4';
							}
							if (unit.health == 5) {
								thisChar = '5';
							}
							if (unit.health == 6) {
								thisChar = '6';
							}
							if (unit.health == 7) {
								thisChar = '7';
							}
						}
					}
				}
				thisLine += thisChar;
			}
			thisLine += "\n";
			output += thisLine;
		}

		Debug.Log (output);
	}

	//clean up

	public void returnToPool(){
		for (int i = 0; i < units.Count; i++) {
			units [i].returnToPool ();
		}
		ObjectPooler.instance.retireBoard (this);
	}

	//setters and getters

	public Tile[,] Grid{
		get{
			return this.grid;
		}
	}

	public int Cols {
		get {
			return this.cols;
		}
	}

	public int Rows {
		get {
			return this.rows;
		}
	}

}
