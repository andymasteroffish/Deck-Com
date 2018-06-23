using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreKey : PassiveObject {

	public StoreKey(TilePos pos){
		type = PassiveObjectType.StoreKey;
		setupGeneral (pos);

	}

	public StoreKey(StoreKey parent){
		setAISimFromParent (parent);
	}

	public override void checkBoard(Board board){
		Unit unitOnMe = board.getUnitOnTile (CurTilePos);
		if (unitOnMe != null) {
			if (unitOnMe.isPlayerControlled) {
				Debug.Log ("gimme dat shit");
				GameManagerTacticsInterface.instance.gm.hasStoreKey = true;
				isDone = true;
			}
		}

	}

	public override void playerTurnStart(){
		Debug.Log ("player testo");
	}

	public override void AITurnStart(){
		Debug.Log ("Ai testo");
	}
}
