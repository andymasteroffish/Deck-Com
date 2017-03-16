using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot {

	public enum Type{money, booster};

	private bool createGO = true;

	public Type type;
	public int level;

	//loot lives on a unit until that unit dies
	private Unit curUnit;
	private Tile curTile;
	private List<Tile> adjacentTiles = new List<Tile>();

	public bool isDone;

	public Loot(Unit holder, int _level){
		curUnit = holder;
		curTile = null;
		type = Random.value > 0.5f ? Loot.Type.booster : Loot.Type.money;
		level = _level;
		isDone = false;
	}

	public void checkShouldDrop(Unit deadUnit){
		if (curUnit == deadUnit) {
			drop ();
		}
	}

	void drop(){
		curTile = curUnit.CurTile;
		adjacentTiles = curUnit.GM.board.getAdjacentTiles (curTile, true, Tile.Cover.Full);
		adjacentTiles.Add (curTile);

		if (createGO) {
			GameObjectManager.instance.getLootGO ().activate (this);
		}

		curUnit = null;
	}

	public bool canBeCollected(Unit collector){
		if (curTile == null) {
			return false;
		}

		for (int i = 0; i < adjacentTiles.Count; i++) {
			if (adjacentTiles [i] == collector.CurTile) {
				return true;
			}
		}
		return false;
	}

	public void collect(Unit collector){
		//give them a loot card
		Card_Loot card = (Card_Loot) CardManager.instance.getCardFromIdName ("loot");
		card.lootSetup (type, level);
		card.setup (collector, collector.deck);
		collector.deck.addCardToHand (card);

		//mark that we're done
		isDone = true;
	}


	//setters and getters
	public Tile CurTile{
		get{
			return this.curTile;
		}
	}

}
