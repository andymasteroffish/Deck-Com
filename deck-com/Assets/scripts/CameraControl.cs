using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour {

	private bool freeControl;
	public float freeControlSpeed;

	private Unit targetUnit = null;
	public float targetLerp;


	public void setTarget(Unit target){
		targetUnit = target;
		freeControl = false;
	}

	void Update () {

		if (!freeControl && targetUnit != null) {
			Vector3 targetPos = targetUnit.transform.position;
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
	
	}
}
