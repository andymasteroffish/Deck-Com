using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public CameraControl cam;
	public Board board;

	public List<Unit> units;

	private Unit activeUnit;

	public Card activeCard;

	//tracking rounds
	private bool isPlayerTurn;
	private int roundNum;

	// Use this for initialization
	void Start () {
		
		board.reset ();

		roundNum = 0;

		units [0].setup (this, 2, 2);
		board.Grid [2, 2].setCover (Tile.Cover.None);

		units [1].setup (this, 4, 2);
		board.Grid [4, 2].setCover (Tile.Cover.None);

		units [2].setup (this, 3, 3);
		board.Grid [3, 3].setCover (Tile.Cover.None);


		foreach(Unit unit in units){
			unit.reset ();
		}

		startRound ();

	}

	//starting and ending turns

	void startRound(){
		roundNum++;

		isPlayerTurn = true;

		List<Unit> unitsPlayer = getPlayerUnits ();
		foreach(Unit unit in unitsPlayer){
			unit.resetRound ();
		}
		setActiveUnit (unitsPlayer [0]);

		clearActiveCard ();
	}

	void endPlayerTurn(){
		isPlayerTurn = false;

		//end the players
		List<Unit> unitsPlayer = getPlayerUnits ();
		foreach(Unit unit in unitsPlayer){
			unit.endTurn ();
		}

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
	}

	void endAITurn(){
		//end AI
		List<Unit> unitsAI = getAIUnits ();
		foreach(Unit unit in unitsAI){
			unit.endTurn ();
		}

		startRound ();
	}

	//Update
	void Update () {

		//getting input

		//clicks
		if (Input.GetMouseButtonDown (0) && !areAnimaitonsHappening()) {
			//see if they clicked a card to play (but only if they are not in the process of playing one)
			if (activeCard == null) {
				activeUnit.checkActiveClick ();
			}
			//check potential targets
			board.checkClick ();
			foreach(Unit unit in units){
				unit.checkGeneralClick ();
			}
		}

		//tabbing
		if (Input.GetKeyDown (KeyCode.Tab)) {
			clearActiveCard ();
			if (isPlayerTurn) {
				tabActivePlayerUnit ();
			} else {
				tabActiveAIUnit ();
			}
		}

		//ending the turn
		if (Input.GetKeyDown (KeyCode.Space)) {
			clearActiveCard ();
			if (isPlayerTurn) {
				endPlayerTurn ();
			} else {
				endAITurn ();
			}
		}

		//pressing escape to cancel a move
		if (Input.GetKeyDown (KeyCode.Escape)) {
			activeUnit.deck.cancel ();
		}
	}

	//Input for cards that are about to be played
	public void tileClicked(Tile tile){
		Debug.Log ("clickci");
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
		activeUnit = newActive;
		foreach (Unit unit in units){
			unit.setActive ( unit==activeUnit);
			if (!unit.IsActive) {
				unit.deck.cancel ();
			}
		}
		cam.setTarget (newActive);
	}

	void tabActivePlayerUnit(){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activeUnit) {
				idNum = i;
			}
		}
		int newID = idNum + 1;
		if (newID >= unitsPlayer.Count) {
			newID = 0;
		}
		setActiveUnit (unitsPlayer [newID]);
	}

	void tabActiveAIUnit(){
		List<Unit> unitsAI = getAIUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsAI.Count; i++) {
			if (unitsAI [i] == activeUnit) {
				idNum = i;
			}
		}
		int newID = idNum + 1;
		if (newID >= unitsAI.Count) {
			newID = 0;
		}
		setActiveUnit (unitsAI [newID]);
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
	public bool areAnimaitonsHappening(){
		foreach(Unit unit in units){
			if (unit.DoingAnimation){
				return true;
			}
		}

		return false;
	}

	//setters and getters

//	public List<Unit> Units{
//		get{
//			return this.allUnits;
//		}
//	}
}
