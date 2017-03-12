using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootGO : MonoBehaviour {

	private Loot loot;

	public SpriteRenderer spriteRend;

	private bool isActive;


	public void activate(Loot _loot){
		loot = _loot;

		isActive = true;
		gameObject.SetActive (true);

		transform.position = loot.CurTile.Pos.getV3 ();

		gameObject.name = "loot";
	}

	public void deactivate(){
		loot = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "loot (inactive)";
	}

	void Update(){
		if (loot.isDone) {
			deactivate ();
		}
	}




	public bool IsActive{
		get{
			return this.isActive;
		}
	}
}
