using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericButton : MonoBehaviour {

	public GameObject target;
	public string message;

	private bool mouseIsOver;

	public SpriteRenderer frameRend;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);

	//some dumb placeholder effects to make UI more obvious 
	public bool pulseWhenPlayerExausted;
	public bool pulseAllTheTime;


	// Use this for initialization
	void Start () {
		mouseIsOver = false;
	}
	
	// Update is called once per frame
	void Update () {
		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;

		bool isPulsing = pulseAllTheTime;
		if (pulseWhenPlayerExausted) {
			isPulsing = GameManagerTacticsInterface.instance.gm.areAllPlayerUnitsExausted ();
		}
		if (isPulsing) {
			float newScale =  1.1f + Mathf.Sin(Time.time*2) * 0.25f;
			transform.localScale = new Vector3(newScale, newScale, 1);
		}

		mouseIsOver = false;	//this will be overwritten by onMouseOver if they are on the button
	}

	void OnMouseOver(){
		mouseIsOver = true;
	}

	void OnMouseDown(){
		target.SendMessage (message);
		mouseIsOver = false;
	}



}
