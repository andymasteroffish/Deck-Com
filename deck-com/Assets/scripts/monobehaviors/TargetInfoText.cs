using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfoText : MonoBehaviour {

	public Camera gameCam;
	public Text textField;

	private Unit targetUnit;

	public RectTransform rectTrans;
	
	// Update is called once per frame
	void Update () {
		//match the mouse position
		Vector3 v3 = Input.mousePosition;
		v3.z = 1f;
		v3 = Camera.main.ScreenToWorldPoint(v3);

		rectTrans.position = v3;
		//transform.position = v3;
	}

	public void turnOn(string text, Unit _targetUnit){
		targetUnit = _targetUnit;
		textField.text = text;
		gameObject.SetActive (true);
	}

	public void turnOff(){
		gameObject.SetActive (false);
	}

	public void unitRollOff(Unit unit){
		if (unit == targetUnit){
			turnOff ();
		}
	}
}
