using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Xml;
using System.IO;

public class CardManager : MonoBehaviour {

	public static CardManager instance = null;

	//public TextAsset xmlText;

	private XmlNodeList nodes;

	private Dictionary<string, Card> cardBlueprints = new Dictionary<string, Card> ();

	private Dictionary<Card.CardType, string> typeNames;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		XmlDocument fullXML = new XmlDocument ();
		fullXML.Load (Application.dataPath + "/external_data/cards.xml");
		nodes = fullXML.GetElementsByTagName ("card");

		//get the cards and store a copy of each one to use as a blueprint
		foreach (XmlNode node in nodes) {
			Card blueprint = getCardFromXMLNode (node);
			//Debug.Log (blueprint != null);
			blueprint.setupBlueprint ();
			cardBlueprints.Add (blueprint.idName, blueprint);
		}
		Debug.Log (cardBlueprints.Count + " unique cards found");

		//set display names for the card types
		typeNames = new Dictionary<Card.CardType, string> ();
		typeNames.Add (Card.CardType.Loot, "");
		typeNames.Add (Card.CardType.Attack, "Attack");
		typeNames.Add (Card.CardType.AttackSpecial, "Special Attack");
		typeNames.Add (Card.CardType.Movement, "Movement");
		typeNames.Add (Card.CardType.Action, "Action");
		typeNames.Add (Card.CardType.Spell, "Spell");
		typeNames.Add (Card.CardType.Equipment, "Equipment");
		typeNames.Add (Card.CardType.Other, "Other");

	}

	public List<Card> getDeckFromTextFile(string filePath){
		List<Card> deck = new List<Card> ();

		string fileText = File.ReadAllText (filePath);

		string[] lines = fileText.Split ('\n');
		for (int i = 0; i < lines.Length; i++) {
			if (lines [i].Length > 1) {
				Card thisCard = getCardFromIdName (lines [i]);
				if (thisCard != null) {
					deck.Add (thisCard);
				} else {
					Debug.Log ("BAD CARD NAME: "+lines [i]+" in deck "+filePath);
				}
			}
		}

		return deck;
	}

	public Card getCardFromIdName(string idName){
//		foreach (XmlNode node in nodes) {
//			if (node.Attributes ["idName"].Value == idName) {
//				return	getCardFromXMLNode (node);
//			}
//		}
		if (cardBlueprints.ContainsKey (idName)) {
			return getCardFromBlueprint(cardBlueprints[idName]);
		}

		Debug.Log ("COULD NOT FIND CARD ID: " + idName);
		return null;
	}

	//used when loading in the cards on start
	public Card getCardFromXMLNode(XmlNode node){
		Card thisCard = null;

		string scriptName = node ["script"].InnerText;

		if (scriptName == "Card_Loot") {
			thisCard = new Card_Loot (node);
		} else if (scriptName == "Card_Movement") {
			thisCard = new Card_Movement (node);
		} else if (scriptName == "Card_Attack") {
			thisCard = new Card_Attack (node);
		} else if (scriptName == "Card_AttackIgnoreWeapon") {
			thisCard = new  Card_AttackIgnoreWeapon (node);
		} else if (scriptName == "Card_BasicAOEAttack") {
			thisCard = new Card_BasicAOEAttack (node);
		} else if (scriptName == "Card_BasicTargetBonus") {
			thisCard = new Card_BasicTargetBonus (node);
		} else if (scriptName == "Card_CoverAttack") {
			thisCard = new Card_CoverAttack (node);
		} else if (scriptName == "Card_GiveCharm") {
			thisCard = new Card_GiveCharm (node);
		} else if (scriptName == "Card_Heal") {
			thisCard = new Card_Heal (node);
		} else if (scriptName == "Card_MoveAndAttack") {
			thisCard = new Card_MoveAndAttack (node);
		} else if (scriptName == "Card_MoveAttackAOE") {
			thisCard = new Card_MoveAttackAOE (node);
		} else if (scriptName == "Card_TeamDash") {
			thisCard = new Card_TeamDash (node);
		} else if (scriptName == "Card_Teleport") {
			thisCard = new Card_Teleport (node);
		} else if (scriptName == "Card_Trade_Places") {
			thisCard = new Card_Trade_Places (node);
		} else if (scriptName == "Card_DirectDamage") {
			thisCard = new Card_DirectDamage (node);
		} else if (scriptName == "Card_Equipment") {
			thisCard = new Card_Equipment (node);
		} else if (scriptName == "Card_RemoveCharm") {
			thisCard = new Card_RemoveCharm (node);
		} else if (scriptName == "Card_SpawnUnit") {
			thisCard = new Card_SpawnUnit (node);
		}
		else{
			Debug.Log ("SCRIPT NAME FOR CARD NOT FOUND: "+scriptName);
		}

		return thisCard;
	}

	//used to actually get new cards during the game
	public Card getCardFromBlueprint(Card blueprint){
		Card thisCard = null;

		string scriptName = blueprint.scriptName;

		if (scriptName == "Card_Loot") {
			thisCard = new Card_Loot ();
		} else if (scriptName == "Card_Movement") {
			thisCard = new Card_Movement ();
		} else if (scriptName == "Card_Attack") {
			thisCard = new Card_Attack ();
		}else if (scriptName == "Card_AttackIgnoreWeapon") {
			thisCard = new  Card_AttackIgnoreWeapon ();
		} else if (scriptName == "Card_BasicAOEAttack") {
			thisCard = new Card_BasicAOEAttack ();
		} else if (scriptName == "Card_BasicTargetBonus") {
			thisCard = new Card_BasicTargetBonus ();
		} else if (scriptName == "Card_CoverAttack") {
			thisCard = new Card_CoverAttack ();
		} else if (scriptName == "Card_GiveCharm") {
			thisCard = new Card_GiveCharm ();
		} else if (scriptName == "Card_Heal") {
			thisCard = new Card_Heal ();
		} else if (scriptName == "Card_MoveAndAttack") {
			thisCard = new Card_MoveAndAttack ();
		} else if (scriptName == "Card_MoveAttackAOE") {
			thisCard = new Card_MoveAttackAOE ();
		} else if (scriptName == "Card_TeamDash") {
			thisCard = new Card_TeamDash ();
		} else if (scriptName == "Card_Teleport") {
			thisCard = new Card_Teleport ();
		} else if (scriptName == "Card_Trade_Places") {
			thisCard = new Card_Trade_Places ();
		} else if (scriptName == "Card_DirectDamage") {
			thisCard = new Card_DirectDamage ();
		} else if (scriptName == "Card_Equipment") {
			thisCard = new Card_Equipment ();
		} else if (scriptName == "Card_RemoveCharm") {
			thisCard = new Card_RemoveCharm ();
		} else if (scriptName == "Card_SpawnUnit") {
			thisCard = new Card_SpawnUnit ();
		}else{
			Debug.Log ("SCRIPT NAME FOR BLUEPRINT NOT FOUND: "+scriptName);
		}
			
		if (thisCard != null) {
			thisCard.blueprint = blueprint;
		}
		return thisCard;
	}


	public List<string> getIDListAtLevel(int level){
		List<string> returnVal = new List<string> ();
		foreach (XmlNode node in nodes) {
			if (int.Parse(node["level"].InnerXml) == level && level >= 0) {
				returnVal.Add (node.Attributes ["idName"].Value);
			}
		}
		return returnVal;
	}


	//setters getters
	public Dictionary<Card.CardType, string> TypeNames{
		get{
			return this.typeNames;
		}
	}




}
