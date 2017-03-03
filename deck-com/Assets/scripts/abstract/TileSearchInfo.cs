using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TileSearchInfo :  IComparable<TileSearchInfo>{

	public Tile tile;
	public TilePos pos;
	public Tile parent;
	public float distFromStart;

	public TileSearchInfo(Tile _tile, Tile _parent){
		tile = _tile;
		pos = tile.Pos;
		parent = _parent;
		distFromStart = 0;
	}

	public int CompareTo(TileSearchInfo other){
		if (other == null)
			return 1;
		else
			return this.distFromStart.CompareTo(other.distFromStart);
	}
}
