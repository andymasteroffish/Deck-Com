using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	public float debugAnimationTimeMod;

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

//	void endPlayerTurn(){
//		isPlayerTurn = false;
//
//		//end the players
//		List<Unit> unitsPlayer = getPlayerUnits ();
//		foreach(Unit unit in unitsPlayer){
//			unit.endTurn ();
//		}
//
//		List<Unit> unitsAI = getAIUnits ();
//		if (unitsAI.Count == 0) {
//			Debug.Log ("YOU WIN");
//		} else {
//			//start the AI
//			foreach (Unit unit in unitsAI) {
//				unit.resetRound ();
//			}
//			setActiveUnit (unitsAI [0]);
//		}
//	}
//
//	void endAITurn(){
//		//end AI
//		List<Unit> unitsAI = getAIUnits ();
//		foreach(Unit unit in unitsAI){
//			unit.endTurn ();
//		}
//
//		startRound ();
//	}

	//Update
	void Update () {

		//getting input

		//clicks
		if (Input.GetMouseButtonDown (0) && !areAnimationsHappening()) {
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
		if (Input.GetKeyDown (KeyCode.Tab) && !areAnimationsHappening()) {
			clearActiveCard ();
			if (isPlayerTurn) {
				tabActivePlayerUnit ();
			} else {
				tabActiveAIUnit ();
			}
		}

		//ending the turn for a unit
		if (Input.GetKeyDown (KeyCode.Space)) {
			clearActiveCard ();
			activeUnit.endTurn ();
//			if (isPlayerTurn) {
//				endPlayerTurn ();
//			} else {
//				endAITurn ();
//			}
		}

		//pressing escape to cancel a move
		if (Input.GetKeyDown (KeyCode.Escape)) {
			activeUnit.deck.cancel ();
		}


		//testing
		//board.getCover(units[0], units[2]);

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
		activeUnit = newActive;
		foreach (Unit unit in units){
			unit.setActive ( unit==activeUnit);
			if (!unit.IsActive) {
				unit.deck.cancel ();
			}
		}
		cam.setTarget (newActive);
	}

	public void tabActivePlayerUnit(){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activeUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum + 1) % unitsPlayer.Count;
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

	public void tabActiveAIUnit(){
		List<Unit> unitsAI = getAIUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsAI.Count; i++) {
			if (unitsAI [i] == activeUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum + 1) % unitsAI.Count;
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
		foreach(Unit unit in units){
			if (unit.areAnimationsHappening()){
				return true;
			}
		}

		return false;
	}

	//setters and getters

	public bool IsPlayerTurn {
		get {
			return this.isPlayerTurn;
		}
	}
}
