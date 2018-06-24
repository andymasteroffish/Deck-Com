using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReinforcementMarker : PassiveObject {

	public int challengeRating;

	public ReinforcementMarker(TilePos pos, int cr){
		type = PassiveObjectType.ReinforcementMarker;
		challengeRating = cr;
		setupGeneral (pos);

		Debug.Log ("mother fuck reinformcement");
	}

	public ReinforcementMarker(StoreKey parent){
		setAISimFromParent (parent);
	}

}
