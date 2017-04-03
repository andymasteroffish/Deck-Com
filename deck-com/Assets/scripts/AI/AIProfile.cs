using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIProfile {

	private Unit owner;

	//general info
	public float preferedDistToClosestEnemy;
	public float acceptableDistanceRangeToClosestEnemy;

	//each of these are the value that the given stat will be multiplied by
	public float totalEnemyDamage;
	public float numEnemiesKilled;
	public float numEnemiesAided;

	public float totalAllyDamage;
	public float numAlliesKilled;
	public float numAlliesAided;

	public float distanceToEnemiesWeight;

	public float[,] coverChange;	//first value represents what it was, second what it is now

	//need to factor in healing allies or enemies as their own stat


	public AIProfile(Unit _owner){
		owner = _owner;

		setDefaultValues ();
	}

	private void setDefaultValues(){
		totalEnemyDamage = 2;
		numEnemiesKilled = 10;
		numEnemiesAided = -10;

		totalAllyDamage = -6;
		numAlliesKilled = -100;
		numAlliesAided = 2;

		preferedDistToClosestEnemy = owner.Weapon.baseRange - 1;
		acceptableDistanceRangeToClosestEnemy = 1.5f;

		distanceToEnemiesWeight = 1;

		coverChange = new float[3, 3];
		for (int i=0; i<3; i++){
			for (int k=0; k<3; k++){
				coverChange[i,k] = 0;
			}
		}
		coverChange[(int)Tile.Cover.None, (int)Tile.Cover.Part] = 4;
		coverChange[(int)Tile.Cover.None, (int)Tile.Cover.Full] = 50;

		coverChange[(int)Tile.Cover.Part, (int)Tile.Cover.None] = -5;
		coverChange[(int)Tile.Cover.Part, (int)Tile.Cover.Full] = 1;

		coverChange[(int)Tile.Cover.Full, (int)Tile.Cover.None] = -7;
		coverChange[(int)Tile.Cover.Full, (int)Tile.Cover.Part] = -1;
	}


}
