using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Profiling;

public class Card_BasicAOEAttack : Card {

	public int damage;
	public int throwRange;
	public float damageRange;

	public bool destroysCover;
	public bool harmsFriendly;

	public Card_BasicAOEAttack(){}
	public Card_BasicAOEAttack(XmlNode _node){
		node = _node;

		damage = int.Parse (node ["damage"].InnerXml);
		throwRange = int.Parse (node ["throw_range"].InnerXml);
		damageRange = float.Parse (node ["damage_range"].InnerXml);

		destroysCover = bool.Parse (node ["destroy_cover"].InnerXml);
		harmsFriendly = bool.Parse (node ["harms_friendly"].InnerXml);
	}

	public override void setupBlueprintCustom(){
		
		description = "Deals " + damage + " damage in area.\nRange "+throwRange;
		if (destroysCover){
			description += "\nDestroys cover";
		}
		if (harmsFriendly){
			description += "\ncan hurt allies";
		}
	}

	public override void setupCustom(){
		Card_BasicAOEAttack blueprintCustom = (Card_BasicAOEAttack)blueprint;
		damage = blueprintCustom.damage;
		throwRange = blueprintCustom.throwRange;
		damageRange = blueprintCustom.damageRange;
		destroysCover = blueprintCustom.destroysCover;
		harmsFriendly = blueprintCustom.harmsFriendly;
	}

	public override void mouseEnterEffects(){
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, baseHighlightColor);
	}

	public override void selectCardCustom(){
		WaitingForTile = true;
		Owner.board.highlightTilesInVisibleRange(Owner.CurTile, throwRange, baseHighlightColor);
	}

	public override void passInTileCustom(Tile tile){
		List<Tile> tiles = Owner.board.getTilesInRange (tile, damageRange, Tile.Cover.Full, true);// Owner.board.getTilesInDist (tile, damageRange);

		for (int i = 0; i < tiles.Count; i++) {
			if (destroysCover) {
				Owner.board.changeCover (tiles [i], Tile.Cover.None);	//PUT THIS BACK
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




	public override int checkMoveVal(MoveInfo move, Board board){
		//return 0;

		Unit unit = board.units [move.unitID];
		int moveVal = 0;

		Tile targetTile = board.Grid [move.targetTilePos.x, move.targetTilePos.y];
		List<Tile> tilesHit = Owner.board.getTilesInRange (targetTile, damageRange, Tile.Cover.Full, true);


		int enemiesHit = 0;
		int alliesHit = 0;

		//let's figure out who our enemies are
		Profiler.BeginSample("sorting allies and foes for AOE");
		List<Unit> enemies = new List<Unit> ();
		List<Unit> allies = new List<Unit> ();
		bool rootingForAI = !unit.isPlayerControlled;
		foreach (Unit u in board.units) {
			if (u.isPlayerControlled == rootingForAI) {
				enemies.Add (u);
				if (tilesHit.Contains (u.CurTile)) {
					enemiesHit++;
				}
			} else {
				allies.Add (u);
				if (tilesHit.Contains (u.CurTile) && harmsFriendly) {
					alliesHit++;
				}
			}
		}
		Profiler.EndSample ();


		//we like hitting enemies, but don't like hitting friends
		if (alliesHit == 0 && enemiesHit > 0) {
			moveVal++;
		}
		if (enemiesHit == 0 && alliesHit > 0) {
			moveVal--;
		}

		//if we can destroy cover, see if we're destroying anything near the enemies
		Profiler.BeginSample("checking cover destruction");
		bool hitsEnemyCover = false;
		if (destroysCover) {
			foreach (Tile t in tilesHit) {
				foreach (Tile adjacent in t.Adjacent) {
					foreach (Unit enemy in enemies) {
						if (enemy.CurTile == adjacent) {
							hitsEnemyCover = true;
							break;
						}
					}
				}
			}
		}
		Profiler.EndSample ();

		if (hitsEnemyCover) {
			moveVal++;
		}


		return moveVal;
	}
}
