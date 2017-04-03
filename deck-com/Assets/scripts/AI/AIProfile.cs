using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIProfile {

	private Unit owner;

	//general info
	public float targetDistMin;
	public float targetDistMax;

	//each of these are the value that the given stat will be multiplied by
	public float totalEnemyDamage;
	public float numEnemiesKilled;
	public float numEnemiesAided;

	public float totalAllyDamage;
	public float numAlliesKilled;
	public float numAlliesAided;

	public float totalDistCloserToTargetDistances;




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

		targetDistMin = owner.Weapon.baseRange;
		targetDistMax = owner.Weapon.baseRange - 3;

		totalDistCloserToTargetDistances = 1;
	}


}
