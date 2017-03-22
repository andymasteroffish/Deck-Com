using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTacticsInterface : MonoBehaviour {

	public float debugAnimationTimeMod;
	public bool debugDoNotShuffle;

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;

	public string[] spawnList;

	public GameObject pickupLootButton;

	private bool doingAnimation;


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

		//clicks
		if (Input.GetMouseButtonDown (0) && !areAnimationsHappening()) {
			gm.checkClick ();
		}

		//tabbing
		if (Input.GetKeyDown (KeyCode.Tab) && !areAnimationsHappening()) {
			gm.tab (1);
		}
		if (Input.GetKeyDown (KeyCode.LeftShift) && !areAnimationsHappening()) {
			gm.tab (-1);
		}

		//ending the turn for a unit
		if (Input.GetKeyDown (KeyCode.Space)) {
			if (gm.activeUnit.isPlayerControlled) {
				gm.endUnitTurn ();
			} else {
				gm.advanceAITurn ();
			}
		}

		//pressing escape to cancel a move
		if (Input.GetKeyDown (KeyCode.Escape)) {
			gm.cancel ();
		}

		//showing the loot button if applicable
		pickupLootButton.SetActive( gm.activeUnit.CanPickupLoot);

		//is the game over
		if (gm.GameIsOver && !areAnimationsHappening()) {
			StartCoroutine (doEndGame());
		}

		//testing
		if (Input.GetKeyDown (KeyCode.T)) {
			float startTime = Time.realtimeSinceStartup;
			Board.debugCounter = 0;
			TurnInfo turn = gm.getAIMove (0, gm.board, gm.board, 0);
			turn.print (gm.board);
//			for (int i=0; i<gm.board.units[0].deck.Hand.Count; i++){
//				List<MoveInfo> moves = gm.board.getAllMovesForCard (0, i);
//				foreach (MoveInfo move in moves) {
//					Board newBoard = gm.board.resolveMoveAndReturnResultingBoard (move);
//				}
//			}
			Debug.Log ("it took " + (Time.realtimeSinceStartup - startTime)+ " seconds and "+Board.debugCounter+" boards");
		}
		if (Input.GetKeyDown (KeyCode.Y)) {
			Debug.Log ("GO mmove!");
//
//			MoveInfo move = new MoveInfo ();
//			move.unitID = 0;
//			move.cardIDName = gm.board.units[;
//			move.targetTilePos = new TilePos (3, 3);
//			Board board1 = gm.board.resolveMoveAndReturnResultingBoard (move);
//			board1.print();
//
//			MoveInfo move2 = new MoveInfo ();
//			move2.unitID = 0;
//			move2.cardIDName = 2;
//			move2.targetTilePos = new TilePos (2, 4);
//			Board board2 = board1.resolveMoveAndReturnResultingBoard (move2);
//			board2.print();
		}
		if (Input.GetKeyDown (KeyCode.P)) {
			gm.board.print ();
		}

	}

	public void pickUpLoot(){
		gm.activeUnit.pickUpLoot ();
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
