using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObjectGO : MonoBehaviour {

	private PassiveObject obj;

	public SpriteRenderer spriteRend;

	public Sprite[] sprites;	//this needs to be in the same order as PassiveObjectType

	private bool isActive;

	private int numTurnsActive;

	public bool hasBeenTriggered;	//some objects need to be turned on by TacticsInterface as part of an animaiton. Not all care about this

	public void activate(PassiveObject _obj){
		obj = _obj;

		isActive = true;
		gameObject.SetActive (true);

		Debug.Log ("dog I am " + obj.type);

		spriteRend.sprite = sprites [(int)obj.type];

		gameObject.name = obj.type.ToString();

		numTurnsActive = 0;

		hasBeenTriggered = false;
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

	public int NumTurnsActive{
		get{
			return this.numTurnsActive;
		}
		set{
			numTurnsActive = value;
		}
	}

	public PassiveObject Obj{
		get{
			return this.obj;
		}
	}
}
