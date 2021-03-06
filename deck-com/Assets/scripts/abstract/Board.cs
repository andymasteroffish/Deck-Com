﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

public class Board {

	public static int debugBoardCount;	//testing
	public static int debugCounter;	//testing
	
	private LevelGen levelGen;

	public int cols, rows;

	public float diagonalVal = 1.41f;

	private Tile[,] grid = null;		//this will be a pointer to the parent grid or to novelGrid
	private Tile[,] novelGrid = null;	//this is the fesh grid waiting in the wings
	private bool usingParentGrid;

	public List<Unit> units;
	public List<Loot> loot;
	public List<PassiveObject> passiveObjects;

	public int partialCoverDamageReduction = 1;
	public float fullCoverDamagePrc = 0.5f;

	public bool isAISim;

	public DistanceManager dm;

	//these values are used for comparing boards and may not be up to date for other parts of the game
	//as a result, they should not be used outside of comparing boards
	public List<Unit> alliesForBoardCompare = null;
	public List<Unit> enemiesForBoardCompare = null;

	public Board(){
		debugBoardCount = 0;
	}

	public void mainBoardSetup(){
		isAISim = false;
		usingParentGrid = false;
		levelGen = new LevelGen ();
		units = new List<Unit> ();
		loot = new List<Loot> ();
		passiveObjects = new List<PassiveObject> ();
	}

	public void reset(int curLevelNum, int curAreaNum){
		//clear ();
		if (GameManagerTacticsInterface.instance.debugIgnoreStandardSpawns) {
			Debug.Log ("tets em daddy");
			grid = levelGen.getTestLevel (GameManagerTacticsInterface.instance.debugMapName);
		} else {
			grid = levelGen.getLevel (curLevelNum, curAreaNum);
		}

		loot.Clear ();
		passiveObjects.Clear ();

		cols = grid.GetLength (0);
		rows = grid.GetLength (1);

		dm = new DistanceManager (cols, rows);

		setupTilesOnNewGrid (grid);

		clearHighlights ();
	}

	//creating a board for AI stuff
	public void setFromParent(Board parent){
		Profiler.BeginSample ("board setup");
		isAISim = true;

		debugBoardCount++;

		levelGen = null;
		//info
		cols = parent.cols;
		rows = parent.rows;
		partialCoverDamageReduction = parent.partialCoverDamageReduction;
		fullCoverDamagePrc = parent.fullCoverDamagePrc;
		diagonalVal = parent.diagonalVal;

		dm = parent.dm;

		//tiles
		grid = parent.grid;
		usingParentGrid = true;

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

		//passive objects might matter
		passiveObjects = new List<PassiveObject>();
		for (int i = 0; i < parent.passiveObjects.Count; i++) {
			//I don't think store keys actually need to be created for AI boards
//			if (parent.passiveObjects [i].type == PassiveObject.PassiveObjectType.StoreKey) {
//				StoreKey newObj = new StoreKey ((StoreKey)parent.passiveObjects [i]);
//				passiveObjects.Add (newObj);
//			}
		}

		Profiler.EndSample ();
	}

	void copyParentGridToNewGrid(){
		usingParentGrid = false;

		//if this is the first time this pooled board has needed a novel grid, let's make one
		if (novelGrid == null) {
			Profiler.BeginSample ("making a fresh novel grid");

			novelGrid = new Tile[cols, rows];
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					novelGrid [x, y] = new Tile (grid [x, y]);
				}
			}

			setupTilesOnNewGrid (novelGrid);

