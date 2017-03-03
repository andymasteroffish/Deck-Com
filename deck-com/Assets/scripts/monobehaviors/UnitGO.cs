using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitGO : MonoBehaviour {

	private Unit owner;

	public SpriteRenderer spriteRend;

	private bool doingAnimation;

	private bool isActive;

	public float moveTime;
	private Tile curTile;

	public SpriteOutline outline;
	public Text healthText;

	public void activate(Unit unit){
		owner = unit;

		isActive = true;
		gameObject.SetActive (true);

		curTile = owner.CurTile;
		transform.position = owner.CurTile.Pos.getV3 ();

		gameObject.name = "unit " + owner.unitName;

		spriteRend.sprite = unit.sprite;
		outline.setup ();
	}

	public void deactivate(){
		owner = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "unit unused";
	}

	// Update is called once per frame
	void Update () {

		if (!doingAnimation) {
			//bounce the sprite when it is active
			float spriteScale = 1;
			if (owner.IsActive && !owner.TurnIsDone) {
				spriteScale = 1.0f + Mathf.Abs (Mathf.Sin (Time.time * 2) * 0.1f);
			}
			spriteRend.gameObject.transform.localScale = new Vector3 (spriteScale, spriteScale, spriteScale);

			//greying when turn is done
			if (!owner.IsHighlighted) {
				Color colThisFrame = new Color (1, 1, 1, 1);
				if (owner.TurnIsDone && owner.GM.IsPlayerTurn == owner.isPlayerControlled) {
					colThisFrame = new Color (0.3f, 0.3f, 0.3f, 0.5f);
				}
				spriteRend.color = colThisFrame;
			}
		}

		//show health
		healthText.text = "HP: " + owner.health+"/"+owner.baseHealth;

		//keeping them raised up for clicking
		transform.position = new Vector3(transform.position.x, transform.position.y, -1f);

		//outline
		if (owner.IsHighlighted) {
			outline.turnOn (owner.highlightCol);
		} else {
			outline.turnOff ();
		}

		//are we dead?
		if (owner.isDead && !doingAnimation) {
			StartCoroutine (doDeathAnimation (moveTime));
		}

		//check if we need to move
		if (curTile != owner.CurTile && !doingAnimation) {
			curTile = owner.CurTile;
			StartCoroutine (doMoveAnimation (owner.CurTile.Pos.getV3 (), moveTime));
		}



	}


	void OnMouseEnter(){
		owner.mouseIsOver = true;
		if (owner.IsHighlighted) {
			owner.GM.activeCard.potentialTargetMouseOver (owner);
		}
	}
	void OnMouseExit(){
		owner.mouseIsOver = false;
		owner.GM.targetInfoText.unitRollOff(owner);
	}


	//animations
	IEnumerator doMoveAnimation(Vector3 target, float time){
		doingAnimation = true;

		time *= GameManagerTacticsInterface.instance.debugAnimationTimeMod;

		Vector3 startPos = transform.position;
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
	}

	IEnumerator doDeathAnimation(float time){
		doingAnimation = true;

		float timer = time * GameManagerTacticsInterface.instance.debugAnimationTimeMod;
		float startScale = transform.localScale.x;

		while (timer > 0) {
			timer -= Time.deltaTime;
			timer = Mathf.Max (0, timer);

			float newScale = timer / time;
			newScale = Mathf.Pow (newScale, 2);
			transform.localScale = new Vector3 (newScale, newScale, newScale);

			yield return null;
		}

		deactivate();

	}

	public bool areAnimationsHappening(){
		if (doingAnimation){
			return true;
		}

		//		if (deck.areAnimationsHappening()){
		//			return true;
		//		}

		return false;
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

	public Unit Owner{
		get{
			return this.owner;
		}
	}
}
