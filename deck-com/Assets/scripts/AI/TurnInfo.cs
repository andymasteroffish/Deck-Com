﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInfo {

	public List<MoveInfo> moves;

	public Board debugResultingBoard = null;	//this will be only be used when testing AI and should be left null most of the time

	//set when the board is evaluated
	public float val;	




//	public int totalValue; //OLD. REMOVE
//
//	public int totalAllies;	//OLD. REMOVE
//
//	//values for evaluating AI moves
//	//REMOVE ALL OF THIS
//	public int numEnemiesDamaged;
//	public int totalEnemyDamage;
//	public int numEnemiesKilled;
//
//	public int numAlliesDamaged;
//	public int totalAllyDamage;
//	public int numAlliesKilled;
//
//	//enemies/allies aided (allies aided multipe times will count twice)
//	public int numAlliesAided;
//	public int numEnemiesAided;
//
//	//being in cover (each unit is evaluated by the lowest cover value to any player unit)
//	public int[] numAlliesCover = new int[3];
//
//	//moving to or from player units (these values can be negative if the move would make them further away)
//	public int numUnitsCloserToTargetDist;
//	public float totalDistCloserToTargetDistances;

	//should like getting new charms too if they'll last more than the one turn

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

//			numEnemiesDamaged = other.numEnemiesDamaged;
//			totalEnemyDamage = other.totalEnemyDamage;
//			numEnemiesKilled = other.numEnemiesKilled;
//
//			numAlliesDamaged = other.numAlliesDamaged;
//			totalAllyDamage = other.totalAllyDamage;
//			numAlliesKilled = other.numAlliesKilled;
//
//			numAlliesAided = other.numAlliesAided;
//			numEnemiesAided = other.numEnemiesAided;
//
//			for (int i = 0; i < 3; i++) {
//				numAlliesCover [i] = other.numAlliesCover [i];
//			}
//
//			numUnitsCloserToTargetDist = other.numUnitsCloserToTargetDist;
//			totalDistCloserToTargetDistances = other.totalDistCloserToTargetDistances;
//
//			calculateTotalValue ();
		}
	}

	public void print(Board board){
		Unit unit = board.units [moves [0].unitID];
		Debug.Log ("MY SEXY ASS TURN for "+unit.unitName);
		//Debug.Log ("old value: " + totalValue);
		Debug.Log ("value: " + val);
		for (int i = 0; i < moves.Count; i++) {
			if (moves [i].passMove) {
				Debug.Log (i + ": PASS");
			} else {
				Debug.Log (i + ": " + moves [i].cardIDName + " targetting " + moves [i].targetTilePos.x + "," + moves [i].targetTilePos.y);
			}
		}

//		if (true) {
//			Debug.Log ("stats:");
//			Debug.Log ("enemies damaged: " + numEnemiesDamaged);
//			Debug.Log ("total enemy damage: " + totalEnemyDamage);
//			Debug.Log ("enemies killed: " + numEnemiesKilled);
//			Debug.Log ("allies damaged: " + numAlliesDamaged);
//			Debug.Log ("total ally damage: " + totalAllyDamage);
//			Debug.Log ("allies killed: " + numAlliesKilled);
//
//			Debug.Log("num allies aided: "+numAlliesAided);
//			Debug.Log("num enemies aided: "+numEnemiesAided);
//
//			Debug.Log ("units closer to target: " + numUnitsCloserToTargetDist);
//			Debug.Log ("total dist closer: " + totalDistCloserToTargetDistances);
//
//			Debug.Log ("allies in full cover " + numAlliesCover[(int)Tile.Cover.Full]);
//			Debug.Log ("allies in part cover " + numAlliesCover[(int)Tile.Cover.Part]);
//			Debug.Log ("allies in no cover " + numAlliesCover[(int)Tile.Cover.None]);
//		}
	}

//	public void resetEvaluations(){
//		totalValue = 0;
//		totalAllies = 0;
//
//		numEnemiesDamaged = 0;
//		totalEnemyDamage = 0;
//		numEnemiesKilled = 0;
//		numAlliesDamaged = 0;
//		totalAllyDamage = 0;
//		numAlliesKilled = 0;
//
//		numAlliesAided = 0;
//		numEnemiesAided = 0;
//
//		numUnitsCloserToTargetDist = 0;
//		totalDistCloserToTargetDistances = 0;
//
//		for (int i = 0; i < 3; i++) {
//			numAlliesCover [i] = 0;
//		}
//	}

	//YOU NEED TO FEED IN WEIGHTED DESIRES
	//THIS IS JUST A GENERIC TEST
//	public void calculateTotalValue(){
//		float total = 0;
//
//		total += totalEnemyDamage * 2;
//		total += numEnemiesKilled * 10;
//		total -= totalAllyDamage * 10;
//		total -= numAlliesKilled * 100;
//
//		total += numAlliesAided * 1;
//		total -= numEnemiesAided * 10;
//
//		total += (float)numUnitsCloserToTargetDist * 1;
//		total += totalDistCloserToTargetDistances * 1;
//
//		total += numAlliesCover [(int)Tile.Cover.Full] * 2;
//		total += numAlliesCover [(int)Tile.Cover.Part] * 0;
//		total += numAlliesCover [(int)Tile.Cover.None] * -5;
//
//		totalValue = (int)total;	//rounding to into to make other math easier
//
//		//Debug.Log ("tot val " + totalValue);
//	}
}
