using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using System.IO;

public class GameManagerTacticsInterface : MonoBehaviour {

	public bool publicRelease;

	private CameraControl cam;

	public float debugAnimationTimeMod;
	public bool debugDoNotShuffle;
	public bool debugPrintAIInfo;
	public bool debugRevealFOW;
	public bool debugRemoveFOW;
	public bool debugIgnoreStandardSpawns;
	public bool debugShowTileDist;
	public bool debugClearTileDists;
	public bool debugWakeAllFoes;
	public bool debugAllFoesHaveLoot;
	public bool debugTreatAllFoesAsVisible;

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;
	public GameObject aiTurnText;

	public int levelsPerArea;

	public int lootPerLevel, potentialBonusLootPerLevel;

	public float minTimeOnPlayerTurn;
	private float playerTurnTimer;

	public string debugMapName;

	//public int mapChunkCols, mapChunkRows;
	public TextAsset[] mapChunks;

	public string[] debugSpawnList;

	public GameObject pickupLootButton;

	public GameObject playerButtonsGroup;
	public GameObject cancelButton;

	private bool doingAnimation;

	private int aiTurnPhase;
	private bool autoPlayAITurn;	//for when ai units are not visible

	public float pauseTimeBeforeTabbingExaustedUnit;
	private float tabTimer;
	private bool tabPlayerAfterAnimations;

	[System.NonSerialized]
	public Tile curMouseOverTile;	//for roll over effects
	private Tile prevMouseOverTile;

	private int thisIsATest = -1;

	void Awake () {
		if (instance == null) {
			instance = this;
			gm = new GameManager ();
		} else if (instance != this) {
			Destroy (gameObject);
		}

	}

	void Start(){
		if (publicRelease) {
			debugDoNotShuffle = false;
			debugPrintAIInfo = false;
			debugRevealFOW = false;
			debugRemoveFOW = false;
			debugIgnoreStandardSpawns = false;
			debugShowTileDist = false;
			debugClearTileDists = false;
			debugWakeAllFoes = false;
			debugAllFoesHaveLoot = false;
			debugTreatAllFoesAsVisible = false;
		}

		cam = GameObject.Find ("Main Camera").GetComponent<CameraControl> ();
		gm.targetInfoText = targetInfoText;
		gm.setup (debugSpawnList);
		doingAnimation = false;

		if (debugWakeAllFoes) {
			foreach (Unit foe in gm.getAIUnits()) {
				foe.wakeUp ();
			}
		}

		playerTurnTimer = 0;
	}

	void Update () {

//		Profiler.BeginSample ("HELLO FRIEND");
//		for (int i = 0; i < 1000; i++) {
//			Debug.Log ("CURSE ALL MEN");
//		}
//		Profiler.EndSample ();

		//getting input

		thisIsATest--;
		if (thisIsATest == 0 && Profiler.enabled) {
			UnityEditor.EditorApplication.isPaused = true;
		}

		//tabbing
		if (Input.GetKeyDown (KeyCode.Tab)) {
			gm.tab (1);
		}
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			gm.tab (-1);
		}


		//input for player turn
		if (gm.IsPlayerTurn) {
			playerTurnTimer += Time.deltaTime;
			//clicks
			if (Input.GetMouseButtonDown (0) && !areAnimationsHappening()) {
				gm.checkClick ();
			}

			//ending the turn for a unit
			if (Input.GetKeyDown (KeyCode.Return)) {
				endPlayerTurn ();
				if (!publicRelease && debugPrintAIInfo) {
					thisIsATest = 1;
				}
			}

			//pressing escape to cancel a move
			if (Input.GetKeyDown (KeyCode.Escape)) {
				cancel ();
			}


		}
		//input for AI turn
		else {
			playerTurnTimer = 0; 
			if (Input.GetKeyDown(KeyCode.Space) || autoPlayAITurn) {
				advanceAITurn ();
			}
		}

		//debug input
		if (Input.GetKey(KeyCode.Z)){
			//teleport
			if (Input.GetKeyDown (KeyCode.T)) {
				for (int x = 0; x < gm.board.Cols; x++) {
					for (int y = 0; y < gm.board.Rows; y++) {
						if (gm.board.Grid [x, y].MouseIsOver) {
							gm.activePlayerUnit.moveTo (gm.board.Grid [x, y]);
						}
					}
				}
			}
			//gain action
			if (Input.GetKeyDown (KeyCode.A)) {
				gm.activePlayerUnit.gainActions (1);
			}
			//draw card
			if (Input.GetKeyDown (KeyCode.D)) {
				gm.activePlayerUnit.deck.drawCard ();
			}
			//heal
			if (Input.GetKeyDown (KeyCode.H)) {
				gm.activePlayerUnit.heal (2);
			}
				
		}

		//is the game over?
		if (gm.GameIsOver && !areAnimationsHappening()) {
			StartCoroutine (doEndGame());
		}

		//showing AI turn text
		aiTurnText.SetActive(!gm.IsPlayerTurn);

		//buttons
		pickupLootButton.SetActive (gm.IsPlayerTurn && gm.activePlayerUnit.CanPickupLoot);
		playerButtonsGroup.SetActive (gm.IsPlayerTurn);
		cancelButton.SetActive (gm.activeCard != null);

