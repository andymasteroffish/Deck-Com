using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressionIcon : MonoBehaviour {

	private SpriteRenderer spriteRend;

	public Sprite completedSprite;

	private bool isCurrent;

	public float pulseScale;
	public float pulseSpeed;


	public void setup(Color col, bool complete, bool current){
		spriteRend = GetComponent<SpriteRenderer> ();
		if (complete) {
			spriteRend.sprite = completedSprite;
		}

		spriteRend.color = col;

		isCurrent = current;
	}
	
	// Update is called once per frame
	void Update () {

		if (isCurrent) {
			float scale = 1f + pulseScale + Mathf.Sin (Time.time * pulseSpeed) * pulseScale;
			transform.localScale = new Vector3 (scale, scale, 1);
		}
		
	}
}
