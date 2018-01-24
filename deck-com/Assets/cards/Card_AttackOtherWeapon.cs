using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_AttackOtherWeapon : Card {

	public int damageMod;
	public int rangeMod;

	public bool usingOtherUnitWeapon;
	public float otherUnitRange;

	private bool onAttackStep = false;
	private Charm newWeapon, oldWeapon;


	public Card_AttackOtherWeapon (){}
	public Card_AttackOtherWeapon(XmlNode _node){
		node = _node;

		rangeMod = 0;
		if (node ["range_mod"] != null) {
			rangeMod = int.Parse (node ["range_mod"].InnerText);
		}

		damageMod = 0;
		if (node ["damage_mod"] != null) {
			damageMod = int.Parse (node ["damage_mod"].InnerText);
		}

		otherUnitRange = 0;
		if (node ["other_unit_range"] != null) {
			otherUnitRange = float.Parse (node ["other_unit_range"].InnerText);
		}

		usingOtherUnitWeapon = false;
		if (node ["use_other_unit_weapon"] != null) {
			usingOtherUnitWeapon = bool.Parse (node ["use_other_unit_weapon"].InnerXml);
		}


	}

	public override void setupBlueprintCustom(){
		
	}

	public override void setupCustom(){
		Card_AttackOtherWeapon blueprintCustom = (Card_AttackOtherWeapon)blueprint;
		damageMod = blueprintCustom.damageMod;
		rangeMod = blueprintCustom.rangeMod;
		usingOtherUnitWeapon = blueprintCustom.usingOtherUnitWeapon;
		otherUnitRange = blueprintCustom.otherUnitRange;
	}


	public override void mouseEnterEffects(){

		Owner.board.highlightTilesInVisibleRange (Owner.CurTile, otherUnitRange, baseHighlightColor);


	}

	public override void setPotentialTargetInfo(Unit unit){
		if (onAttackStep) {
			setPotentialTargetInfoTextForWeapon (unit, damageMod);
		} else {
			Owner.GM.targetInfoText.turnOn ("borrow\n"+unit.Weapon.name, 0, unit, false);
		}
	}

	public override void selectCardCustom(){
		onAttackStep = false;

		oldWeapon = Owner.Weapon;
		WaitingForUnit = true;

		Owner.board.highlightUnitsInVisibleRange (Owner.CurTile, otherUnitRange, true, true, baseHighlightColor);
	}

	public override void passInUnitCustom(Unit unit){
		if (!onAttackStep) {
			newWeapon = unit.Weapon;
			Owner.Weapon = newWeapon;

			selectCardForWeapon (rangeMod);

			//Charm otherWepClone = new Charm (newWeapon);
			Owner.addCharm (newWeapon.idName);

			onAttackStep = true;
			setPotentialTargetInfoTextForWeapon (unit, damageMod);
		} else {
			
			WaitingForUnit = false;
			int damageVal = getWeaponDamageToUnit (unit, damageMod);
			doDamageToUnit( unit, damageVal );

			for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
				Owner.Charms [i].dealWeaponDamageCustom (this, unit, damageVal);
			}

			returnOriginalWep ();

			finish ();
		}
	}

	public override void cancelCustom(){
		

		if (onAttackStep) {
			onAttackStep = false;
			returnOriginalWep ();
			//finish ();
		}
	}

	private void returnOriginalWep(){
		Owner.Weapon = oldWeapon;
		for (int i = Owner.Charms.Count - 1; i >= 0; i--) {
			if (Owner.Charms [i].idName == newWeapon.idName) {
				Owner.removeCharm (Owner.Charms [i]);
			}
		}
	}

	//THIS DOENS'T WORK YET AS THIS CARD REQUIRES MULTIPLE TARGETS
	public override void resolveFromMove(MoveInfo move){
		
	}

}
