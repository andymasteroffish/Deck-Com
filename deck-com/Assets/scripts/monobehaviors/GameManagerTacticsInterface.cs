using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerTacticsInterface : MonoBehaviour {

	public float debugAnimationTimeMod;

	public static GameManagerTacticsInterface instance;

	[System.NonSerialized]
	public GameManager gm;

	public TargetInfoText targetInfoText;

	public string[] spawnList;

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

		//testing
		if (Input.GetKeyDown(KeyCode.C)){
			gm.board.clearHighlights();
		}
//		Tile targetTest = gm.board.Grid [4, 6];
//		gm.board.raytrace (gm.units [0].CurTile, targetTest, Tile.Cover.Full);
//		Vector3 dist = targetTest.Pos.getV3 () - gm.units [0].CurTile.Pos.getV3 ();
//		Debug.DrawLine(gm.units [0].CurTile.Pos.getV3(), targetTest.Pos.getV3(), Color.red);

	}

	//FILL THIS IN!!!!!
	public bool areAnimationsHappening(){
		//		foreach(Unit unit in units){
		//			if (unit.areAnimationsHappening()){
		//				return true;
		//			}
		//		}

		return false;
	}
}
