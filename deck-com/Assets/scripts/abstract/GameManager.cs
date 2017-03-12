using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager {
	
	//public CardManager cardManager;
	//public GameObjectManager gameObjectManager;

	private CameraControl cam;
	public Board board;

	//public string[] spawnList;
	//public GameObject[] spawnList;

	public List<Unit> units = new List<Unit>();

	public Unit activeUnit;

	public Card activeCard;

	public TargetInfoText targetInfoText;

	//tracking rounds
	private bool isPlayerTurn;
	private int roundNum;

	public GameManager(){
	}

	public void setup (string[] spawnList) {

		board = new Board ();
		board.reset ();

		roundNum = 0;

		cam = GameObject.Find ("Main Camera").GetComponent<CameraControl> ();

		//place the units
		for (int i = 0; i < spawnList.Length; i++) {
			Unit unit = UnitManager.instance.getUnitFromIdName (spawnList [i]);
			Tile spawnTile = board.GetUnoccupiedTileWithSpawnProperty( unit.isPlayerControlled ? Tile.SpawnProperty.Player : Tile.SpawnProperty.Foe);
			unit.setup (this, spawnTile);
			units.Add (unit);
		}

		foreach(Unit unit in units){
			unit.reset ();
		}

		//add some loot to some of them TESTING
		List<Unit> potentialHolders = new List<Unit>();
		for (int i = 0; i < units.Count; i++) {
			if (units [i].isPlayerControlled == false) {
				potentialHolders.Add (units [i]);
			}
		}
		for (int i = 0; i < 4; i++) {
			Unit holder = potentialHolders [(int)Random.Range (0, potentialHolders.Count)];
			potentialHolders.Remove (holder);
			Loot loot = new Loot (holder, Loot.Type.money, 1);
			board.LootList.Add (loot);
		}

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

		List<Unit> unitsAI = getAIUnits ();
		if (unitsAI.Count == 0) {
			Debug.Log ("YOU WIN");
		} else {
			//start the AI
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
		for (int i = units.Count - 1; i >= 0; i--) {
			units[i].checkGeneralClick ();
		}
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
		foreach (Unit unit in units){
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

	//killing units
	public void removeUnit(Unit deadOne){
		units.Remove (deadOne);
	}

	//unit tools
	public List<Unit> getAIUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in units){
			if (!unit.isPlayerControlled) {
				aiUnits.Add (unit);
			}
		}
		return aiUnits;
	}
	public List<Unit> getPlayerUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in units){
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

//	public Card ActiveCard{
//		get{
//			return this.activeCard;
//		}
//	}

//	public List<Unit> Units{
//		get{
//			return this.units;
//		}
//	}
}
