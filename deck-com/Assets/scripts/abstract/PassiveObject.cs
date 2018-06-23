using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObject {

	public enum PassiveObjectType { StoreKey, ReinforcementMarker, None };

	private TilePos curTilePos;

	private bool isAISim;

	public bool isDone;

	public PassiveObjectType type;

	public void setupGeneral(TilePos _curTile){
		curTilePos = new TilePos(_curTile);
		isDone = false;
		isAISim = false;
		GameObjectManager.instance.getPassiveObjectGO ().activate (this);
	}

	public void setAISimFromParent(PassiveObject parent){
		curTilePos = new TilePos(parent.curTilePos);
		isDone = parent.isDone;
		isAISim = true;
		type = parent.type;
	}

	public virtual void checkBoard(Board board){}

	public virtual void playerTurnStart(){}

	public virtual void AITurnStart(){}


	//setters and getters
	public TilePos CurTilePos{
		get{
			return this.curTilePos;
		}
	}
}
