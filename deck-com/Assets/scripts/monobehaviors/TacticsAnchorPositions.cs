using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TacticsAnchorPositions : MonoBehaviour {

	public Camera hudCam;

	public Vector2 cardOffsetPlayer, cardOffsetAI;

	public Vector2 charmOffset;
	public Vector2 aiCardRevealOffset;

	public Vector2 actionMarkerOffset, actionMarkerAIOffset;

	public Vector2 playerButtonsOffset;

	public Transform card_player_startPos;
	public Transform card_player_restPos;
	public Transform card_player_endPos;	//not set yet

	public Transform card_ai_startPos;
	public Transform card_ai_restPos;
	public Transform card_ai_endPos;		//not set yet
	public Transform card_ai_revealPos;

	public Transform charm_player_startPos, charm_player_endPos;

	public Transform charm_ai_startPos, charm_ai_endPos;

	public Transform actionMarker_player_startPos, actionMarker_ai_startPos;

	public Transform playerButtons;

	void Awake () {
		setAnchors ();
	}
	
	// Update is called once per frame
	void Update () {
		//this really only needs to happen if the screen changes size
		setAnchors();
	}

	public void setAnchors(){
		float camWidth = hudCam.orthographicSize * Screen.width / Screen.height;
		float camHeight = hudCam.orthographicSize;

		float camLeft = -camWidth;
		float camRight = camWidth;
		float camBottom = -camHeight;
		float camTop = camHeight;

		Debug.Log ("cam left " + camLeft);
		Debug.Log ("cam top " + camTop);


		card_player_restPos.position = new Vector3 (camLeft+cardOffsetPlayer.x, camBottom+cardOffsetPlayer.y, 0);
		card_player_startPos.position = new Vector3 (camLeft - 2.0f,	camBottom - cardOffsetPlayer.y, 0);	//off screen

		card_ai_restPos.position = new Vector3 (camLeft + cardOffsetAI.x, camTop - cardOffsetAI.y, 0);
		card_ai_startPos.position = new Vector3 (camLeft -2.0f, camTop - cardOffsetAI.y, 0);				//off screen

		card_ai_revealPos.position = new Vector3 (camLeft + aiCardRevealOffset.x, camTop + aiCardRevealOffset.y, 0);

		charm_player_startPos.position = new Vector3 (camRight + 4.0f, camBottom + charmOffset.y);	//off screen
		charm_player_endPos.position = new Vector3 (camRight + charmOffset.x, camBottom + charmOffset.y);

		charm_ai_startPos.position = new Vector3 (camRight + 4.0f, camTop - charmOffset.y);	//off screen
		charm_ai_endPos.position = new Vector3 (camRight + charmOffset.x, camTop - charmOffset.y);

		actionMarker_player_startPos.position = new Vector3 (camLeft + actionMarkerOffset.x, camBottom + actionMarkerOffset.y, 0);
		actionMarker_ai_startPos.position = new Vector3 (camLeft + actionMarkerAIOffset.x, camTop + actionMarkerAIOffset.y, 0);


		playerButtons.position = new Vector3 (camLeft + playerButtonsOffset.x, camTop + playerButtonsOffset.y);
	}


}
