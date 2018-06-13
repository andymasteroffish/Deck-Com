using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour {

	public GameObject target;
	public string message;

	private bool mouseIsOver;

	public SpriteRenderer frameRend;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);
	public Color disabledColor = new Color (0.75f, 0.75f, 0.75f, 0.4f);

	//some dumb placeholder effects to make UI more obvious 
	public bool pulseWhenPlayerExausted;
	public bool pulseAllTheTime;

	//turning the button off
	public bool isDisabled;
	public Text text; 


	// Use this for initialization
	void Start () {
		mouseIsOver = false;
	}
	
	// Update is called once per frame
	void Update () {
		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;
		if (text != null) {
			text.color = Color.black;
		}

		bool isPulsing = pulseAllTheTime;
		if (pulseWhenPlayerExausted) {
			isPulsing = GameManagerTacticsInterface.instance.gm.areAllPlayerUnitsExausted ();
		}
		if (isPulsing) {
			float newScale =  1.1f + Mathf.Sin(Time.time*2) * 0.25f;
			transform.localScale = new Vector3(newScale, newScale, 1);
		}

		if (isDisabled) {
			text.color = disabledColor;
			frameRend.color = disabledColor;
		}

		mouseIsOver = false;	//this will be overwritten by onMouseOver if they are on the button
	}

	void OnMouseOver(){
		if (!isDisabled) {
			mouseIsOver = true;
		}
	}

	void OnMouseDown(){
		if (!isDisabled) {
			target.SendMessage (message);
			mouseIsOver = false;
		}
	}



}
