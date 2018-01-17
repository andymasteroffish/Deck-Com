using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;


//THIS IS A SPECIAL CHARM TO KEEP AIs PATROLLING AND NOT PLAYING THEIR OTHER CARDS

public class Charm_PatrolStatus : Charm {

	AIProfile patrolProfile;


	public Charm_PatrolStatus(XmlNode _node){
		node = _node;
	}
	public Charm_PatrolStatus(Charm parent){
		setFromParent (parent);
	}

	public override void setupCustom(){
		type = Charm.CharmType.StatusEffect;
		className = CharmClass.PatrolStatus;

		patrolProfile = new AIProfile (Owner);
		patrolProfile.setPatrolValues ();
	}

	public override void setFromParentCustom(Charm parent){
		patrolProfile = ((Charm_PatrolStatus)parent).patrolProfile;
	}

	public override void startAITurnCustom(){
		//if the unit is awake, this is done
		if (Owner.isAwake) {
			Owner.removeCharm (this);
		} 
		//otherwise give them the move card
		else {
			Card card = CardManager.instance.getCardFromIdName ("patrol_move_5");
			card.setup (Owner, Owner.deck);
			Owner.deck.addCardToHand (card);
		}
	}

	//if a unit wakes up during their turn, this should be removed at the end of their turn
	public override void turnEndPreDiscardCustom(){
		Debug.Log ("CHECK ME");
		if (Owner.isAwake) {
			Owner.removeCharm (this);
		} 
	}


	public override void takeDamageCustom(Card card, Unit source){
		Owner.removeCharm (this);
	}

	//all cards that aren't the patrol moves should be ignored
	public override int getCardActionCostModCustom(Card card){
		if (card.idName == "patrol_move_5" || card.idName == "patrol_move_6") {
			return 0;
		}

		return 999;
	}

	public override AIProfile checkReplaceAIProfile(){
		return patrolProfile;
	}

	public override float getAIMoveValue(Board oldBoard, Board newBoard, Unit curUnit, ref TurnInfo info, bool printInfo){

		float val = 0;

		if (Owner.isPodLeader) {
			return val;
		} else {
			//get pod leader location
			TilePos leaderPos = null;
			foreach (Unit mate in Owner.podmates) {
				if (mate.isPodLeader) {
					leaderPos = mate.CurTile.Pos;
				}
			}
			if (ReferenceEquals(leaderPos,null)) {
				Debug.Log ("oh fuck. POD LEADER NOT FOUND");
			}

			//the turn should just be a single move action, so we can grab the target position from that
			TilePos moveTarget = info.moves[0].targetTilePos;

//			int unitID = oldBoard.getUnitID (curUnit);
//			Debug.Log ("unit ID: "+unitID);
//			Debug.Log ("spot: " + newBoard.units [unitID].CurTile.Pos.x + "," + newBoard.units [unitID].CurTile.Pos.x);

			//how far are we from the leader?
			float distFromLeader = newBoard.dm.getDist(leaderPos, moveTarget);

			if (distFromLeader < patrolProfile.maxPatrolDistFromLeader) {
				val = 1;
			} else {
				float distOutOfRange = distFromLeader - patrolProfile.maxPatrolDistFromLeader;
				val = distOutOfRange * patrolProfile.maxPatrolDistFromLeaderWeight;
			}


			if (printInfo) {
				
				Debug.Log ("leader at " + leaderPos.x + "," + leaderPos.y);
				Debug.Log ("Im at " + curUnit.CurTile.Pos.x + "," + curUnit.CurTile.Pos.y);
				Debug.Log ("my new spot is " + distFromLeader + " away");
				Debug.Log ("total weight is " + val);

				foreach (MoveInfo move in info.moves) {
					Debug.Log("move: "+move.cardIDName + " targetting " + move.targetTilePos.x + "," + move.targetTilePos.y);
				}

			}



		}

		return val;
	}
}
