using UnityEngine;
using System.Collections;

public class TilePos {

	public int x,y;
	
	public TilePos(){
		set (0,0);
	}
	
	public TilePos(int _x, int _y){
		set (_x,_y);
	}
	
	public TilePos(TilePos orig){
		set (orig);
	}
	
	public void set(int _x, int _y){
		x = _x;
		y = _y;
	}
	
	public void set(TilePos orig){
		x = orig.x;
		y = orig.y;
	}
	
	public TilePos addDirection( Tile.Direction dir ){
		TilePos returnVal = new TilePos( this );
		if (dir == Tile.Direction.Up ){
			returnVal.y++;
		}
		if (dir == Tile.Direction.Right ){
			returnVal.x++;
		}
		if (dir == Tile.Direction.Down ){
			returnVal.y--;
		}
		if (dir == Tile.Direction.Left ){
			returnVal.x--;
		}
		
		return returnVal;
	}

	public Tile.Direction getDirectionToAdjacent(TilePos other){
		
		if (x < other.x && y == other.y){
			return Tile.Direction.Right;
		}
		else if (x > other.x && y == other.y){
			return Tile.Direction.Left;
		}

		else if (y > other.y && x == other.x){
			return Tile.Direction.Down;
		}
		else if (y < other.y && x == other.x){
			return Tile.Direction.Up;
		}

		Debug.Log("BAD! "+x+","+y+" to "+other.x+","+other.y);
		return Tile.Direction.None;

	}

//	public float getDist(TilePos other){
//		return Mathf.Sqrt( Mathf.Pow(Mathf.Abs(x-other.x),2) + Mathf.Pow(Mathf.Abs(y-other.y),2) );
//	}
//	public float getDistSq(TilePos other){
//		return Mathf.Pow(Mathf.Abs(x-other.x),2) + Mathf.Pow(Mathf.Abs(y-other.y),2) ;
//	}


	public Vector2 getV2(){
		return new Vector2 (x, y);
	}
	public Vector3 getV3(){
		return new Vector3 (x, y, 0);
	}

	public static bool operator ==(TilePos a, TilePos b){
//		if (a == null && b == null) {
//			return true;
//		} else if (a == null || b == null) {
//			return false;
//		}
	
		return a.x==b.x && a.y==b.y;
	}

	public static bool operator !=(TilePos a, TilePos b){
//		if (a == null && b == null) {
//			return false;
//		} else if (a == null || b == null) {
//			return true;
//		}

		return !(a.x==b.x && a.y==b.y);
	}
	
	public static TilePos operator +(TilePos a, TilePos b){
		return new TilePos( a.x+b.x, a.y+b.y);
	}
	public static TilePos operator -(TilePos a, TilePos b){
		return new TilePos( a.x-b.x, a.y-b.y);
	}


	public bool Equals( TilePos b ){
		return x==b.x && y==b.y;
	}

	
}
