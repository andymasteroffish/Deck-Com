using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndGameReportInterface : MonoBehaviour {

	public bool doingAnimation;

	public EndGameManager manager;

	// Use this for initialization
	void Start () {
		
		doingAnimation = false;

		List<Card_Loot> loot = EndGameInfoHolder.instance.getLoot ();

		manager = new EndGameManager (loot);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
