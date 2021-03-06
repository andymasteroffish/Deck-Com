﻿using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	private Camera cam;

	private bool freeControl;
	public float freeControlSpeed;

	private UnitGO targetUnit = null;

	private TilePos	targetTile;
	private bool usingTargetTile = false;

	public float targetLerp;

	public float zoomSpeed;
	public float minSize, maxSize;
	private float startSize;
	private float targetSize;


	private bool doingAIReveal;

	void Start(){
		cam = GetComponent<Camera> ();
		startSize = cam.orthographicSize;
		targetSize = startSize;

		doingAIReveal = false;
	}


	public void setTarget(Unit target, bool isAIReveal = false){
		if (doingAIReveal && !isAIReveal) {
			return;
		}
		targetUnit = GameObjectManager.instance.findUnitGO(target);
		usingTargetTile = false;
		freeControl = false;
	}

	public void setTarget(TilePos target, bool isAIReveal = false){
		if (doingAIReveal && !isAIReveal) {
			return;
		}
		targetUnit = null;
		usingTargetTile = true;
		targetTile = new TilePos (target);
		freeControl = false;
	}

	public void revealAIUnit(Unit newTarget){
		StartCoroutine(doRevealAIUnit(newTarget));
	}



	void Update () {


		if (!freeControl && targetUnit != null) {
			Vector3 targetPos = targetUnit.transform.position;
			targetPos.z = transform.position.z;
			transform.position = Vector3.Lerp (transform.position, targetPos, targetLerp);
		}
		if (!freeControl && usingTargetTile) {
			Vector3 targetPos = targetTile.getV3();
			targetPos.z = transform.position.z;
			transform.position = Vector3.Lerp (transform.position, targetPos, targetLerp);
		}

		Vector2 input = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
		if (input.magnitude > 0.5f) {
			freeControl = true;
		}

		if (freeControl) {
			transform.position += new Vector3 (input.x, input.y, 0) * freeControlSpeed * Time.deltaTime;
		}

		//zooming
		targetSize += Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime;
		targetSize = Mathf.Clamp (targetSize, minSize, maxSize);
		cam.orthographicSize = Mathf.Lerp (cam.orthographicSize, targetSize, targetLerp);

	
	}


	IEnumerator doRevealAIUnit(Unit newTarget){
		doingAIReveal = true;
		yield return new WaitForSeconds (0.5f);
		setTarget (newTarget, true);
		yield return new WaitForSeconds (2f);
		setTarget (GameManagerTacticsInterface.instance.gm.activePlayerUnit, true);
		doingAIReveal = false;
	}
}
