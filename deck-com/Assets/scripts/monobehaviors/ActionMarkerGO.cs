using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMarkerGO : MonoBehaviour {

	private bool isActive;
	private Unit owner;
	private int idNum;

	private bool needsStartPos = true;
	private Transform startPosPlayer, startPosAI;
	private Transform startPos;
	public float xSpacing;

	private bool doingAnimation;

	public float growTime;

	public float smallScale;
	private bool isDemoingActionCost;

	private ActionMarkerGO child;

	public void activate(Unit unit, int _idNum){
		isActive = true;
		owner = unit;
		idNum = _idNum;
		//check if this is redundant
		if (GameObjectManager.instance.checkForDuplicateActionMarker (this) == true) {
			deactivate ();
			return;
		}
		//or if they don't have the current number of needed actions
		if (owner.ActionsLeft < idNum) {
			deactivate ();
			return;
		}

		gameObject.SetActive(true);

		child = null;

		if (needsStartPos){
			needsStartPos = false;
			startPosPlayer = GameObject.Find("actionMarker_player_startPos").transform;
			startPosAI = GameObject.Find("actionMarker_ai_startPos").transform;
		}

		startPos = owner.isPlayerControlled ? startPosPlayer : startPosAI;

		gameObject.name = "action_marker "+unit.unitName;

		transform.position = startPos.position + new Vector3 (xSpacing * (idNum-1), 0, 0);

		isDemoingActionCost = false;

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

		//should we be demoing action cost for a card?
		if (!doingAnimation) {
			int curCost = 0;
			foreach (Card card in owner.deck.Hand) {
				if (card.mouseIsOver) {
					curCost = card.getNumActionsNeededToPlay ();
				}
			}
			int totalActions = owner.ActionsLeft;

			bool shouldDemoCost = curCost > (totalActions-idNum);
			if (shouldDemoCost != isDemoingActionCost) {
				isDemoingActionCost = shouldDemoCost;
				float newScale = isDemoingActionCost ? smallScale : 1;
				StartCoroutine( doScaleAnim(newScale, growTime, false));
			}
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

		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;
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
	public Unit Owner {
		get {
			return this.owner;
		}
	}
	public int IDNum{
		get{
			return this.idNum;
		}
	}
}
