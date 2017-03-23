using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInfo {

	public List<MoveInfo> moves;

	public int totalValue;


	//values for evaluating AI moves
	public int numEnemiesDamaged;
	public int totalEnemyDamage;
	public int numEnemiesKilled;

	public int numAlliesDamaged;
	public int totalAllyDamage;
	public int numAlliesKilled;

	//enemies/allies aided

	//should like getting new charms too if they'll last more than the one turn

	public TurnInfo(MoveInfo move){
		totalValue = 0;
		moves = new List<MoveInfo> ();
		moves.Add (move);
	}

	public void addMoves(TurnInfo other){
		for (int i = 0; i < other.moves.Count; i++) {
			moves.Add (other.moves [i]);
		}

		//we only care about the final board state so all evaluation should be transfered over
		if (moves.Count > 0) {
			numEnemiesDamaged = other.numEnemiesDamaged;
			totalEnemyDamage = other.totalEnemyDamage;
			numEnemiesKilled = other.numEnemiesKilled;

			numAlliesDamaged = other.numAlliesDamaged;
			totalAllyDamage = other.totalAllyDamage;
			numAlliesKilled = other.numAlliesKilled;

			calculateTotalValue ();
		}
	}

	public void print(Board board){
		Unit unit = board.units [moves [0].unitID];
		Debug.Log ("MY SEXY ASS TURN for "+unit.unitName);
		Debug.Log ("value: " + totalValue);
		for (int i = 0; i < moves.Count; i++) {
			Debug.Log (i + ": "+moves[i].cardIDName+" targetting "+moves[i].targetTilePos.x+","+moves[i].targetTilePos.y);
		}

		Debug.Log ("stats:");
		Debug.Log ("enemies damaged: " + numEnemiesDamaged);
		Debug.Log ("total enemy damage: " + totalEnemyDamage);
		Debug.Log ("enemies killed: " + numEnemiesKilled);
		Debug.Log ("allies damaged: " + numAlliesDamaged);
		Debug.Log ("total ally damage: " + totalAllyDamage);
		Debug.Log ("allies killed: " + numAlliesKilled);
	}

	public void resetEvaluations(){
		totalValue = 0;

		numEnemiesDamaged = 0;
		totalEnemyDamage = 0;
		numEnemiesKilled = 0;
		numAlliesDamaged = 0;
		totalAllyDamage = 0;
		numAlliesKilled = 0;
	}

	//YOU NEED TO FEED IN WEIGHTED DESIRES
	//THIS IS JUST A GENERIC TEST
	public void calculateTotalValue(){
		float total = 0;

		total += totalEnemyDamage * 1;
		total += numEnemiesKilled * 5;
		total -= totalAllyDamage * 2;
		total -= numAlliesKilled * 100;

		totalValue = (int)total;	//roudning to into to make other math easier

		//Debug.Log ("tot val " + totalValue);
	}
}
