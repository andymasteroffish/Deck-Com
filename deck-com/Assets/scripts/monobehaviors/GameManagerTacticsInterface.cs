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

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;
	public GameObject aiTurnText;

	public float lootDropPrc;

	public string debugMapName;

	public int mapChunkCols, mapChunkRows;
	public TextAsset[] mapChunks;

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
		cam = GameObject.Find ("Main Camera").GetComponent<CameraControl> ();
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

		//testing
		if (Input.GetKeyDown (KeyCode.T)) {
			gm.activePlayerUnit.deck.discardACardAtRandom ();
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
				TilePos targetPos = gm.activeAIUnit.aiTurnInfo.moves [gm.activeAIUnit.curAITurnStep].targetTilePos;
				thisCard.revealAICardFlag = true; 
				//spawn one or more targets
				TargetGO target = GameObjectManager.instance.getTargetGO ();
				target.activate (targetPos, thisCard.baseHighlightColor);
				//focus camera on the target
				cam.setTarget(targetPos);

				//if the target is a unit and the card is an attack, let's get some info about the hit
				Unit thisUnit = gm.board.getUnitOnTile(targetPos);
				if (thisUnit != null) {
					thisCard.setPotentialTargetInfo (thisUnit);
				}

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
