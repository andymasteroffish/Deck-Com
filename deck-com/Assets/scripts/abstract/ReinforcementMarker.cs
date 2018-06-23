using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReinforcementMarker : PassiveObject {

	public ReinforcementMarker(TilePos pos){
		type = PassiveObjectType.ReinforcementMarker;
		setupGeneral (pos);

		Debug.Log ("mother fuck reinformcement");
	}

	public ReinforcementMarker(StoreKey parent){
		setAISimFromParent (parent);
	}

}
