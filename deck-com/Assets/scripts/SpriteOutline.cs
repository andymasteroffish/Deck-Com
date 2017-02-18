using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOutline : MonoBehaviour {

	private SpriteRenderer parentSprite;
	private SpriteRenderer[] sprites;

	public void setup () {
		parentSprite = transform.parent.gameObject.GetComponent<SpriteRenderer> ();

		sprites = GetComponentsInChildren<SpriteRenderer> ();

		//do some basic setup
		for (int i = 0; i < sprites.Length; i++) {
			sprites [i].sprite = parentSprite.sprite;
			sprites [i].sortingOrder = parentSprite.sortingOrder - 1;
		}

		turnOff ();	//start with no outline
		
	}
	
	// Update is called once per frame
	void Update () {
		
		
	}

	public void turnOn(){
		turnOn (sprites [0].color);
	}
	public void turnOn(Color col){
		gameObject.SetActive (true);
		for (int i = 0; i < sprites.Length; i++) {
			sprites [i].color = col;
		}
	}

	public void turnOff(){
		gameObject.SetActive (false);
	}
}
