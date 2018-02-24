using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMatcher : MonoBehaviour {

	private Camera thisCam;
	public Camera targetCam;

	// Use this for initialization
	void Start () {
		thisCam = GetComponent<Camera> ();
	}
	
	// Update is called once per frame
	void Update () {

		transform.position = targetCam.transform.position;
		thisCam.orthographicSize = targetCam.orthographicSize;
		
	}
}
