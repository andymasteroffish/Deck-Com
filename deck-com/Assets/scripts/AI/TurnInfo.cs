using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInfo {

	public List<MoveInfo> moves;

	public float totalValue;


	//values for evaluating AI moves
	public Tile.Cover lowestCover;
	public bool isFlanking;
	//etc.


	public TurnInfo(MoveInfo move){
		totalValue = 0;
		moves = new List<MoveInfo> ();
		moves.Add (move);
	}

	public void addMoves(TurnInfo other){
		for (int i = 0; i < other.moves.Count; i++) {
			moves.Add (other.moves [i]);
		}
	}

	public void print(Board board){
		Unit unit = board.units [moves [0].unitID];
		Debug.Log ("MY SEXY ASS TURN for "+unit.unitName);
		for (int i = 0; i < moves.Count; i++) {
			Debug.Log (i + ": "+unit.deck.Hand[ moves [i].cardID].idName);
		}
	}
}
