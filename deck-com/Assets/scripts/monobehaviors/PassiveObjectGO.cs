using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObjectGO : MonoBehaviour {

	private PassiveObject obj;

	public SpriteRenderer spriteRend;

	public Sprite[] sprites;	//this needs to be in the same order as PassiveObjectType

	private bool isActive;


	public void activate(PassiveObject _obj){
		obj = _obj;

		isActive = true;
		gameObject.SetActive (true);

		spriteRend.sprite = sprites [(int)obj.type];

		gameObject.name = obj.type.ToString();
	}

	public void deactivate(){
		obj = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "passive object (inactive)";
	}

	void Update(){
		transform.position = obj.CurTilePos.getV3 ();

		if (obj.isDone) {
			deactivate ();
		}
	}




	public bool IsActive{
		get{
			return this.isActive;
		}
	}
}
