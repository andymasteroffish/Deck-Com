using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

public class Charm_LatchOn : Charm {

	public Unit targetUnit;
	private bool needsTarget;

	private TilePos otherPosWhenLatched;

	public Charm_LatchOn(XmlNode _node){
		node = _node;
	}
	public Charm_LatchOn(Charm parent){
		setFromParent (parent);
	}

	public virtual void setFromParentCustom(Charm parent){
		Charm_LatchOn p = (Charm_LatchOn)parent;

		targetUnit = p.targetUnit;
		needsTarget = p.needsTarget;
		otherPosWhenLatched = new TilePos (p.otherPosWhenLatched);
	}

	public override void setupCustom(){
		targetUnit = null;
		needsTarget = true;
		otherPosWhenLatched = new TilePos (-1, -1);

		className = CharmClass.LatchOn;

		//The leech doesn't recieve AI bad points for this. The leech likes it!
		if (Owner.idName == "leech") {
			aiGoodCharmPoints = 4;
			aiBadCharmPoints = 0;
		}
	}

	public void setTarget(Unit _targetUnit){
		targetUnit = _targetUnit;
		needsTarget = false;
		otherPosWhenLatched = new TilePos( targetUnit.CurTile.Pos );
	}


	//falls off if hit
//	public override void takeDamageCustom (Card card, Unit source){
//		Owner.removeCharm (this);
//	}


	//no moving while latched on
	public override int getCardActionCostModCustom(Card card){
		if (card.type == Card.CardType.Movement) {
			return 999;
		}

		return 0;
	}

	//if target moves or is killed, this goes away
	public override void anyActionTakenCustom(){
		if (targetUnit.isDead || targetUnit.CurTile.Pos != otherPosWhenLatched) {
			Owner.removeCharm (this);
		}
	}
}
