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
			gm.endUnitTurn ();
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
			Debug.Log ("GO attack!");
			MoveInfo move = new MoveInfo ();
			Unit actingUnit = gm.getPlayerUnits () [0];
			move.card = actingUnit.deck.Hand [0];
			move.targetTilePos = new TilePos (1, 3);
			gm.board.resolveMove (move);
		}
		if (Input.GetKeyDown (KeyCode.Y)) {
			Debug.Log ("GO mmove!");
			MoveInfo move = new MoveInfo ();
			Unit actingUnit = gm.getPlayerUnits () [0];
			move.card = actingUnit.deck.Hand [1];
			move.targetTilePos = new TilePos (3, 3);
			gm.board.resolveMove (move);
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
