using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameDisplayFrame : MonoBehaviour {

	private bool doingAnimation;
	private Vector3 startPos, endPos;

	public float xeno;

	private GameManager gm;

	public Text text;

	// Use this for initialization
	void Start () {
		startPos = GameObject.Find ("charm_ai_startPos").transform.position;
		endPos = GameObject.Find ("charm_ai_endPos").transform.position;
		gm = GameManagerTacticsInterface.instance.gm;
	}


	
	// Update is called once per frame
	void Update () {

		//set the text and get ready to slide!
		Vector3 targetPos = transform.position;
		if (gm.activeAIUnit != null) {
			text.text = gm.activeAIUnit.unitName;
			targetPos.x = endPos.x;
		} else {
			text.text = "";
			targetPos.x = startPos.x;
		}

		transform.position = Vector3.Lerp (transform.position, targetPos, xeno);


	}


//	IEnumerator doMoveAnimation(Vector3 start, Vector3 target, float time, bool deactivateWhenDone){
//		doingAnimation = true;
//
//		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;
//		float timer = 0;
//
//		while (timer < time) {
//			timer += Time.deltaTime;
//			float prc = Mathf.Clamp (timer / time, 0, 1);
//			prc = Mathf.Pow (prc, 0.75f);
//			transform.position = Vector3.Lerp (start, target, prc);
//			yield return null;
//		}
//
//		doingAnimation = false;
//		transform.position = target;
//
//		if (deactivateWhenDone) {
//			deactivate ();
//		}
//	}




//	public bool IsActive{
//		get{
//			return this.isActive;
//		}
//		set{
//			isActive = value;
//		}
//	}
//	public bool DoingAnimation {
//		get {
//			return this.doingAnimation;
//		}
//	}
}
