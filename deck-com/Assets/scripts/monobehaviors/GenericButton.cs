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

	//public bool doNotForceMouseIsOver;

	// Use this for initialization
	void Start () {
		mouseIsOver = false;
	}
	
	// Update is called once per frame
	void Update () {
		frameRend.color = mouseIsOver ? mouseOverColor : normalColor;

		mouseIsOver = false;	//this will be overwritten by onMouseOver if they are on the button
	}

	void OnMouseOver(){
		mouseIsOver = true;
	}

	void OnMouseDown(){
		target.SendMessage (message);
		mouseIsOver = false;
	}

//	void OnMouseEnter(){
//		mouseIsOver = true;
//	}
//	void OnMouseExit(){
//		mouseIsOver = false;
//	}



}
