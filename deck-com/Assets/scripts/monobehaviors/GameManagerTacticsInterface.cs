﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTacticsInterface : MonoBehaviour {

	public float debugAnimationTimeMod;
	public bool debugDoNotShuffle;

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;

	public string mapName;
	public string[] spawnList;

	public GameObject pickupLootButton;

	private bool doingAnimation;

	private int aiTurnPhase;

	void Awake () {
		if (instance == null) {
			instance = this;
			gm = new GameManager ();
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start(){
		gm.targetInfoText = targetInfoText;
		gm.setup (spawnList);
		doingAnimation = false;
	}

	void Update () {

		//getting input

		//tabbing
		if (Input.GetKeyDown (KeyCode.Tab)) {
			gm.tab (1);
		}
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			gm.tab (-1);
		}


		//input for player turn
		if (gm.IsPlayerTurn) {
			//clicks
			if (Input.GetMouseButtonDown (0) && !areAnimationsHappening()) {
				gm.checkClick ();
			}

			//ending the turn for a unit
			if (Input.GetKeyDown (KeyCode.Space)) {
				gm.endPlayerTurn ();
			}

			//pressing escape to cancel a move
			if (Input.GetKeyDown (KeyCode.Escape)) {
				gm.cancel ();
			}

			//showing the loot button if applicable
			pickupLootButton.SetActive (gm.activePlayerUnit.CanPickupLoot);

		}
		//input for AI turn
		else {
			if (Input.GetKeyDown(KeyCode.Space)) {
				advanceAITurn ();
			}
		}


		//is the game over
		if (gm.GameIsOver && !areAnimationsHappening()) {
			StartCoroutine (doEndGame());
		}


	}

	public void pickUpLoot(){
		gm.activePlayerUnit.pickUpLoot ();
	}

	public void startNewAIUnitTurn(){
		aiTurnPhase = 0;
	}
	private void advanceAITurn(){
		aiTurnPhase++;

		//are we done?
		if (aiTurnPhase == 1 && gm.activeAIUnit.curAITurnStep >= gm.activeAIUnit.aiTurnInfo.moves.Count) {
			gm.endAITurn ();
			return;
		}

		//otherwise reveal the card and mark the target
		if (aiTurnPhase == 1) {
			//turn on the reveal flag
			if (gm.activeAIUnit.aiTurnInfo.moves [gm.activeAIUnit.curAITurnStep].passMove == false) {
				string cardIDName = gm.activeAIUnit.aiTurnInfo.moves [gm.activeAIUnit.curAITurnStep].cardIDName;
				Card thisCard = gm.activeAIUnit.deck.getCardInHandFromID (cardIDName);
				thisCard.revealAICardFlag = true; 
				//spawn one or more targets
				GameObjectManager.instance.getTargetGO ().activate (gm.activeAIUnit.aiTurnInfo.moves [gm.activeAIUnit.curAITurnStep].targetTilePos, thisCard.baseHighlightColor);
			} else {
				gm.endAITurn ();
				return;
			}
		}

		//play the card
		if (aiTurnPhase == 2) {
			gm.advanceAITurn ();
			aiTurnPhase = 0;
			//remove targets
			GameObjectManager.instance.turnOffAllTargets();
		}
	}

	IEnumerator doEndGame(){
		float timer = 0;
		doingAnimation = true;

		float pauseTime = 1 * debugAnimationTimeMod;
		while (timer < pauseTime){
			timer += Time.deltaTime;
			yield return null;
		}

		Debug.Log ("load pls");
		doingAnimation = false;
		UnityEngine.SceneManagement.SceneManager.LoadScene ("endGameResport");
	}

	//FILL THIS IN!!!!!
	public bool areAnimationsHappening(){
		if (doingAnimation){
			return true;
		}
		//		foreach(Unit unit in units){
		//			if (unit.areAnimationsHappening()){
		//				return true;
		//			}
		//		}

		return false;
	}
}
