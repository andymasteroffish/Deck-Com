using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class EnemyAI {

	private Unit owner;

	public void setup(Unit _owner, XmlNode node){
		owner = _owner;
	}

	public List<MoveInfo> getMove(Board curBoard, List<Card> hand, int numActions){
		List<MoveInfo> moves = new List<MoveInfo>();

		return moves;
	}

	public int getMaxMoveInOneAction(List<Card> hand){
		int maxVal = -1;
		for (int i = 0; i < hand.Count; i++) {
			if (hand [i].type == Card.CardType.Movement) {
				maxVal = Mathf.Max (maxVal, hand [i].getAIMovementRange ());
			}
		}
		return maxVal;
	}

	public int getMaxAttackRange(List<Card> hand){
		int maxVal = -1;
		for (int i = 0; i < hand.Count; i++) {
			if (hand [i].type == Card.CardType.Attack || hand [i].type == Card.CardType.AttackSpecial) {
				maxVal = Mathf.Max (maxVal, hand [i].getAIAttackRange ());
			}
		}
		return maxVal;
	}


}
