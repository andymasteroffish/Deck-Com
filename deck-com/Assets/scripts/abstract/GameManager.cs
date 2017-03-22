using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager {
	
	private CameraControl cam;
	public Board board;

	public Unit activeUnit;

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
		setActiveUnit (unitsPlayer [0]);

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
			setActiveUnit (unitsAI [0]);
		}

		clearActiveCard ();
	}

	//checking input
	public void checkClick(){
		//see if they clicked a card to play (but only if they are not in the process of playing one)
		if (activeCard == null) {
			activeUnit.checkActiveClick ();
		}
		//check potential targets
		board.checkClick ();

	}

	public void endUnitTurn(){
		clearActiveCard ();
		for (int i = board.units.Count-1; i >= 0; i--) {
			board.units [i].turnEndCleanUp ();
		}
		activeUnit.endTurn ();
	}

	public void advanceAITurn(){
		if (activeUnit.curAITurnStep >= activeUnit.aiTurnInfo.moves.Count) {
			activeUnit.endTurn ();
		} else {
			board.resolveMove (activeUnit.aiTurnInfo.moves [activeUnit.curAITurnStep]);
			activeUnit.curAITurnStep++;
		}
	}

	public void cancel(){
		activeUnit.deck.cancel ();
	}

	//Input for cards that are about to be played
	public void tileClicked(Tile tile){
		if (activeCard != null) {
			activeCard.passInTile (tile);
		}
		//activeUnit.deck.passInTile (tile);
	}
	public void unitClicked(Unit unit){
		if (activeCard != null) {
			activeCard.passInUnit (unit);
		}
		//activeUnit.deck.passInUnit (unit);
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

	void setActiveUnit(Unit newActive){
		if (newActive == activeUnit) {
			return;
		}
		activeUnit = newActive;
		foreach (Unit unit in board.units){
			unit.setActive ( unit==activeUnit);
			if (!unit.IsActive) {
				unit.deck.cancel ();
			}
		}
		cam.setTarget (newActive);
	}

	public void tab(int dir){
		clearActiveCard ();
		if (isPlayerTurn) {
			tabActivePlayerUnit (dir);
		} else {
			tabActiveAIUnit (dir);
		}
	}

	public void tabActivePlayerUnit(int dir){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activeUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum + dir + unitsPlayer.Count) % unitsPlayer.Count;
		while (unitsPlayer [idNum].TurnIsDone && count < unitsPlayer.Count + 1) {
			idNum = (idNum + 1) % unitsPlayer.Count;
			count++;
		}

		//if we found one, great, otherwise, go to the next turn
		if (count <= unitsPlayer.Count) {
			setActiveUnit (unitsPlayer [idNum]);
		} else {
			startAITurn ();
		}
	}

	public void tabActiveAIUnit(int dir){
		List<Unit> unitsAI = getAIUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsAI.Count; i++) {
			if (unitsAI [i] == activeUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum  + dir + unitsAI.Count) % unitsAI.Count;
		while (unitsAI [idNum].TurnIsDone && count < unitsAI.Count + 1) {
			idNum = (idNum + 1) % unitsAI.Count;
			count++;
		}

		//if we found one, great, otherwise, go to the next turn
		if (count <= unitsAI.Count) {
			setActiveUnit (unitsAI [idNum]);
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
		//Debug.Log ("get move at depth " + curDepth);
		List<MoveInfo> allMoves = curBoard.getAllMovesForUnit (unitID);

		if (allMoves.Count == 0) {
			//Debug.Log ("null at depth " + curDepth);
			return null;
		}

//		//take a look at how this move will look
//		foreach(MoveInfo move in allMoves){
//			Board newBoard = board.resolveMoveAndReturnResultingBoard (move);
//			newBoard.compareBoardSates (board, ref move);
//		}

		//sort it to get the top X moves

		//actually, let's just try it where we do all possible moves and let them all play out
		List<TurnInfo> potentialTurns = new List<TurnInfo>();

		foreach (MoveInfo move in allMoves) {
			TurnInfo turn = new TurnInfo (move);

			Board newBoard = curBoard.resolveMoveAndReturnResultingBoard (move);
			//evaluate
			newBoard.compareBoardSates(originalBoard, !curBoard.units[unitID].isPlayerControlled, ref turn);

			TurnInfo followingMoves = getAIMove (unitID, newBoard, originalBoard, curDepth+1);

			if (followingMoves != null) {
				turn.addMoves (followingMoves);
			}

			potentialTurns.Add (turn);
		}

		//find the best one
		//int bestID = 0;
		int bestValue = -1000;
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].totalValue > bestValue) {
				bestValue = potentialTurns [i].totalValue;
			}
		}
		//get a list of all moves with that value
		List<TurnInfo> goodTurns = new List<TurnInfo>();
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].totalValue == bestValue) {
				goodTurns.Add(potentialTurns [i]);
			}
		}
		//return one of them
		return goodTurns[ (int)Random.Range(0,goodTurns.Count) ];
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
