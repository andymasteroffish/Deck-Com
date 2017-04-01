using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfoText : MonoBehaviour {

	public Camera gameCam;
	public Text textField;

	private Unit targetUnit;

	public RectTransform rectTrans;

	public Vector3 offsetForAITurn;

	
	// Update is called once per frame
	void Update () {
		//match the mouse position on the player turn
		if (GameManagerTacticsInterface.instance.gm.IsPlayerTurn) {
			Vector3 v3 = Input.mousePosition;
			v3.z = 1f;
			v3 = Camera.main.ScreenToWorldPoint (v3);

			rectTrans.position = v3;
		}
		//and just match the target on the AI turn
		else {
			rectTrans.position = targetUnit.CurTile.Pos.getV3 () + offsetForAITurn;
		}
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
		if (unit == targetUnit && GameManagerTacticsInterface.instance.gm.IsPlayerTurn){
			turnOff ();
		}
	}
}
