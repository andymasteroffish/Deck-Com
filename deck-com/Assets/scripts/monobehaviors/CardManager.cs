﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class CardManager : MonoBehaviour {

	public static CardManager instance = null;

	public TextAsset xmlText;

	private XmlDocument fullXML;
	private XmlNodeList nodes;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		fullXML = new XmlDocument ();
		fullXML.LoadXml (xmlText.text);
		nodes = fullXML.GetElementsByTagName ("card");
	}

	public List<Card> getDeckFromTextFile(TextAsset file){
		List<Card> deck = new List<Card> ();

		string[] lines = file.text.Split ('\n');
		for (int i = 0; i < lines.Length; i++) {
			if (lines [i].Length > 1) {
				Card thisCard = getCardFromIdName (lines [i]);
				if (thisCard != null) {
					deck.Add (thisCard);
				} else {
					Debug.Log ("BAD CARD NAME: "+lines [i]+" in deck "+file.name);
				}
			}
		}

		return deck;
	}

	public Card getCardFromIdName(string idName){
		foreach (XmlNode node in nodes) {
			if (node.Attributes ["idName"].Value == idName) {
				return	getCardFromXMLNode (node);
			}
		}
		Debug.Log ("COULD NOT FIND CARD ID: " + idName);
		return null;
	}

	public Card getCardFromXMLNode(XmlNode node){
		Card thisCard = null;

		string scriptName = node ["script"].InnerText;

		if (scriptName == "Card_Movement") {
			thisCard = new Card_Movement (node);
		}
		else if (scriptName == "Card_Attack") {
			thisCard = new Card_Attack (node);
		}
		else if (scriptName == "Card_AttackIgnoreWeapon") {
			thisCard = new  Card_AttackIgnoreWeapon(node);
		}
		else if (scriptName == "Card_BasicAOEAttack") {
			thisCard = new Card_BasicAOEAttack (node);
		}
		else if (scriptName == "Card_BasicTargetBonus") {
			thisCard = new Card_BasicTargetBonus (node);
		}
		else if (scriptName == "Card_CoverAttack") {
			thisCard = new Card_CoverAttack (node);
		}
		else if (scriptName == "Card_GiveCharm") {
			thisCard = new Card_GiveCharm (node);
		}
		else if (scriptName == "Card_Heal") {
			thisCard = new Card_Heal (node);
		}
		else if (scriptName == "Card_MoveAndAttack") {
			thisCard = new Card_MoveAndAttack (node);
		}
		else if (scriptName == "Card_MoveAttackAOE") {
			thisCard = new Card_MoveAttackAOE (node);
		}
		else if (scriptName == "Card_MovementSelfDestruct") {
			thisCard = new Card_MovementSelfDestruct (node);
		}
		else if (scriptName == "Card_TeamDash") {
			thisCard = new Card_TeamDash (node);
		}
		else if (scriptName == "Card_Teleport") {
			thisCard = new Card_Teleport (node);
		}

		else{
			Debug.Log ("SCRIPT NAME FOR CHARM NOT FOUND: "+scriptName);
		}


		return thisCard;
	}



}