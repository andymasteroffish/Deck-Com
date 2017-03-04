using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBCardGO : MonoBehaviour {

	private Card card;
	private int order;

	private bool needPosInfo = true;
	private Vector3 topLeftPoint;

	private Vector3 homePos;
	public Vector3 spacing;

	public int numCardsPerCol;

	public Text nameField;
	public Text descField;
	public SpriteRenderer spriteRend;

	private bool doingAnimation;

	private bool isActive;

	public Vector3 mouseOverAdjust;

	public float moveTime;

	private bool mouseIsOver;


	public void activate(Card _card, int _order){
		card = _card;
		order = _order;

		isActive = true;
		gameObject.SetActive (true);

		gameObject.name = "card "+card.name;

		int col = order / numCardsPerCol;
		int row = order % numCardsPerCol;

		if (needPosInfo) {
			needPosInfo = false;
			topLeftPoint = GameObject.Find ("cardTopLeft").transform.position;
		}
		homePos = topLeftPoint + new Vector3(spacing.x * col, spacing.y * row, spacing.z * row);

		transform.position = homePos;
		spriteRend.transform.localPosition = Vector3.zero;

		//set the text
		nameField.text = card.name;
		descField.text = card.description;

		mouseIsOver = false;
	}

	public void deactivate(){
		card = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "card unused";
	}
	
	// Update is called once per frame
	void Update () {



		//set the position (if we're not sliding it)
		if (!doingAnimation) {
			Vector3 spritePos = new Vector3 (0, 0, 0);

			if (mouseIsOver) {
				spritePos += mouseOverAdjust;
			}

			spriteRend.transform.localPosition = spritePos;
		}

		//time to die?
		if (DBManagerInterface.instance.manager.activeDeck == null) {
			deactivate ();
		}

	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}

	IEnumerator doMoveAnimation(Vector3 target, float time, bool deactivateWhenDone){
		doingAnimation = true;

		Vector3 startPos = transform.position;

		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;

		float timer = 0;

		while (timer < time) {
			timer += Time.deltaTime;
			float prc = Mathf.Clamp (timer / time, 0, 1);
			prc = Mathf.Pow (prc, 0.75f);
			transform.position = Vector3.Lerp (startPos, target, prc);
			yield return null;
		}

		doingAnimation = false;
		transform.position = target;

		if (deactivateWhenDone) {
			deactivate ();
		}
	}


	//setters and getters
	public bool IsActive{
		get{
			return this.isActive;
		}
		set{
			isActive = value;
		}
	}
	public bool DoingAnimation {
		get {
			return this.doingAnimation;
		}
	}
}
