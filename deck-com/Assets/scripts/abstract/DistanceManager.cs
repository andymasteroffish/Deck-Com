using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceManager : MonoBehaviour {

	private float[,] dists;

	public DistanceManager(int cols, int rows){
		dists = new float[cols, rows];

		for (int x = 0; x < cols; x++) {
			for (int y = 0; y < rows; y++) {
				dists[x,y] = Mathf.Sqrt( Mathf.Pow(Mathf.Abs(x),2) + Mathf.Pow(Mathf.Abs(y),2) );
			}
		}
	}

	public float getDist(Tile a, Tile b){
		//transpose the points so that one will be at 0,0 and the other will be moved (and/or flipped) the same way
		int newX = Mathf.Abs (a.Pos.x - b.Pos.x);
		int newY = Mathf.Abs (a.Pos.y - b.Pos.y);
		return dists [newX, newY];
	}

	public float getDist(TilePos a, TilePos b){
		//transpose the points so that one will be at 0,0 and the other will be moved (and/or flipped) the same way
		int newX = Mathf.Abs (a.x - b.x);
		int newY = Mathf.Abs (a.y - b.y);
		return dists [newX, newY];
	}
}
