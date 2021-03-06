﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class AIProfile {

	private Unit owner;

	//general info
	public float preferedDistToClosestEnemy;
	public float acceptableDistanceRangeToClosestEnemy;

	public float hateForPassing;

	public bool willAttackAllies;	//cards will need to specificly deal with this

	//patrol stuff
	public bool ignoreDistanceChecks;
	public float maxPatrolDistFromLeader;
	public float maxPatrolDistFromLeaderWeight;

	//each of these are the value that the given stat will be multiplied by
	public float totalEnemyDamageWeight;
	public float totalEnemyHealWeight;
	public float numEnemiesKilledWeight;
	public float numEnemiesAidedWeight;
	public float numEnemiesCursedWeight;
	public float deltaEnemyGoodCharmWeight;
	public float deltaEnemyBadCharmWeight;

	public float totalAllyDamageWeight;
	public float totalAllyHealWeight;
	public float numAlliesKilledWeight;
	public float numAlliesAidedWeight;
	public float numAlliesCursedWeight;
	public float deltaAllyGoodCharmWeight;
	public float deltaAllyBadCharmWeight;

	public float distanceToEnemiesWeight;

	public float newAlliesWeight;

	public float[,] coverChange;	//first value represents what it was, second what it is now

	public Dictionary<string, float> preferedCardWeights = new Dictionary<string, float>();

	//need to factor in healing allies or enemies as their own stat

	public AIProfile(Unit _owner){
		owner = _owner;
	}

	public AIProfile(Unit _owner, string xmlName){
		owner = _owner;

		setDefaultValues ();

		if (xmlName != "none") {
			XmlDocument xml = new XmlDocument ();
			xml.Load(Application.dataPath + "/external_data/foes/ai_profiles/"+xmlName+".xml");
			setFromXML(xml.GetElementsByTagName ("ai")[0]);
		}
	}

	private void setDefaultValues(){
		totalEnemyDamageWeight = 2;
		totalEnemyHealWeight = -2;
		numEnemiesKilledWeight = 10;
		numEnemiesAidedWeight = -10;
		numEnemiesCursedWeight = 5;
		deltaEnemyGoodCharmWeight = -10;
		deltaEnemyBadCharmWeight = 5;

		totalAllyDamageWeight = 100;//-6;	//PUT THIS BACK
		totalAllyHealWeight = 1;
		numAlliesKilledWeight = -100;
		numAlliesAidedWeight = 2;
		numAlliesCursedWeight = -6;
		deltaAllyGoodCharmWeight = 2;
		deltaAllyBadCharmWeight = -6;

		preferedDistToClosestEnemy = 7;//owner.Weapon.baseRange - 3;//- 1;
		acceptableDistanceRangeToClosestEnemy = 1.5f;

		ignoreDistanceChecks = false;
		maxPatrolDistFromLeader = 0;
		maxPatrolDistFromLeaderWeight = 0;

		hateForPassing = 1;

		willAttackAllies = false;

		distanceToEnemiesWeight = 1;

		newAlliesWeight = 4;

		coverChange = new float[3, 3];
		for (int i=0; i<3; i++){
			for (int k=0; k<3; k++){
				coverChange[i,k] = 0;
			}
		}
		coverChange [(int)Tile.Cover.None, (int)Tile.Cover.Part] = 2;//4;
		coverChange[(int)Tile.Cover.None, (int)Tile.Cover.Full] = 10;//20;

		coverChange [(int)Tile.Cover.Part, (int)Tile.Cover.None] = -2;//-5;
		coverChange[(int)Tile.Cover.Part, (int)Tile.Cover.Full] = 1;

		coverChange [(int)Tile.Cover.Full, (int)Tile.Cover.None] = -3;//-7;
		coverChange[(int)Tile.Cover.Full, (int)Tile.Cover.Part] = -1;
	}

	public void setPatrolValues(){
		//Debug.Log ("setting patrol vals");

		maxPatrolDistFromLeader = 5;
		maxPatrolDistFromLeaderWeight = -100;

		//keeping the cover stuff
		coverChange = new float[3, 3];
		for (int i=0; i<3; i++){
			for (int k=0; k<3; k++){
				coverChange[i,k] = 0;
			}
		}
		coverChange[(int)Tile.Cover.None, (int)Tile.Cover.Part] = 5;
		coverChange[(int)Tile.Cover.None, (int)Tile.Cover.Full] = 5;

		coverChange[(int)Tile.Cover.Part, (int)Tile.Cover.None] = -5;
		coverChange[(int)Tile.Cover.Part, (int)Tile.Cover.Full] = 5;

		coverChange[(int)Tile.Cover.Full, (int)Tile.Cover.None] = -5;
		coverChange[(int)Tile.Cover.Full, (int)Tile.Cover.Part] = 5;


		//everything else is irrelevant
		totalEnemyDamageWeight = 0;
		totalEnemyHealWeight = 0;
		numEnemiesKilledWeight = 0;
		numEnemiesAidedWeight = 0;
		numEnemiesCursedWeight = 0;
		deltaEnemyGoodCharmWeight = 0;
		deltaEnemyBadCharmWeight = 0;

		totalAllyDamageWeight = 0;
		totalAllyHealWeight = 0;
		numAlliesKilledWeight = 0;
		numAlliesAidedWeight = 0;
		numAlliesCursedWeight = 0;
		deltaAllyGoodCharmWeight = 0;
		deltaAllyBadCharmWeight = 0;

		ignoreDistanceChecks = true;
		preferedDistToClosestEnemy = 0;
		acceptableDistanceRangeToClosestEnemy = 10000;

		hateForPassing = 0;
		willAttackAllies = false;

		distanceToEnemiesWeight = 0;

		newAlliesWeight = 0;


	}

	private void setFromXML(XmlNode node){
		
		foreach (XmlNode child in node.ChildNodes) {
			string nodeName = child.Name;
			float value = 0;
			bool valueB = false;
			float.TryParse (child.InnerText, out value);
			bool.TryParse (child.InnerText, out valueB);
		

			if (nodeName == "preferedDistToClosestEnemy") {
				preferedDistToClosestEnemy = value;
			} else if (nodeName == "acceptableDistanceRangeToClosestEnemy") {
				acceptableDistanceRangeToClosestEnemy = value;
			} else if (nodeName == "hateForPassing") {
				hateForPassing = value;
			} else if (nodeName == "willAttackAllies") {
				willAttackAllies = valueB;
			}  else if (nodeName == "distanceToEnemiesWeight") {
				distanceToEnemiesWeight = value;
			} else if (nodeName == "totalEnemyDamageWeight") {
				totalEnemyDamageWeight = value;
			} else if (nodeName == "totalEnemyHealWeight") {
				totalEnemyHealWeight = value;
			} else if (nodeName == "numEnemiesKilledWeight") {
				numEnemiesKilledWeight = value;
			} else if (nodeName == "numEnemiesAidedWeight") {
				numEnemiesAidedWeight = value;
			} else if (nodeName == "numEnemiesCursedWeight") {
				numEnemiesCursedWeight = value;
			} else if (nodeName == "deltaEnemyGoodCharmWeight") {
				deltaEnemyGoodCharmWeight = value;
			} else if (nodeName == "deltaEnemyBadCharmWeight") {
				deltaEnemyBadCharmWeight = value;
			} else if (nodeName == "totalAllyDamageWeight") {
				totalAllyDamageWeight = value;
			} else if (nodeName == "totalAllyHealWeight") {
				totalAllyHealWeight = value;
			} else if (nodeName == "numAlliesKilledWeight") {
				numAlliesKilledWeight = value;
			} else if (nodeName == "numAlliesAidedWeight") {
				numAlliesAidedWeight = value;
			} else if (nodeName == "numAlliesCursedWeight") {
				numAlliesCursedWeight = value;
			} else if (nodeName == "deltaAllyGoodCharmWeight") {
				deltaAllyGoodCharmWeight = value;
			} else if (nodeName == "deltaAllyBadCharmWeight") {
				deltaAllyBadCharmWeight = value;
			} else if (nodeName == "newAlliesWeight") {
				newAlliesWeight = value;
			}
			else if (nodeName == "coverNoneToPart") {
				coverChange [(int)Tile.Cover.None, (int)Tile.Cover.Part] = value;
			} else if (nodeName == "coverNoneToFull") {
				coverChange [(int)Tile.Cover.None, (int)Tile.Cover.Full] = value;
			} else if (nodeName == "coverPartToNone") {
				coverChange [(int)Tile.Cover.Part, (int)Tile.Cover.None] = value;
			} else if (nodeName == "coverPartToFull") {
				coverChange [(int)Tile.Cover.Part, (int)Tile.Cover.Full] = value;
			} else if (nodeName == "coverFullToNone") {
				coverChange [(int)Tile.Cover.Full, (int)Tile.Cover.None] = value;
			} else if (nodeName == "coverFullToPart") {
				coverChange [(int)Tile.Cover.Full, (int)Tile.Cover.Part] = value;
			} 

			else if (nodeName == "prefered_card") {
				foreach (XmlNode info in child.ChildNodes) {
					preferedCardWeights.Add(info.Name, float.Parse (info.InnerText));
					//Debug.Log ("love " + info.Name + " for " + float.Parse (info.InnerText));
				}
			}

			else {
				Debug.Log ("unkown AI atribute: " + nodeName);
			}

		}

	}


}
