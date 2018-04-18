using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressionMap : MonoBehaviour {

	public int numAreas;
	public int numLevelsPerArea;

	public Transform startPos;

	public GameObject levelMarkerPrefab;
	public float spacingX, areaSpacingX;

	public TextMesh nextLevelLext;

	public Color[] areaColors;

	public string[] areaNames;


	public void setup(int curLevel){

		//spawn 'em
		for (int i = -1; i < numAreas * numLevelsPerArea; i++) {
			int area = levelToArea (i);
			GameObject markerObj = Instantiate (levelMarkerPrefab, startPos.position + new Vector3(spacingX*i + areaSpacingX*area, 0,0), Quaternion.identity) as GameObject;
			markerObj.transform.parent = transform;
			markerObj.GetComponent<LevelProgressionIcon> ().setup (areaColors [area], i < curLevel, i == curLevel);
		}

		int levelNum = (curLevel % numLevelsPerArea) + 1;
		string levelNumText = " " + levelNum;
		if (levelToArea (curLevel) == 0) {
			levelNumText = "";				//no number for the intro
		}
		string nextLevelLabel = "Next Level: "+ areaNames[levelToArea(curLevel)] +levelNumText;
		nextLevelLext.text = nextLevelLabel;


	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private int levelToArea(int level){
		
		if (level < 0) {
			return 0;
		}

		return (level / numLevelsPerArea) + 1;
	}
}
