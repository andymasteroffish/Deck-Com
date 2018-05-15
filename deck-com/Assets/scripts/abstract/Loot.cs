using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot {
	
	private bool createGO = true;

	//public Type type;
	public int level;

	//loot lives on a unit until that unit dies
	private Unit curUnit;
	private Tile curTile;

	public bool isDone;

	public Loot(Unit holder, int curArea){
		curUnit = holder;
		curTile = null;
		level = curArea;
		isDone = false;
	}

	public void checkShouldDrop(Unit deadUnit){
		if (curUnit == deadUnit) {
			drop ();
		}
	}

	void drop(){
		curTile = curUnit.CurTile;

		if (createGO) {
			GameObjectManager.instance.getLootGO ().activate (this);
		}

		curUnit = null;
	}

	public bool canBeCollected(Unit collector){
		if (curTile == null) {
			return false;
		}

		if (curTile == collector.CurTile) {
			return true;
		}

		for (int i = 0; i < CurTile.AdjacentIncludingDiag.Length; i++) {
			if (CurTile.AdjacentIncludingDiag [i] == collector.CurTile) {
				return true;
			}
		}
		return false;
	}

	public void collect(Unit collector){
		//give them a loot card
		Card_Loot card = (Card_Loot) CardManager.instance.getCardFromIdName ("loot");
		card.lootSetup (level);
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
