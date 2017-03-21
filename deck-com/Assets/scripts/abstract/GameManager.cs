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
		activeUnit.endTurn ();
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
