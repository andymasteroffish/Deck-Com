using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharmGO : MonoBehaviour {

	private Charm charm;

	public Text nameField;
	public Text descField;
	public SpriteRenderer spriteRend;

	public float ySpacing;

	public float moveTime;

	private bool isActive;

	//sliding in and out
	private Vector3 startPos, endPos;
	private Vector3 startAnchor, endAnchor;
	private Vector3 startPosPlayer, endPosPlayer;
	private Vector3 startPosAI, endPosAI;
	private bool needsPositions = true;
	private bool doingAnimation;

	public void activate(Charm _charm){
		charm = _charm;
		isActive = true;
		//check if this is redundant
		if (GameObjectManager.instance.checkForDuplicateCharmGO (this) == true) {
			deactivate ();
			return;
		}

		gameObject.SetActive (true);

		if (needsPositions == true) {
			needsPositions = false;
			startPosPlayer = GameObject.Find ("charm_player_startPos").transform.position;
			endPosPlayer = GameObject.Find ("charm_player_endPos").transform.position;
			startPosAI = GameObject.Find ("charm_ai_startPos").transform.position;
			endPosAI = GameObject.Find ("charm_ai_endPos").transform.position;
		}

		startAnchor = charm.Owner.isPlayerControlled ? startPosPlayer : startPosAI;
		endAnchor = charm.Owner.isPlayerControlled ? endPosPlayer : endPosAI;

		gameObject.name = "charm "+charm.Owner.unitName+" "+charm.name;

		setAnimationPositions ();
		StartCoroutine (doMoveAnimation (startPos, endPos, moveTime, false));
	}

	public void deactivate(){
		charm = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "charm unused";
	}

	// Update is called once per frame
	void Update () {
		nameField.text = charm.name;
		descField.text = charm.description;

		if (charm.hasChangedPos) {
			charm.hasChangedPos = false;
			Debug.Log (charm.idName +" my new order: " + charm.offsetID);
			Debug.Log ("end pos was " + endPos);
			setAnimationPositions ();
			StartCoroutine (doMoveAnimation (transform.position, endPos, moveTime, false));
			Debug.Log("end pos is "+endPos);
		}

		if ( (!charm.Owner.IsActive || charm.isDead || charm.Owner.isDead) && !doingAnimation) {
			StartCoroutine (doMoveAnimation (endPos, startPos, moveTime, true));
		}

		//set the sprite color
		Color col = new Color (1, 1, 1, 1);
		if (charm.Owner.isPlayerControlled && !charm.Owner.GM.IsPlayerTurn) {
			col = new Color (1, 1, 1, 0.4f);
		}
		spriteRend.color = col;
	}

	private void setAnimationPositions(){
		float thisYSpacing = ySpacing;
		if (!charm.Owner.isPlayerControlled) {
			thisYSpacing *= -1;
		}
		startPos = new Vector3 (startAnchor.x, startAnchor.y + charm.offsetID * thisYSpacing, startAnchor.z);
		endPos = new Vector3 (endAnchor.x, endAnchor.y + charm.offsetID * thisYSpacing, endAnchor.z);
	}


	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time, bool deactivateWhenDone){
		doingAnimation = true;

		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;
		float timer = 0;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			prc = Mathf.Pow (prc, 0.75f);
			transform.position = Vector3.Lerp (start, target, prc);
			yield return null;
		}

		doingAnimation = false;
		transform.position = target;

		if (deactivateWhenDone) {
			deactivate ();
		}
	}


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
	public Charm parentCharm{
		get{
			return this.charm;
		}
	}
}
