﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class AIProfile {

	private Unit owner;

	//general info
	public float preferedDistToClosestEnemy;
	public float acceptableDistanceRangeToClosestEnemy;

	//each of these are the value that the given stat will be multiplied by
	public float totalEnemyDamageWeight;
	public float numEnemiesKilledWeight;
	public float numEnemiesAidedWeight;

	public float totalAllyDamageWeight;
	public float numAlliesKilledWeight;
	public float numAlliesAidedWeight;

	public float distanceToEnemiesWeight;

	public float[,] coverChange;	//first value represents what it was, second what it is now

	//need to factor in healing allies or enemies as their own stat


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
		numEnemiesKilledWeight = 10;
		numEnemiesAidedWeight = -10;

		totalAllyDamageWeight = -6;
		numAlliesKilledWeight = -100;
		numAlliesAidedWeight = 2;

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

	private void setFromXML(XmlNode node){
		
		foreach (XmlNode child in node.ChildNodes) {
			string nodeName = child.Name;
			float value = float.Parse (child.InnerText);
		

			if (nodeName == "preferedDistToClosestEnemy") {
				preferedDistToClosestEnemy = value;
			} else if (nodeName == "acceptableDistanceRangeToClosestEnemy") {
				acceptableDistanceRangeToClosestEnemy = value;
			} else if (nodeName == "distanceToEnemiesWeight") {
				distanceToEnemiesWeight = value;
			} else if (nodeName == "totalEnemyDamageWeight") {
				totalEnemyDamageWeight = value;
			} else if (nodeName == "numEnemiesKilledWeight") {
				numEnemiesKilledWeight = value;
			} else if (nodeName == "numEnemiesAidedWeight") {
				numEnemiesAidedWeight = value;
			} else if (nodeName == "totalAllyDamageWeight") {
				totalAllyDamageWeight = value;
			} else if (nodeName == "numAlliesKilledWeight") {
				numAlliesKilledWeight = value;
			} else if (nodeName == "numAlliesAidedWeight") {
				numAlliesAidedWeight = value;
			} else if (nodeName == "coverNoneToPart") {
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
			} else {
				Debug.Log ("unkown AI atribute: " + nodeName);
			}

		}

	}


}
