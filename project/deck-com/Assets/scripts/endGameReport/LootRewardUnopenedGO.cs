using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootRewardUnopenedGO : MonoBehaviour {

	private LootReward reward;
	private EndGameReportInterface manager;
	private int order;

	public GameObject openLootPrefab;

	private bool isActive;

	private Vector3 centerPos;

	private Vector3 waitBasePos;
	public Vector3 waitOffset;

	private bool doingAnimation;
	public float slideTime, shakeTime, shrinkTime;
	public float pauseTimeBetweenAnimSteps;
	public float bigScale;

	private bool skipAnimationFlag;

	public void setup(LootReward _reward, int _order, EndGameReportInterface _manager){
		reward = _reward;
		order = _order;
		manager = _manager;
		isActive = true;
		doingAnimation = false;
		waitBasePos = GameObject.Find ("lootWaitPos").transform.position;
		centerPos = GameObject.Find ("lootCenterPos").transform.position;
		skipAnimationFlag = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (!doingAnimation) {
			int curOffsetLevel = order - manager.NextRewardToShow;
			transform.position = waitBasePos + waitOffset * curOffsetLevel;

			float angle = (1 - Mathf.PerlinNoise (Time.time, order) * 2) * 30;
			transform.localEulerAngles = new Vector3 (0, 0, angle);
		}

	}

	public void startAnimation(){
		StartCoroutine(doAnimation());
	}

	public void skipAnimation(){
		skipAnimationFlag = true;
	}

	IEnumerator doAnimation(){
		doingAnimation = true;
		transform.localEulerAngles = new Vector3 (0, 0, 0);

		float timer = 0;

		//move to the center
		Vector3 startPos = transform.position;
		float startScale = transform.localScale.x;
		while (timer < slideTime && !skipAnimationFlag) {
			timer += Time.deltaTime;
			float prc = timer / slideTime;
			transform.position = Vector3.Lerp (startPos, centerPos, prc);
			transform.localScale = new Vector3 (1, 1, 1) * Mathf.Lerp (startScale, bigScale, prc);
			yield return null;
		}

		if (!skipAnimationFlag) {
			yield return new WaitForSeconds (pauseTimeBetweenAnimSteps);
		}

		//shake a bit
		float shakePhaseTime = shakeTime / 3;
		timer = 0;
		while (timer < shakeTime && !skipAnimationFlag) {
			timer += Time.deltaTime;

			float relativeTime = timer + shakePhaseTime / 2;
			int curPhase = Mathf.FloorToInt(relativeTime/shakePhaseTime);
			float curPhaseTime = relativeTime - curPhase * shakePhaseTime;

			float prc = (curPhaseTime / shakePhaseTime) * 2 - 1;
			if (curPhase % 2 == 0) {
				prc = prc *= -1;
			}

			float angle = 30 * prc;
			transform.localEulerAngles = new Vector3 (0, 0, angle);

			yield return null;
		}

		if (!skipAnimationFlag) {
			yield return new WaitForSeconds (pauseTimeBetweenAnimSteps);
		}

		//spawn the rewards
		if (reward.money > 0) {
			Instantiate (openLootPrefab, transform.position, Quaternion.identity).GetComponent<LootRewardOpenGO> ().setup (reward.money);
		}
		if (reward.cards.Count > 0) {
			for (int i = 0; i < reward.cards.Count; i++) {
				float prc = (float)i / (float)(reward.cards.Count - 1);
				Instantiate (openLootPrefab, transform.position, Quaternion.identity).GetComponent<LootRewardOpenGO> ().setup (reward.cards [i], prc);
			}
		}

		//shrink down
		timer = 0;
		startScale = transform.localScale.x;
		while (timer < slideTime && !skipAnimationFlag) {
			timer += Time.deltaTime;
			float prc = timer / slideTime;
			transform.localScale = new Vector3 (1, 1, 1) * Mathf.Lerp (startScale, 0, prc);
			yield return null;
		}

		//we're done!
		isActive = false;
		gameObject.SetActive (false);
		doingAnimation = false;

	}


	public bool DoingAnimation{
		get{
			return this.doingAnimation;
		}
	}

	public bool IsActive{
		get{
			return this.isActive;
		}
	}
}
