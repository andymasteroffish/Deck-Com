using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameDisplayFrame : MonoBehaviour {

	public bool isForPlayer;

	public Transform startPos, endPos;

	public float xeno;

	private GameManager gm;

	public Text text;

	// Use this for initialization
	void Start () {
		//startPos = GameObject.Find ("charm_ai_startPos").transform.position;
		//endPos = GameObject.Find ("charm_ai_endPos").transform.position;
		gm = GameManagerTacticsInterface.instance.gm;
	}


	
	// Update is called once per frame
	void Update () {

		Unit myActiveUnit = isForPlayer ? gm.activePlayerUnit : gm.activeAIUnit;

		//set the text and get ready to slide!
		Vector3 targetPos = transform.position;
		if (myActiveUnit != null) {
			text.text = myActiveUnit.unitName;
			targetPos.x = endPos.position.x;
		} else {
			text.text = "";
			targetPos.x = startPos.position.x;
		}

		transform.position = Vector3.Lerp (transform.position, targetPos, xeno);


	}

}
