using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGO : MonoBehaviour {

	public SpriteRenderer spriteRend;

	public float spinSpeed;
	public float pulseSpeed, pulseRange;

	private bool isActive;
	private bool doingAnimation;

	public float growTime;

	public void activate(TilePos targetPos, Color col){
		gameObject.SetActive (true);
		isActive = true;
		transform.position = targetPos.getV3 ();
		spriteRend.color = col;
		transform.localScale = Vector3.zero;
		StartCoroutine (doScaleAnim (1, false, growTime));
	}

	public void deactivate(){
		isActive = false;
		StartCoroutine (doScaleAnim (0, true, growTime));
	}
	
	// Update is called once per frame
	void Update () {

		transform.localEulerAngles = new Vector3 (0, 0, Time.time * spinSpeed);

		float scalePrc = (Time.time * pulseSpeed) % 1.0f;
		float scale = (1.0f + pulseRange) - scalePrc * (pulseRange * 2);
		if (!doingAnimation) {
			transform.localScale = new Vector3 (scale, scale, scale);
		}
		
	}

	IEnumerator doScaleAnim(float endScale, bool turnOffAfter, float time){
		doingAnimation = true;
		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;
		float timer = 0;
		float startScale = transform.localScale.x;
		while (timer < time) {
			timer += Time.deltaTime;
			float prc = timer / time;
			float thisScale = 1.0f - prc * startScale + prc * endScale;
			transform.localScale = new Vector3 (thisScale, thisScale, thisScale);
			yield return null;
		}
		doingAnimation = false;
		if (turnOffAfter) {
			gameObject.SetActive (false);
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

	public bool DoingAnimation{
		get{
			return this.doingAnimation;
		}
	}
}
