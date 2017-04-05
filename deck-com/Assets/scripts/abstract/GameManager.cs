using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager {
	
	private CameraControl cam;	//THIS SHOULD NOT BE HERE. MOVE THIS TO AN INTERFACE CLASS
	public Board board;

	public Unit activePlayerUnit;
	public Unit activeAIUnit;

	public Card activeCard;

	public TargetInfoText targetInfoText;

	//tracking rounds
	private bool isPlayerTurn;
	private int roundNum;

	//finishing
	private bool gameIsOver;
	private bool playerWins;

	public GameManager(){
	}

	public void setup (string[] spawnList) {

		board = new Board ();
		board.reset ();

		roundNum = 0;
		gameIsOver = false;
		playerWins = false;
			
		cam = GameObject.Find ("Main Camera").GetComponent<CameraControl> ();

		//place the units
		for (int i = 0; i < spawnList.Length; i++) {
			Unit unit = UnitManager.instance.getUnitFromIdName (spawnList [i]);
			Tile spawnTile = board.GetUnoccupiedTileWithSpawnProperty( unit.isPlayerControlled ? Tile.SpawnProperty.Player : Tile.SpawnProperty.Foe);
			unit.setup (this, board, spawnTile);
			board.units.Add (unit);
		}

		board.resetUnitsAndLoot ();

		startPlayerTurn ();

	}

	//starting and ending turns

	void startPlayerTurn(){
		roundNum++;

		isPlayerTurn = true;

		List<Unit> unitsPlayer = getPlayerUnits ();
		foreach(Unit unit in unitsPlayer){
			unit.resetRound ();
		}
		setActivePlayerUnit (unitsPlayer [0]);

		if (activeAIUnit != null) {
			activeAIUnit.setActive (false);
			activeAIUnit = null;
		}

		clearActiveCard ();
	}

	void startAITurn(){
		isPlayerTurn = false;

		if (checkGameOver()) {
			endGame ();
		} else {
			//start the AI
			List<Unit> unitsAI = getAIUnits ();
			foreach (Unit unit in unitsAI) {
				unit.resetRound ();
			}
			setActiveAIUnit (unitsAI [(int)Random.Range(0,unitsAI.Count)]);
		}

		clearActiveCard ();
	}

	public void resetAllAIFlags(){
		Debug.Log ("reset all AI flags");
		foreach (Unit unit in board.units) {
			unit.resetAISimFlags ();
		}
	}

	//checking input
	public void checkClick(){
		//see if they clicked a card to play (but only if they are not in the process of playing one)
		if (activeCard == null) {
			activePlayerUnit.checkActiveClick ();
		}
		//check potential targets
		board.checkClick ();

	}

	public void endPlayerTurn(){
		clearActiveCard ();
		List<Unit> unitsPlayer = getPlayerUnits ();
		for(int i=unitsPlayer.Count-1; i>=0; i--){
			unitsPlayer[i].endTurn ();
		}
		//check if anything (including foes) has clean up effects
		for (int i = board.units.Count-1; i >= 0; i--) {
			board.units [i].turnEndCleanUp ();
		}
		startAITurn ();
	}
//	public void endUnitTurn(){
//		clearActiveCard ();
//		for (int i = board.units.Count-1; i >= 0; i--) {
//			board.units [i].turnEndCleanUp ();
//		}
//		activePlayerUnit.endTurn ();
//	}

	public void advanceAITurn(){
		board.resolveMove (activeAIUnit.aiTurnInfo.moves [activeAIUnit.curAITurnStep]);
		activeAIUnit.curAITurnStep++;
	}
	public void endAITurn(){
		activeAIUnit.endTurn ();
		//check if anything has clean up effects
		for (int i = board.units.Count-1; i >= 0; i--) {
			board.units [i].turnEndCleanUp ();
		}
		//go to the next one
		tabActiveAIUnit(1);
	}

	public void cancel(){
		activePlayerUnit.deck.cancel ();
	}

	//Input for cards that are about to be played
	public void tileClicked(Tile tile){
		if (activeCard != null) {
			activeCard.passInTile (tile);
		}
	}
	public void unitClicked(Unit unit){
		if (activeCard != null) {
			activeCard.passInUnit (unit);
		}
	}

	public void setCardActive(Card newCard){
		activeCard = newCard;
	}

	public void clearActiveCard(){
		if (activeCard != null){
			activeCard = null;
			board.clearHighlights ();
		}
	}

	//switching units

	void setActivePlayerUnit(Unit newActive){
//		if (newActive == activePlayerUnit) {
//			return;
//		}
		activePlayerUnit = newActive;
		List<Unit> playerUnits = getPlayerUnits ();
		foreach (Unit unit in playerUnits){
			unit.setActive ( unit==activePlayerUnit);
			if (!unit.IsActive) {
				unit.deck.cancel ();
			}
		}
		cam.setTarget (newActive);
	}

	void setActiveAIUnit(Unit newActive){
		if (newActive == activeAIUnit) {
			return;
		}
		activeAIUnit = newActive;
		List<Unit> playerUnits = getAIUnits ();
		foreach (Unit unit in playerUnits){
			unit.setActive ( unit==activeAIUnit);
		}
		cam.setTarget (newActive);
		GameManagerTacticsInterface.instance.startNewAIUnitTurn ();
	}

	public void tab(int dir){
		clearActiveCard ();
		tabActivePlayerUnit (dir);
	}

	public void tabActivePlayerUnitSkippingExausted(int dir){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activePlayerUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum + dir + unitsPlayer.Count) % unitsPlayer.Count;
		while (unitsPlayer [idNum].ActionsLeft == 0 && count < unitsPlayer.Count + 1) {
			idNum = (idNum + 1) % unitsPlayer.Count;
			count++;
		}

		setActivePlayerUnit (unitsPlayer [idNum]);
	}

	public void tabActivePlayerUnit(int dir){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activePlayerUnit) {
				idNum = i;
			}
		}
		int newIdNum = (idNum + dir + unitsPlayer.Count) % unitsPlayer.Count;
		setActivePlayerUnit (unitsPlayer [newIdNum]);
	}

	public void tabActiveAIUnit(int dir){
		List<Unit> unitsAI = getAIUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsAI.Count; i++) {
			if (unitsAI [i] == activeAIUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum  + dir + unitsAI.Count) % unitsAI.Count;
		while (unitsAI [idNum].isExausted && count < unitsAI.Count + 1) {
			idNum = (idNum + 1) % unitsAI.Count;
			count++;
		}

		//if we found one, great, otherwise, go to the next turn
		if (count <= unitsAI.Count) {
			setActiveAIUnit (unitsAI [idNum]);
		} else {
			startPlayerTurn ();
		}
	}

	public bool checkGameOver(){
		List<Unit> unitsAI = getAIUnits ();
		if (unitsAI.Count == 0) {
			return true;
		}
		return false;
	}

	public void endGame(){
		gameIsOver = true;

		List<Unit> unitsAI = getAIUnits ();
		playerWins = unitsAI.Count == 0;

		Debug.Log ("pls save everything thanks");
		List <Unit> playerUnits = getPlayerUnits ();

		List<Card_Loot> loot = new List<Card_Loot> ();
		for (int i = 0; i < playerUnits.Count; i++) {
			playerUnits [i].endGame ();
			List<Card_Loot> unitLoot = playerUnits [i].syphonLoot ();
			foreach (Card_Loot lootCard in unitLoot) {
				loot.Add (lootCard);
			}
		}

		//save the decks
		for (int i = 0; i < playerUnits.Count; i++) {
			playerUnits [i].saveDeckFile ();
		}

		//save whatever info needs to be passed to the next scene
		EndGameInfoHolder.instance.lootList = loot;
	}

	//AI shit
	public TurnInfo getAIMove(int unitID, Board curBoard, Board originalBoard, int curDepth){
		float startTime = Time.realtimeSinceStartup;
		if (GameManagerTacticsInterface.instance.debugPrintAIInfo && curDepth == 0) {
			Board.debugCounter = 0;	
		}

		//Debug.Log ("get move at depth " + curDepth);
		List<MoveInfo> allMoves = curBoard.getAllMovesForUnit (unitID);

		if (allMoves.Count == 0) {
			//Debug.Log ("null at depth " + curDepth);
			return null;
		}

		//find all possible moves that could follow
		List<TurnInfo> potentialTurns = new List<TurnInfo>();

		foreach (MoveInfo move in allMoves) {
			TurnInfo turn = new TurnInfo (move);
			//generate a board with this move
			Board newBoard = curBoard.resolveMoveAndReturnResultingBoard (move);
			if (GameManagerTacticsInterface.instance.debugPrintAIInfo) {
				turn.debugResultingBoard = newBoard;
			}

			//find all of the moves the AI could make from there
			TurnInfo followingMoves = getAIMove (unitID, newBoard, originalBoard, curDepth+1);

			//if there were move moves, add them to the turn
			if (followingMoves != null) {
				turn.addMoves (followingMoves);
			} else {
				//if there were no further moves, this is the end of this set and we should evaluate the board
				newBoard.compareBoardSates (originalBoard, curBoard.units [unitID], ref turn, false);
			}

			potentialTurns.Add (turn);
		}

		//find the best one
		//int bestID = 0;
		float bestValue = -9999;
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].val > bestValue) {
				bestValue = potentialTurns [i].val;
			}
		}
		//get a list of all moves with that value
		List<TurnInfo> goodTurns = new List<TurnInfo>();
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].val == bestValue) {
				goodTurns.Add(potentialTurns [i]);
			}
		}

		//get the turn to return
		TurnInfo returnVal = goodTurns[ (int)Random.Range(0,goodTurns.Count) ];

		//print info if we should
		if (GameManagerTacticsInterface.instance.debugPrintAIInfo && curDepth == 0) {
			returnVal.print (board);
			Debug.Log ("it took " + (Time.realtimeSinceStartup - startTime) + " seconds and " + Board.debugCounter + " boards to generate move");
		
			//in order to see what the hell the board evaluation is doing, we'll do one more but have it print info as it goes
			Debug.Log ("------TEST-------");
			TurnInfo temp = new TurnInfo (new MoveInfo(unitID));
			returnVal.debugResultingBoard.compareBoardSates (originalBoard, curBoard.units [unitID], ref temp, true);
			//temp.print (board);
		}

		return returnVal;
	}


	//unit tools
	public List<Unit> getAIUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in board.units){
			if (!unit.isPlayerControlled) {
				aiUnits.Add (unit);
			}
		}
		return aiUnits;
	}
	public List<Unit> getPlayerUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in board.units){
			if (unit.isPlayerControlled) {
				aiUnits.Add (unit);
			}
		}
		return aiUnits;
	}

	//checking for things
	public bool areAnimationsHappening(){
//		foreach(Unit unit in units){
//			if (unit.areAnimationsHappening()){
//				return true;
//			}
//		}

		return false;
	}

	//setters and getters

	public bool IsPlayerTurn {
		get {
			return this.isPlayerTurn;
		}
	}

	public bool GameIsOver{
		get{
			return this.gameIsOver;
		}
	}

	public bool PlayerWinds{
		get{
			return this.playerWins;
		}
	}
}
