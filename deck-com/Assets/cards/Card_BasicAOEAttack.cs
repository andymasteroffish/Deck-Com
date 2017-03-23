using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Card_BasicAOEAttack : Card {

	public int damage;
	public int throwRange;
	public float damageRange;

	public bool destroysCover;
	public bool harmsFriendly;

	public Card_BasicAOEAttack(XmlNode _node){
		node = _node;

		damage = int.Parse (node ["damage"].InnerXml);
		throwRange = int.Parse (node ["throw_range"].InnerXml);
		damageRange = float.Parse (node ["damage_range"].InnerXml);

		destroysCover = bool.Parse (node ["destroy_cover"].InnerXml);
		harmsFriendly = bool.Parse (node ["harms_friendly"].InnerXml);
	}

	public override void setupCustom(){

		type = CardType.AttackSpecial;

		description = "Deals " + damage + " damage in area.\nRange "+throwRange;
		if (destroysCover){
			description += "\nDestroys cover";
		}
		if (harmsFriendly){
			description += "\ncan hurt allies";
		}
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, attackHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, attackHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		List<Tile> tiles = Owner.board.getTilesInRange (tile, damageRange, Tile.Cover.Full, true);// Owner.board.getTilesInDist (tile, damageRange);

		for (int i = 0; i < tiles.Count; i++) {
			if (destroysCover) {
				tiles [i].setCover (Tile.Cover.None);
			}
			Unit thisUnit = Owner.board.getUnitOnTile (tiles [i]);
			if (thisUnit != null) {
				if (harmsFriendly || (thisUnit.isPlayerControlled != Owner.isPlayerControlled)) {
					doDamageToUnit (thisUnit, damage);
					//thisUnit.takeDamage (damage);
				}
			}
		}

		finish ();
	}

	public override void resolveFromMove(MoveInfo move){
		passInTileCustom ( Owner.board.Grid[move.targetTilePos.x, move.targetTilePos.y]);
	}
}
