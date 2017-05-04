using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTacticsInterface : MonoBehaviour {


	private CameraControl cam;

	public float debugAnimationTimeMod;
	public bool debugDoNotShuffle;
	public bool debugPrintAIInfo;
	public bool debugRevealFOW;
	public bool debugIgnoreStandardSpawns;
	public bool debugShowTileDist;

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;
	public GameObject aiTurnText;

	public float lootDropPrc;

	public string debugMapName;

	public int mapChunkCols, mapChunkRows;
	public TextAsset[] mapChunks;

	public string[] debugSpawnList;

	public GameObject pickupLootButton;

	public GameObject playerButtonsGroup;
	public GameObject cancelButton;

	private bool doingAnimation;

	private int aiTurnPhase;
	private bool autoPlayAITurn;	//for when ai units are not visible

	void Awake () {
		if (instance == null) {
			instance = this;
			gm = new GameManager ();
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start(){
		cam = GameObject.Find ("Main Camera").GetComponent<CameraControl> ();
		gm.targetInfoText = targetInfoText;
		gm.setup (debugSpawnList);
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
				endPlayerTurn ();
			}

			//pressing escape to cancel a move
			if (Input.GetKeyDown (KeyCode.Escape)) {
				cancel ();
			}


		}
		//input for AI turn
		else {
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
			

		//is the game over
		if (gm.GameIsOver && !areAnimationsHappening()) {
			StartCoroutine (doEndGame());
		}

		//showing AI turn text
		aiTurnText.SetActive(!gm.IsPlayerTurn);

		//buttons
		pickupLootButton.SetActive (gm.IsPlayerTurn && gm.activePlayerUnit.CanPickupLoot);
		playerButtonsGroup.SetActive (gm.IsPlayerTurn);
		cancelButton.SetActive (gm.activeCard != null);

		//testing
//		for (int i = 0; i < 4; i++) {
//			if (gm.board.units [i] != gm.activePlayerUnit) {
//				gm.board.getCover (gm.activePlayerUnit, gm.board.units [i]);
//			}
//		}
//
//		for (int x = 0; x <= gm.board.cols; x++) {
//			Debug.DrawLine (new Vector3 (x, 0, 1), new Vector3 (x, gm.board.rows, 1), Color.red);
//		}
//		for (int y = 0; y <= gm.board.rows; y++) {
//			Debug.DrawLine (new Vector3 (0, y, 1), new Vector3 (gm.board.cols, y, 1), Color.red);
//		}

	}

	//button listeners
	public void pickUpLoot(){
		gm.activePlayerUnit.pickUpLoot ();
	}

	public void endPlayerTurn(){
		gm.endPlayerTurn ();
		autoPlayAITurn = false;
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
					autoPlayAITurn = true;;
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
