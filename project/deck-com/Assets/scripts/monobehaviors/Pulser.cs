using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulser : MonoBehaviour {

	private SpriteRenderer spriteRend;

	private float startScale;
	public float scaleSpeed, scaleRange;

	public float colorSpeed;
	private Color startColor;
	public Color altColor;


	// Use this for initialization
	void Start () {
		spriteRend = GetComponent<SpriteRenderer>();
		startScale = transform.localScale.x;
		startColor = spriteRend.color;
	}
	
	// Update is called once per frame
	void Update () {

		float thisScale = startScale + Mathf.Sin(Time.time*scaleSpeed)*scaleRange;
		transform.localScale = new Vector3(thisScale, thisScale, thisScale);

		Color thisCol = Color.Lerp(startColor, altColor, Mathf.Abs(Mathf.Sin(Time.time*colorSpeed)));
		spriteRend.color = thisCol;
		
	}
}
