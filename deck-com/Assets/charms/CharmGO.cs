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
	private Transform startTrans, endTrans = null;
	private Vector3 startPos, endPos;
	private bool doingAnimation;

	public void activate(Charm _charm){
		charm = _charm;
		isActive = true;
		gameObject.SetActive (true);

		if (startTrans == null) {
			startTrans = GameObject.Find ("charmStartPos").transform;
			endTrans = GameObject.Find ("charmEndPos").transform;
		}

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

		if ( (!charm.Owner.IsActive || charm.isDead) && !doingAnimation) {
			StartCoroutine (doMoveAnimation (endPos, startPos, moveTime, true));
		}

	}

	private void setAnimationPositions(){
		startPos = new Vector3 (startTrans.position.x, startTrans.position.y + charm.offsetID * ySpacing, startTrans.position.z);
		endPos = new Vector3 (endTrans.position.x, endTrans.position.y + charm.offsetID * ySpacing, endTrans.position.z);
	}


	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time, bool deactivateWhenDone){
		doingAnimation = true;

		time *= charm.Owner.GM.debugAnimationTimeMod;
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
}
