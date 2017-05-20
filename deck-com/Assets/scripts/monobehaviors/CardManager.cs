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

	void Awake(){
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		XmlDocument fullXML = new XmlDocument ();
		fullXML.Load (Application.dataPath + "/external_data/cards.xml");
		nodes = fullXML.GetElementsByTagName ("card");
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
		} else if (scriptName == "Card_GiveBadCharm") {
			thisCard = new Card_GiveBadCharm (node);
		} else if (scriptName == "Card_Heal") {
			thisCard = new Card_Heal (node);
		} else if (scriptName == "Card_MoveAndAttack") {
			thisCard = new Card_MoveAndAttack (node);
		} else if (scriptName == "Card_MoveAttackAOE") {
			thisCard = new Card_MoveAttackAOE (node);
		} else if (scriptName == "Card_MovementSelfDestruct") {
			thisCard = new Card_MovementSelfDestruct (node);
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
		}

		else{
			Debug.Log ("SCRIPT NAME FOR CARD NOT FOUND: "+scriptName);
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

}
