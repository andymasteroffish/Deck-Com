using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class CardBlueprint {

	public string scriptName;

	//generic values
	public string idName;
	public string cardName;

	public int baseActionCost;
	public int costToAddToDeck;

	public int bonusActions;
	public int bonusCards;
	public int bonusHeal;

	public string bonusCardID;

	public bool ignoreTargetCharms;
	public bool ignoreCasterCharms;

	//things for specific cards
	public int damageMod;
	public int rangeMod;

	public int damage;
	public float range;

	//dictionaries for custom values (not as fast as giving them vars)
	Dictionary<string, int> intVals;
	Dictionary<string, int> floatVals;

	public void setup(XmlNode node){

		//grab all of the values & set the defaults

		idName = node.Attributes ["idName"].Value;
		cardName = node ["name"].InnerText;

		baseActionCost = 1;
		if (node ["action_cost"] != null) {
			baseActionCost = int.Parse(node ["action_cost"].InnerText);
		}

		costToAddToDeck = 2;
		if (node ["add_to_deck_cost"] != null) {
			costToAddToDeck = int.Parse(node ["add_to_deck_cost"].InnerText);
		}

		//any bonuses?
		bonusActions = 0;
		if (node ["bonus_actions"] != null) {
			bonusActions = int.Parse (node ["bonus_actions"].InnerText);
		}

		bonusCards = 0;
		if (node ["bonus_cards"] != null) {
			bonusCards = int.Parse (node ["bonus_cards"].InnerText);
		}

		bonusHeal = 0;
		if (node ["bonus_heal"] != null) {
			bonusHeal = int.Parse (node ["bonus_heal"].InnerText);
		}

		bonusCardID = "none";
		if (node ["bonus_card_id"] != null) {
			bonusCardID = node ["bonus_card_id"].InnerText;
		}

		//will this ignore any charms
		ignoreTargetCharms = false;
		if (node["ignore_target_charms"] != null){
			ignoreTargetCharms = bool.Parse (node ["ignore_target_charms"].InnerText);
		}

		ignoreCasterCharms = false;
		if (node["ignore_user_charms"] != null){
			ignoreCasterCharms = bool.Parse (node ["ignore_user_charms"].InnerText);
		}

		//things for specific cards
		rangeMod = 0;
		if (node ["range_mod"] != null) {
			rangeMod = int.Parse (node ["range_mod"].InnerText);
		}

		damageMod = 0;
		if (node ["damage_mod"] != null) {
			damageMod = int.Parse (node ["damage_mod"].InnerText);
		}

		damage = 0;
		if (node ["damage"] != null) {
			damage = int.Parse (node ["damage"].InnerXml);
		}

		range = 0;
		if (node ["range"] != null) {
			range = float.Parse (node ["range"].InnerXml);
		}
	}
}
