using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DBUnusedCharmGO : MonoBehaviour {

	private Charm charm;
	private int order;

	public Text nameField;
	public Text descField;
	public SpriteRenderer spriteRend;


	private bool mouseIsOver;
	public Color normalColor = Color.white;
	public Color mouseOverColor = new Color (0.75f, 0.75f, 0.75f);

	private bool isActive;

	private bool needPosInfo = true;
	private Vector3 topLeftPoint;

	public Vector3 spacing;

	public int numCharmsPerCol;

	public void activate(Charm _charm, int _order){
		charm = _charm;
		order = _order;

		if (needPosInfo) {
			needPosInfo = false;
			topLeftPoint = GameObject.Find ("charmTopLeft").transform.position;
		}

		isActive = true;
		gameObject.SetActive (true);

		gameObject.name = "charm "+charm.name+" "+order;

		nameField.text = charm.name;
		descField.text = charm.description;

		mouseIsOver = false;

		int col = order / numCharmsPerCol;
		int row = order % numCharmsPerCol;

		transform.position = topLeftPoint + new Vector3(spacing.x * col, spacing.y * row, spacing.z * row);

	}

	public void deactivate(){
		charm = null;
		isActive = false;
		gameObject.SetActive (false);
		gameObject.name = "charm unused";
	}

	// Update is called once per frame
	void Update () {
		DBManager manager = DBManagerInterface.instance.manager;

		//time to die?
		if (manager.unusedWeaponsOpen == false) {
			deactivate ();
			return;
		}

		spriteRend.color = mouseIsOver ? mouseOverColor : normalColor;

		if (Input.GetMouseButtonDown (0) && mouseIsOver) {
			manager.replaceCharm(charm);
			mouseIsOver = false;
		}
		
	}

	void OnMouseEnter(){
		mouseIsOver = true;
	}
	void OnMouseExit(){
		mouseIsOver = false;
	}

	public bool IsActive{
		get{
			return this.isActive;
		}
	}
}
