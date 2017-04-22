using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInfo {

	public List<MoveInfo> moves;

	public Board debugResultingBoard = null;	//this will be only be used when testing AI and should be left null most of the time

	//set when the board is evaluated
	public float val;	

	public TurnInfo(MoveInfo move){
		//totalValue = 0;
		val = 0;
		moves = new List<MoveInfo> ();
		moves.Add (move);
	}

	public void addMoves(TurnInfo other){
		for (int i = 0; i < other.moves.Count; i++) {
			moves.Add (other.moves [i]);
		}

		debugResultingBoard = other.debugResultingBoard;

		//we only care about the final board state so all evaluation should be transfered over
		if (moves.Count > 0) {
			val = other.val;
		}
	}

	public void print(Board board){
		Unit unit = board.units [moves [0].unitID];
		Debug.Log ("MY SEXY ASS TURN for "+unit.unitName);
		Debug.Log ("value: " + val);
		for (int i = 0; i < moves.Count; i++) {
			if (moves [i].passMove) {
				Debug.Log (i + ": PASS");
			} else {
				Debug.Log (i + ": " + moves [i].cardIDName + " targetting " + moves [i].targetTilePos.x + "," + moves [i].targetTilePos.y);
			}
		}
	}
}
