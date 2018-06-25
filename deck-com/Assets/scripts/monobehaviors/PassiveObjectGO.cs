using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveObjectGO : MonoBehaviour {

	private PassiveObject obj;

	public SpriteRenderer spriteRend;

	public Sprite[] sprites;	//this needs to be in the same order as PassiveObjectType

	private bool isActive;

	private bool doingAnimation;

	private int numTurnsActive;

	public bool hasBeenTriggered;	//some objects need to be turned on by TacticsInterface as part of an animaiton. Not all care about this


	public void activate(PassiveObject _obj){
		obj = _obj;

		isActive = true;
		gameObject.SetActive (true);

		transform.localScale = new Vector3 (1, 1, 1);

		spriteRend.sprite = sprites [(int)obj.type];

		gameObject.name = obj.type.ToString();

		doingAnimation = false;

		numTurnsActive = 0;

		hasBeenTriggered = false;

		//reinforcement marker starts invisible
		if (obj.type == PassiveObject.PassiveObjectType.ReinforcementMarker) {
			transform.localScale = new Vector3 (0, 0, 0);
		}
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


	public void triggerScaleAnimation(float targetScale, float time){
		StartCoroutine (doScaleAnimation (targetScale, time));
	}
	IEnumerator doScaleAnimation(float targetScale, float time){
		doingAnimation = true;

		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;

		float startScale = transform.localScale.x;
		float timer = 0;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			float newScale = prc * targetScale + (1.0f - prc) * startScale;
			transform.localScale = new Vector3 (newScale, newScale, newScale);
			yield return null;
		}

		doingAnimation = false;
		transform.localScale = new Vector3 (targetScale, targetScale, targetScale);
	}

	public bool IsActive{
		get{
			return this.isActive;
		}
	}

	public bool DoingAnimation {
		get {
			return this.doingAnimation;
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
