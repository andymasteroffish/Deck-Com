using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassTurnIcon : MonoBehaviour {

	SpriteRenderer spriteRend;

	public Vector3 offset;

	public float timeBeforeFade;
	public float fadeTime;
	public float maxScale;

	private float timer;
	float startScale;

	// Use this for initialization
	void Start () {
		spriteRend = GetComponent<SpriteRenderer> ();
		timer = 0;
		startScale = transform.localScale.x;
		transform.position += offset;
	}
	
	// Update is called once per frame
	void Update () {

		timer += Time.deltaTime;

		if (timer > timeBeforeFade) {
			float prc = (timer - timeBeforeFade) / fadeTime;
			spriteRend.color = new Color (1, 1, 1, 1.0f - prc);
			float scale = Mathf.Lerp (startScale, maxScale, prc);
			transform.localScale = new Vector3 (scale, scale, scale);
		}

		if (timer > timeBeforeFade + fadeTime) {
			Destroy (gameObject);
		}
		
		
	}
}
