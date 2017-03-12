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

	public GameObject pickupLootButton;

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

		//showing the loot button if applicable
		pickupLootButton.SetActive( gm.activeUnit.CanPickupLoot);

	}

	public void pickUpLoot(){
		gm.activeUnit.pickUpLoot ();
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