			Profiler.EndSample ();
		} 

		//otherwise, just copy the parent grid to the novel grid
		else {
			Profiler.BeginSample ("copying old grid to novel grid");
			//JUST GOING THROUGH THIS FOR LOOP IS TAKING A HUGELY LONG TIME. LONGER THAN THE TIME SPENT RUNNING setFromParent()
			for (int x = 0; x < cols; x++) {
				for (int y = 0; y < rows; y++) {
					novelGrid [x, y].setFromParent(grid [x, y]);
				}
			}
			Profiler.EndSample ();
		}

		//move all units to the new grid
		foreach (Unit unit in units) {
			TilePos pos = unit.CurTile.Pos;
			unit.moveTo (grid [pos.x, pos.y]);
		}

		//and then set the novel grid as the grid
		grid = novelGrid;
	}

	void setupTilesOnNewGrid(Tile[,] newGrid){
		Profiler.BeginSample ("setup tiles on new grid");
		//give them some relevant info
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				Tile[] adjacent = new Tile[] {null, null, null, null};
				if (x < cols-1)	adjacent [(int)Tile.Direction.Right]= newGrid [x+1, y];
				if (x > 0)		adjacent [(int)Tile.Direction.Left] = newGrid [x-1, y];
				if (y < rows-1)	adjacent [(int)Tile.Direction.Up] 	= newGrid [x, y + 1];
				if (y > 0)		adjacent [(int)Tile.Direction.Down] = newGrid [x, y - 1];

				Tile[] adjacentIncludingDiag = new Tile[] {adjacent[0], adjacent[1], adjacent[2], adjacent[3], null, null, null, null};
				//up, right
				if (x < cols-1 	&& y < rows-1)	adjacentIncludingDiag [4]= newGrid [x+1, y+1];
				//up, left
				if (x > 0	 	&& y < rows-1)	adjacentIncludingDiag [5]= newGrid [x-1, y+1];
				//down, right
				if (x < cols-1 	&& y > 0)		adjacentIncludingDiag [6]= newGrid [x+1, y-1];
				//down, left
				if (x > 0	 	&& y > 0)		adjacentIncludingDiag [7]= newGrid [x-1, y-1];

				newGrid [x, y].setInfo (adjacent, adjacentIncludingDiag);
				newGrid [x, y].createVisibilityGrid (cols, rows);
			}
		}
		Profiler.EndSample ();
	}

	//NOT USED FOR INTO THE BREAHC MODE
	public void resetUnitsAndLoot(int curAreaNum){
		foreach(Unit unit in units){
			unit.reset ();
		}

		int lootToDistribute = GameManagerTacticsInterface.instance.lootPerLevel + (int)Random.Range (0, (float)GameManagerTacticsInterface.instance.potentialBonusLootPerLevel+1);
		//intro level gets a set amount
		if (curAreaNum == 0) {
			lootToDistribute = 2;
		}

		if (GameManagerTacticsInterface.instance.debugAllFoesHaveLoot) {
			lootToDistribute = 999;
		}
			
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
	}

	public void addFoes(List<Unit> newFoes, int curAreaNum, int numLoot){
		//reset 'em
		foreach (Unit foe in newFoes) {
			//foes that just landed should only be able to move
			if (GameManagerTacticsInterface.instance.gm.TurnNum > 0) {
				foe.resetWithCard ("patrol_move_4");
			} else {
				foe.reset ();
			}
			foe.resetRound ();
		}
		//give one of them loot if we should
		int lootHolderID = (int)Random.Range (0, newFoes.Count);
		for (int i=0; i<numLoot; i++){
			Loot thisLoot = new Loot (newFoes [lootHolderID], curAreaNum);
			loot.Add (thisLoot);
			lootHolderID++;
			if (lootHolderID >= newFoes.Count) {
				lootHolderID = 0;
			}
		}

		//add 'em
		units.AddRange(newFoes);
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
		Profiler.BeginSample ("resolve and return");
		//Debug.Log ("new resolve for unit with " + units [move.unitID].ActionsLeft + " actions left");
		units [move.unitID].isActingAIUnit = true;
		//Profiler.BeginSample ("creating board");
		Board newBoard = ObjectPooler.instance.getBoard();//new Board(this);
		newBoard.setFromParent(this);
		//Profiler.EndSample ();

		newBoard.resolveMove (move);
		Profiler.EndSample ();
		return newBoard;
	}

	//checks after any card has been played
	public void doPostCardPlayActions(){
		//Debug.Log ("resolve");
		//see if charms need to do anythng
		for (int u = units.Count-1; u >= 0; u--) {
			for (int c = units [u].Charms.Count - 1; c >= 0; c--) {
				units [u].Charms [c].anyActionTaken ();
			}
		}

		//does this affect passive objects?
		for (int i = passiveObjects.Count-1; i>=0; i--) {
			passiveObjects [i].checkBoard (this);
			if (passiveObjects [i].isDone) {
				passiveObjects.RemoveAt (i);
			}
		}

		if (!isAISim) {
			GameManagerTacticsInterface.instance.gm.doUserSidePostCardPlayActions ();
		}
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
			unit.showCoverLevelIcon = -1;
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
		if (!deadUnit.isAISimUnit) {
			updateVisible ();
		}

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
		
		//check if any change is needed
		if (target.CoverVal == newCoverVal) {
			return;
		}

		//Debug.Log ("change cover " + target.Pos.x + "," + target.Pos.y + "  sim: " + isAISim);

		Profiler.BeginSample("change cover");

		//if we were using the parent grid, we now need to update that
		if (usingParentGrid) {
			//Debug.Log ("make a new grid");
			copyParentGridToNewGrid ();
		}

		//redirect the target to the new grid in case it changed from the parent grid to the novel one
		TilePos pos = target.Pos;
		target = grid [pos.x, pos.y];

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

		//have each unit check their new visibility
		//it is way too goddamn slow to do this for every unit on AI boards.
		if (!isAISim) {
			foreach (Unit unit in units) {
				unit.setVisibleTiles ();
			}
		}

		Profiler.EndSample ();
	}

	//telling the tiles if any player unit can see them
	public void updateVisible(){
		//Debug.Log ("updating visible on frame "+Time.frameCount);
		//mark all tiles that are visible to player units
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				grid [x, y].isVisibleToPlayer = false;
				foreach (Unit unit in units) {
					if (unit.isPlayerControlled && !unit.isDead && !grid[x,y].isVisibleToPlayer && unit.visibleTiles != null) {
						//Debug.Log ("unit " + unit.unitName + " has sight " + unit.sightRange + " and currently sees " + unit.visibleTiles.Count + " tiles");
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
			//check if this would wake up any AI units in patrol mode
			if (!unit.isPlayerControlled) {
				if (grid [unit.CurTile.Pos.x, unit.CurTile.Pos.y].isVisibleToPlayer && unit.curBehavior == Unit.BehaviorMode.Patrolling) {
					unit.wakeUp ();
				}
			}
		}
	}

	//showing which units will be visible from a given tile
	public void updateUnitVisibilityIconsFromTile(Tile sourceTile, Unit sourceUnit){
		List<Tile> tiles = getTilesInVisibleRange (sourceTile, sourceUnit.getSightRange());

		//go through all units and see if they are on any of those tiles
		foreach (Unit unit in units) {
			unit.showVisibilityIcon = false;
			unit.showCoverLevelIcon = -1;
			if (unit != sourceUnit && unit.getIsVisibleToPlayer()) {
				foreach (Tile tile in tiles) {
					if (unit.CurTile == tile) {
						unit.showVisibilityIcon = true;
						unit.showCoverLevelIcon = (int)getCover (sourceTile, unit.CurTile);
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
	public List<Unit> highlightUnitsInVisibleRange(Tile source, float range, bool sourceIsPlayer, bool includeMyTeam, bool includeOtherTeam, Color col){
		bool includePlayer = false;
		bool includeAI = false;
		if (sourceIsPlayer) {
			includePlayer = includeMyTeam;
			includeAI = includeOtherTeam;
		}
		else {
			includePlayer = includeOtherTeam;
			includeAI = includeMyTeam;
		}
		return highlightUnitsInVisibleRange (source, range, includePlayer, includeAI, col);
	}
		
	public List<Unit> highlightUnitsInVisibleRange(Tile source, float range, bool includePlayer, bool includeAI, Color col){
		clearHighlights ();
		List<Tile> selectable = getTilesInVisibleRange (source, range);
		List<Unit> highlightedUnits = new List<Unit> ();
		foreach (Tile tile in selectable) {
			foreach (Unit unit in units) {
				if (unit.CurTile == tile && !unit.isDead) {
					if ((unit.isPlayerControlled && includePlayer) || (!unit.isPlayerControlled && includeAI)) {
						unit.setHighlighted (true, col);
						highlightedUnits.Add (unit);
					}
				}
			}
		}

		return highlightedUnits;
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
				foreach (Tile t in curTile.AdjacentIncludingDiag) {
					if (t != null) {
						bleedTiles.Add (t);
					}
				}
//				List<Tile> adjacentTiles = getAdjacentTiles (curTile, true, Tile.Cover.Full);
//				foreach (Tile t in adjacentTiles) {
//					bleedTiles.Add (t);
//				}
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
			Debug.Log ("set visible tiles for " + source.unitName + " with vision " + source.getSightRange());
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
		highlightTilesVisibleToUnit (source, col, -1, true);
//		clearHighlights ();
//		turnOffUnitMouseColliders ();
//		if (source.visibleTiles == null) {
//			Debug.Log ("no vis tiles, so set visible tiles for " + source.unitName + " with vision " + source.getSightRange());
//			source.setVisibleTiles ();
//		}
//		foreach (Tile tile in source.visibleTiles) {
//			tile.setHighlighted (true, col);
//		}
	}

	//ignoreCover level is an int to allow it to be -1, meaning allow all cover levels
	public void highlightTilesVisibleToUnit(Unit source, Color col, int ignoreCoverLevel, bool canContainUnit){
		clearHighlights ();
		turnOffUnitMouseColliders ();
		if (source.visibleTiles == null) {
			Debug.Log ("no vis tiles, so set visible tiles for " + source.unitName + " with vision " + source.getSightRange());
			source.setVisibleTiles ();
		}
		foreach (Tile tile in source.visibleTiles) {
			bool skip = false;

			if ((int)tile.CoverVal== ignoreCoverLevel) {
				skip = true;
			}

			//this one is a bit more expensive so no point in doing it if the tile has been ruled out already
			if (!skip && !canContainUnit && getUnitOnTile (tile) != null) {
				skip = true;
			}

			//if it passed all of the checks, add it
			if (!skip) {
				tile.setHighlighted (true, col);
			}
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
								if (!unit.isDead && unit.CurTile.Pos == grid [x, y].Pos) {
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
			if (info.pos.x != sourceX || info.pos.y != sourceY) {	//don't include the source
				returnTiles.Add (info.tile);
			}
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

	public List<Unit> getAdjacentUnits(Tile start, bool includeDiagonal){
		Tile[] tiles = includeDiagonal ? start.AdjacentIncludingDiag : start.Adjacent;
		//List<Tile> tiles = getAdjacentTiles (start, includeDiagonal, Tile.Cover.Full);
		List<Unit> units = new List<Unit> ();

		for (int i = 0; i < tiles.Length; i++) {
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

	public List<Unit> getAllHighlightedUnits(){
		List<Unit> returnVal = new List<Unit> ();
		foreach (Unit unit in units) {
			if (unit.IsHighlighted) {
				returnVal.Add (unit);
			}
		}
		return returnVal;
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

		if (matches.Count == 0) {
			return null;
		}

		return matches [(int)Random.Range (0, matches.Count)];
	}

	public List<Tile> GetAllTilesWithSpawnProperty(Tile.SpawnProperty property){
		List<Tile> matches = new List<Tile> ();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid [x, y].spawnProperty == property) {
					matches.Add (grid [x, y]);
				}
			}
		}
		return matches ;
	}

	public Tile GetRandomTileWithCoverLevel(Tile.Cover cover){
		List<Tile> matches = new List<Tile> ();
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid [x, y].CoverVal == cover) {
					matches.Add (grid [x, y]);
				}
			}
		}

		if (matches.Count == 0) {
			Debug.Log ("NO TILES WITH COVER " + cover);
			return null;
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

	//right now only adjacent tiles can provide cover. Maybe that's bad?
	public Tile.Cover getCover(Tile sourceTile, Tile targetTile){
		int highestCoverVal = 0;

		//attacks from north
		if (sourceTile.Pos.y-1 > targetTile.Pos.y) {
			Tile coverTile = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Up) );
			if ((int)coverTile.CoverVal > highestCoverVal) {
				highestCoverVal = (int)coverTile.CoverVal;
			}
		}
		//attacks from south
		if (sourceTile.Pos.y+1 < targetTile.Pos.y) {
			Tile coverTile = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Down) );
			if ((int)coverTile.CoverVal > highestCoverVal) {
				highestCoverVal = (int)coverTile.CoverVal;
			}
		}
		//attacks from east
		if (sourceTile.Pos.x-1 > targetTile.Pos.x) {
			Tile coverTile = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Right) );
			if ((int)coverTile.CoverVal > highestCoverVal) {
				highestCoverVal = (int)coverTile.CoverVal;
			}
		}
		//attacks from west
		if (sourceTile.Pos.x+1 < targetTile.Pos.x) {
			Tile coverTile = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Left) );
			if ((int)coverTile.CoverVal > highestCoverVal) {
				highestCoverVal = (int)coverTile.CoverVal;
			}
		}

		//litteral corner cases. If the source is diagonal to the target, get both of the adjacent cover values and use the lowest
		TilePos deltaPos = targetTile.Pos-sourceTile.Pos;
		//source is to bottom left
		if (deltaPos.x == 1 && deltaPos.y == 1) {
			Tile coverTileA = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Left) );
			Tile coverTileB = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Down) );
			highestCoverVal = (int)coverTileA.CoverVal < (int)coverTileB.CoverVal ? (int)coverTileA.CoverVal : (int)coverTileB.CoverVal;
		}
		//source is to bottom right
		if (deltaPos.x == -1 && deltaPos.y == 1) {
			Tile coverTileA = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Right) );
			Tile coverTileB = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Down) );
			highestCoverVal = (int)coverTileA.CoverVal < (int)coverTileB.CoverVal ? (int)coverTileA.CoverVal : (int)coverTileB.CoverVal;
		}
		//source is to top left
		if (deltaPos.x == 1 && deltaPos.y == -1) {
			Tile coverTileA = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Left) );
			Tile coverTileB = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Up) );
			highestCoverVal = (int)coverTileA.CoverVal < (int)coverTileB.CoverVal ? (int)coverTileA.CoverVal : (int)coverTileB.CoverVal;
		}
		//source is to top right
		if (deltaPos.x == -1 && deltaPos.y == -1) {
			Tile coverTileA = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Right) );
			Tile coverTileB = getTileFromPos(targetTile.Pos.addDirection (Tile.Direction.Up) );
			highestCoverVal = (int)coverTileA.CoverVal < (int)coverTileB.CoverVal ? (int)coverTileA.CoverVal : (int)coverTileB.CoverVal;
		}



		//if nothing hit, then there is no cover
		return (Tile.Cover)highestCoverVal;
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

	public Tile getTileFromPos(TilePos pos){
		if (pos.x < 0 || pos.x >= cols || pos.y < 0 || pos.y >= rows) {
			Debug.Log ("BAD POS");
			return null;
		}
		return grid [pos.x, pos.y];
	}


	//*************
	//AI stuff
	//*************
	public List<MoveInfo> getAllMovesForUnit(int unitID){
		//Debug.Log ("getting moves for unit");
		Profiler.BeginSample ("get all AI moves for unit");

		units [unitID].deck.updateCardsDisabled ();
		List<MoveInfo> moves = new List<MoveInfo> ();
		int actionsLeft = units [unitID].ActionsLeft;
		List<string> cardIDsUsed = new List<string> ();
		for (int i = 0; i < units [unitID].deck.Hand.Count; i++) {
			string idName = units [unitID].deck.Hand [i].idName;
			Card thisCard = units [unitID].deck.Hand [i];
			if (actionsLeft >= thisCard.getNumActionsNeededToPlay ()) {
				//make sure we have not gotten moves for the same card alread
				if (cardIDsUsed.Contains (idName) == false) {
					List<MoveInfo> movesForThisCard = getAllMovesForCard (unitID, i);
					filterBadMovesIfApplicable (movesForThisCard, i);
					moves.AddRange (movesForThisCard);
					cardIDsUsed.Add (idName);
				}
			}
		}

		//if the unit has at least one action, pass-turn is also a viable move
		//not when patrolling!
		if (actionsLeft >= 1 && units[unitID].curBehavior != Unit.BehaviorMode.Patrolling) {
			MoveInfo thisMove = new MoveInfo (unitID);
			thisMove.passMove = true;
			moves.Add (thisMove);
		}
		Profiler.EndSample ();
		//Debug.Log ("found " + moves.Count + " moves");
		return moves;
	}
	public List<MoveInfo> getAllMovesForCard(int unitID, int cardID){
		List<MoveInfo> moves = new List<MoveInfo> ();
		clearHighlights ();
		Card thisCard = units [unitID].deck.Hand [cardID];
		if (thisCard.IsDisabled) {
			return moves;
		}
		thisCard.selectCard (true);

		//go through all highlighted tiles
		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				if (grid [x, y].IsHighlighted) {
					MoveInfo move = new MoveInfo (unitID, thisCard.idName, grid [x, y].Pos);
					//Debug.Log (units [unitID].unitName + " can play " + units [unitID].deck.Hand [cardID].idName + " on " + grid [x, y].Pos.x + "," + grid [x, y].Pos.y);
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

	//some cards (like movement cards) can result in lots and lots of potential moves, most of which are obviously bad
	//rather than allow these to create tons of branching trees, let's try and filter out obviousl bad calls
	public void filterBadMovesIfApplicable(List<MoveInfo> moves, int cardID, bool showDebugColors=false){
		Profiler.BeginSample ("filtering out moves");
		int bestVal = 0;
		int numMovesAtBestVal = 0;

		//MAGIC NUMBER
		//if the number of moves at best val is less than this, we'll include less good moves if there are any
		int minimumAcceptableBestMoves = 5;

		Dictionary<MoveInfo, int> values = new Dictionary<MoveInfo, int> ();

		foreach (MoveInfo move in moves) {
			int thisVal = units[move.unitID].deck.Hand[cardID].checkMoveVal (move, this);
			values.Add (move, thisVal);
			//if this is the new best, mark it and reste our count
			if (thisVal > bestVal) {
				bestVal = thisVal;
				numMovesAtBestVal = 0;
			}
			//if this macthes the best, increase our count
			if (thisVal == bestVal) {
				numMovesAtBestVal++;
			}
		}

		//go through each move and paint the coresponding tile based on the move value for debug purposes
		if (showDebugColors) {
			foreach (MoveInfo move in moves) {
				Tile targetTile = Grid [move.targetTilePos.x, move.targetTilePos.y]; 
				if (values [move] < 0) {
					targetTile.setHighlighted (true, Color.red);
				} else {
					float prc = (float)values [move] / (float)bestVal;
					targetTile.setHighlighted (true, Color.Lerp(Color.blue, Color.green, prc));
				}
			}
		}

		//starting from the highest value, grab enough moves
		int lowestGoodVal = bestVal;
		int numMovesAtThisVal = numMovesAtBestVal;
		//never grab a move with an initial value less than 0
		while (numMovesAtThisVal < minimumAcceptableBestMoves && lowestGoodVal>1) {
			lowestGoodVal--;
			numMovesAtThisVal = 0;
			foreach(MoveInfo move in moves){
				if (values [move] >= lowestGoodVal) {
					numMovesAtThisVal++;
				}
			}
		}

		for (int i = moves.Count - 1; i >= 0; i--) {
			if (values [moves [i]] < lowestGoodVal) {
				moves.RemoveAt (i);
			}
		}


		Profiler.EndSample ();
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

	//we clal this once on the original board in an AI turn so that we don't recalculate the old board state for every single new board state
	public void refreshAllyAndEnemyListsForBoardCompare(bool rootingForAI){
		if (alliesForBoardCompare == null) {
			alliesForBoardCompare = new List<Unit> ();
		}
		if (enemiesForBoardCompare == null) {
			enemiesForBoardCompare = new List<Unit> ();
		}
		alliesForBoardCompare.Clear ();
		enemiesForBoardCompare.Clear ();

		foreach (Unit unit in units) {
			if (unit.isPlayerControlled == rootingForAI) {
				enemiesForBoardCompare.Add (unit);
			} else {
				alliesForBoardCompare.Add (unit);
			}
		}

		//set their lowest cover too
		for (int i = 0; i < alliesForBoardCompare.Count; i++) {
			Tile.Cover oldLowestCover = Tile.Cover.Full;
			foreach (Unit enemy in enemiesForBoardCompare) {
				if (!enemy.isDead) {
					Tile.Cover thisCover = getCover (enemy.CurTile, alliesForBoardCompare [i].CurTile);
					if ((int)thisCover < (int)oldLowestCover) {
						oldLowestCover = thisCover;
						if (thisCover == Tile.Cover.None) {
							break;
						}
					}	
				}
			}
			alliesForBoardCompare [i].oldLowestCoverForAISim = oldLowestCover;
		}
	}

	public void compareBoardSates(Board oldBoard, Unit curUnit, ref TurnInfo info, bool printInfo){
		Profiler.BeginSample("compare boards");

		AIProfile curAIProfile = curUnit.aiProfile;

		//chekc if any charms on the unit would overwrite that profile
		foreach (Charm charm in curUnit.Charms) {
			AIProfile charmProfile = charm.checkReplaceAIProfile ();
			if (charmProfile != null) {
				Debug.Log ("replacing AI profile with the one from " + charm.idName);
				curAIProfile = charmProfile;
			}
		}

		//create unit lists. These lists should line up exactly. it is bad if they don't
		Profiler.BeginSample("making lists");
		List<Unit> curAllies = new List<Unit>();
		List<Unit> oldAllies = oldBoard.alliesForBoardCompare;// new List<Unit>();
		List<Unit> curEnemies = new List<Unit> ();
		List<Unit> oldEnemies = oldBoard.enemiesForBoardCompare;// new List<Unit>();
		Profiler.EndSample ();

		//Debug.Log ("old allies: " + oldAllies.Count);
		bool rootingForAI = !curUnit.isPlayerControlled;

		info.val = 0;

		//check if they are passing their turn
		if (info.moves [0].passMove) {
			info.val = -curAIProfile.hateForPassing;
		}

		//sepeare them into allies on enemies depending on which side we are trying to get a move for
		Profiler.BeginSample("sepearte allies and foes");
		foreach (Unit unit in units) {
			if (unit.isPlayerControlled == rootingForAI) {
				curEnemies.Add (unit);
			} else {
				curAllies.Add (unit);
			}
		}

		//sanity check
		if (oldEnemies.Count != curEnemies.Count || oldAllies.Count > curAllies.Count) {
			Debug.Log ("BAD BAD BAD BAD");
		}
		if (oldAllies.Count < curAllies.Count) {
			int dif = curAllies.Count - oldAllies.Count;
			info.val += curAIProfile.newAlliesWeight * dif;
			if (printInfo) {
			
				Debug.Log (dif + " new allies with weight of "+curAIProfile.newAlliesWeight);
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
		info.val += totalEnemyDamage * curAIProfile.totalEnemyDamageWeight;
		info.val += totalEnemyHeal * curAIProfile.totalEnemyHealWeight;
		info.val += numEnemiesKilled * curAIProfile.numEnemiesKilledWeight;
		info.val += numEnemiesAided * curAIProfile.numEnemiesAidedWeight;
		info.val += deltaEnemyGoodCharms * curAIProfile.deltaEnemyGoodCharmWeight;
		info.val += deltaEnemyBadCharms * curAIProfile.deltaEnemyBadCharmWeight;
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
		info.val += totalAllyDamage * curAIProfile.totalAllyDamageWeight;
		info.val += totalAllyHeal * curAIProfile.totalAllyHealWeight;
		info.val += numAlliesKilled * curAIProfile.numAlliesKilledWeight;
		info.val += numAlliesAided * curAIProfile.numAlliesAidedWeight;
		info.val += numAlliesCursed * curAIProfile.numAlliesCursedWeight;
		info.val += deltaAllyGoodCharms * curAIProfile.deltaAllyGoodCharmWeight;
		info.val += deltaAllyBadCharms * curAIProfile.deltaAllyBadCharmWeight;
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
			Debug.Log ("allies aided " + numAlliesAided+ "  weight " + curAIProfile.numAlliesAidedWeight);
			Debug.Log ("allies cursed " + numAlliesCursed);
			Debug.Log ("ally good charm delta " + deltaAllyGoodCharms);
			Debug.Log ("ally bad charm delta " + deltaAllyBadCharms);
		}

		//checking distance stuff
		Profiler.BeginSample("distance stuff");
		if (curAIProfile.ignoreDistanceChecks == false) {
			for (int i = 0; i < oldAllies.Count; i++) {

				float minDistFromClosest = curAllies [i].aiProfile.preferedDistToClosestEnemy - curAllies [i].aiProfile.acceptableDistanceRangeToClosestEnemy;
				float maxDistFromClosest = curAllies [i].aiProfile.preferedDistToClosestEnemy + curAllies [i].aiProfile.acceptableDistanceRangeToClosestEnemy;

				float oldClosestDistToEnemy = 9999;
				float newClosestDistToEnemy = 9999;

				foreach (Unit enemy in oldEnemies) {
					float dist = dm.getDist (enemy.CurTile, oldAllies [i].CurTile);		// enemy.CurTile.Pos.getDist (oldAllies [i].CurTile.Pos);
					if (dist < oldClosestDistToEnemy)
						oldClosestDistToEnemy = dist;
				}
				foreach (Unit enemy in curEnemies) {
					float dist = dm.getDist (enemy.CurTile, curAllies [i].CurTile);		// enemy.CurTile.Pos.getDist (curAllies [i].CurTile.Pos);
					if (dist < newClosestDistToEnemy)
						newClosestDistToEnemy = dist;
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
				//THIS RELIES ON THE ALLIE'S AI PROFILE, not the current unit's
				info.val += change * curAllies [i].aiProfile.distanceToEnemiesWeight;

				if (printInfo) {
					Debug.Log ("min dist: " + minDistFromClosest);
					Debug.Log ("max dist: " + maxDistFromClosest);
					Debug.Log ("old closest: " + oldClosestDistToEnemy);
					Debug.Log ("new closest: " + newClosestDistToEnemy);
					Debug.Log ("old val: " + oldVal);
					Debug.Log ("new val: " + newVal);
					Debug.Log (curAllies [i].unitName + " dist to enemy change: " + change);
					Debug.Log ("actual value: " + (change * curAllies [i].aiProfile.distanceToEnemiesWeight));
				}
			}
		}
		Profiler.EndSample ();

		//how has ally cover changed?
		Profiler.BeginSample("cover checks");
		for (int i = 0; i < oldAllies.Count; i++) {

			Tile.Cover oldLowestCover = oldAllies [i].oldLowestCoverForAISim;

			Tile.Cover newLowestCover = Tile.Cover.Full;
			foreach (Unit enemy in curEnemies) {
				if (!enemy.isDead) {
					Tile.Cover thisCover = getCover (enemy.CurTile, curAllies [i].CurTile);
					if ((int)thisCover < (int)newLowestCover) {
						newLowestCover = thisCover;
						if (thisCover == Tile.Cover.None) {
							break;
						}
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

			float changeVal = curAIProfile.coverChange [(int)oldLowestCover, (int)newLowestCover];
			info.val += changeVal;

			if (printInfo) {
				Debug.Log ("ally " + i + " " + curAllies [i] + " was " + oldLowestCover + " is " + newLowestCover + " for val "+changeVal);
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
				if (curAIProfile.preferedCardWeights.ContainsKey(cardName)){
					info.val += curAIProfile.preferedCardWeights [cardName];
					//Debug.Log("played "+cardName+" worth "+curAIProfile.preferedCardWeights [cardName]);
				}
			}
		}
		Profiler.EndSample ();

		//do any charms affect things?
		foreach (Charm charm in curUnit.Charms) {
			float charmVal = charm.getAIMoveValue (oldBoard, this, curUnit, ref info, printInfo);
			info.val += charmVal;
			if (charmVal != 0 && printInfo) {
				Debug.Log ("charm " + charm.idName + " val: " + charmVal);
			}
		}


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
