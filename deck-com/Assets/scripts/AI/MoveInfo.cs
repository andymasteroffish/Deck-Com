using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInfo {

	public int unitID;
	public string cardIDName;

	public TilePos targetTilePos; //can refer to a tile or unit on that tile
	public TilePos secondTarget;	//this should probably just be an array or a list



	public MoveInfo(){
	}

	public MoveInfo(int unit, string card, TilePos target){
		unitID = unit;
		cardIDName = card;
		targetTilePos = target;
	}
}
