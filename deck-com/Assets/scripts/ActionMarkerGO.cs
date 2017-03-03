using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMarkerGO : MonoBehaviour {

	private bool isActive;
	private Unit owner;
	private int idNum;

	private bool needsStartPos = true;
	private Transform startPos;
	public float xSpacing;

	private bool doingAnimation;

	public float growTime;

	private ActionMarkerGO child;

	public void activate(Unit unit, int _idNum){
		isActive = true;
		gameObject.SetActive(true);

		owner = unit;
		idNum = _idNum;

		child = null;

		if (needsStartPos){
			needsStartPos = false;
			startPos = GameObject.Find("actionMarker_startPos").transform;
		}

		gameObject.name = "action_marker "+unit.unitName;

		transform.position = startPos.position + new Vector3 (xSpacing * (idNum-1), 0, 0);

		StartCoroutine (doScaleAnim (1, growTime, false));
	}

	public void deactivate(){
		isActive = false;
		gameObject.SetActive (false);
		owner = null;
		gameObject.name = "action_marker unused";
	}


	
	// Update is called once per frame
	void Update () {
		//kill this if the owner is not active or uses the action
		if (!owner.IsActive || owner.ActionsLeft < idNum) {
			StartCoroutine (doScaleAnim (0, growTime, true));
		}

		//spawn another if the owner has more actions!
		if (owner.ActionsLeft > idNum && child == null) {
			child = GameObjectManager.instance.getActionMarkerGO ();
			child.activate (owner, idNum + 1);
		}

		//and if the action go used, clear the child
		if (child != null) {
			if (!child.isActive) {
				child = null;
			}
		}
	}

	IEnumerator doScaleAnim(float targetScale, float time, bool deactivateWhenDone){
		doingAnimation = true;

		time *= owner.GM.debugAnimationTimeMod;
		float timer = 0;
		float startScale = transform.localScale.x;

		while (timer < time) {
			timer += Time.deltaTime;
			timer = Mathf.Min (1, timer);

			float newScale = Mathf.Lerp(startScale, targetScale,  timer / time);
			newScale = Mathf.Pow (newScale, 2);
			transform.localScale = new Vector3 (newScale, newScale, newScale);

			yield return null;
		}

		transform.localScale = new Vector3 (targetScale, targetScale, targetScale);	

		doingAnimation = false;

		if (deactivateWhenDone) {
			deactivate ();
		}
	}



	//setters and getters
	public bool IsActive{
		get{
			return this.isActive;
		}
		set{
			isActive = value;
		}
	}
	public bool DoingAnimation {
		get {
			return this.doingAnimation;
		}
	}
}
