using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveInfo {

	//public Unit actingUnit;
	public Card card;
	public TilePos targetTilePos; //can refer to a tile or unit on that tile

	public TilePos secondTarget;


	//values for evaluating AI moves
	public Tile.Cover lowestCover;
	public bool isFlanking;
	//etc.


}