		//did I just toggle the debug switch to show tile distances?
		if (debugClearTileDists){
			debugClearTileDists = false;
			for (int x = 0; x < GameManagerTacticsInterface.instance.gm.board.cols; x++) {
				for (int y = 0; y < GameManagerTacticsInterface.instance.gm.board.rows; y++) {
					GameManagerTacticsInterface.instance.gm.board.Grid [x, y].debugText = "";
				}
			}
		}

		//roll over effects for tiles
		if (curMouseOverTile != prevMouseOverTile) {
			if (curMouseOverTile != null) {

				//visibility icons for movement
				if (curMouseOverTile.IsHighlighted && gm.activeCard != null && gm.activeCard.showVisibilityIconsWhenHighlighting) {
					gm.board.updateUnitVisibilityIconsFromTile (curMouseOverTile, gm.activePlayerUnit);	
					//THIS WILL BE WRONG IF TE UNIT MOVING IS NOT THE ACTIVE UNIT AND THEY HAVE A DIFFERENT SIGHT RANGE
				} else {
					gm.board.clearVisibilityIcons ();
				}
			}
		}
		prevMouseOverTile = curMouseOverTile;


		//testing
//		if (Input.GetKeyDown (KeyCode.F)) {
//			int unitID = 3;
//			int cardID = 0;
//			List<MoveInfo> testMoves = gm.board.getAllMovesForCard (unitID, cardID);
//			gm.board.filterBadMovesIfApplicable (testMoves, cardID, true);
//
//			foreach (MoveInfo move in testMoves) {
//				gm.board.Grid [move.targetTilePos.x, move.targetTilePos.y].debugText = "X";
//			}
//		}

	}

	//things we need to chekc after various game objetcs have gottena  chance
	void LateUpdate(){
		//check if we should be tabbing the player
		if (tabPlayerAfterAnimations) {
			tabTimer -= Time.deltaTime;
			if (tabTimer <= 0 && areAnimationsHappening () == false) {
				//if there is a unit to go to, do it
				if (!gm.areAllPlayerUnitsExausted ()) {
					gm.tabActivePlayerUnitSkippingExausted (1);
				} else {
					tabPlayerAfterAnimations = false;
				}
			}
		}
	}

	public void triggerTabAfterAnimations(){
		tabPlayerAfterAnimations = true;
		tabTimer = pauseTimeBeforeTabbingExaustedUnit;
	}
	public void cancelTabAfterAnimations(){
		tabPlayerAfterAnimations = false;
	}

	//button listeners
	public void pickUpLoot(){
		gm.activePlayerUnit.pickUpLoot ();
	}

	public void endPlayerTurn(){
		if (playerTurnTimer > minTimeOnPlayerTurn || !GameManagerTacticsInterface.instance.publicRelease) {	//remove the minimum time before ending turn if we're testing debug shit
			gm.endPlayerTurn ();
			autoPlayAITurn = false;
		}
	}

	public void tabPlayer(){
		gm.tab (1);
	}

	public void cancel(){
		gm.cancel ();
	}

	//deailng with AI
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
				TilePos targetPos = gm.activeAIUnit.aiTurnInfo.moves [gm.activeAIUnit.curAITurnStep].targetTilePos;

				//if the unit or the target is visible, demo it
				if (gm.board.Grid [targetPos.x, targetPos.y].isVisibleToPlayer || gm.activeAIUnit.getIsVisibleToPlayer ()) {
					autoPlayAITurn = false;

					thisCard.revealAICardFlag = true; 
					//spawn one or more targets
					TargetGO target = GameObjectManager.instance.getTargetGO ();
					target.activate (targetPos, thisCard.baseHighlightColor);
					//focus camera on the target
					cam.setTarget (targetPos);

					//if the target is a unit and the card is an attack, let's get some info about the hit
					Unit thisUnit = gm.board.getUnitOnTile (targetPos);
					if (thisUnit != null) {
						thisCard.setPotentialTargetInfo (thisUnit);
					}
				}
				//otherwise, just move on
				else {
					autoPlayAITurn = true;
				}

				//testing
//				advanceAITurn();
//				return;

			} else {
				gm.endAITurn ();
				return;
			}
		}

		//play the card
		if (aiTurnPhase == 2) {
			gm.advanceAITurn ();
			aiTurnPhase = 0;
			autoPlayAITurn = !gm.activeAIUnit.getIsVisibleToPlayer ();
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
		UnityEngine.SceneManagement.SceneManager.LoadScene ("endGameReport");
	}

	public bool areAnimationsHappening(){
		if (doingAnimation){
			return true;
		}
		if (GameObjectManager.instance.areAnimationsHappening ()) {
			return true;
		}

		return false;
	}




	//data stuff
	//THIS IS DONE IN MenuTools.cs NOW
	/*
	void resetPlayerData(){
		Debug.Log ("Dad's getting a brand new boy!");
		string playerTargetPath = Application.dataPath + "/external_data/player";
		string playerSourcePath = Application.dataPath + "/external_data/player_safe";
		CopyDir (playerSourcePath, playerTargetPath);
	}

	//this code from https://stackoverflow.com/questions/7146021/copy-all-files-in-directory
	void CopyDir(string sourceDir, string targetDir)
	{
		Directory.CreateDirectory(targetDir);

		foreach(var file in Directory.GetFiles(sourceDir))
			File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

		foreach(var directory in Directory.GetDirectories(sourceDir))
			CopyDir(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
	}
	*/
}
