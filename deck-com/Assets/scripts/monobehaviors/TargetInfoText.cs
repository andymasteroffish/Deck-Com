using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetInfoText : MonoBehaviour {

	public Camera gameCam;
	public Text descriptionText;
	public Text damageText;

	private Unit targetUnit;

	public RectTransform rectTrans;

	public Vector3 offsetForAITurn;

	public Color noDamageBlinkCol;
	public float noDamageBlinkSpeed;
	private Color defaultTextColor;
	private bool doNoDamageBlink;

	void Start(){
		defaultTextColor = damageText.color;
	}

	
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

		damageText.color = defaultTextColor;
		if (doNoDamageBlink && Time.time%noDamageBlinkSpeed < noDamageBlinkSpeed/2.0f) {
			damageText.color = noDamageBlinkCol;
		}
	}

	public void turnOn(string text, int damage, Unit _targetUnit){
		targetUnit = _targetUnit;
		descriptionText.text = text;
		damageText.text = "DAMAGE: " + damage.ToString ();
		gameObject.SetActive (true);

		doNoDamageBlink = damage <= 0;
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
