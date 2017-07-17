using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class EndGameReportInterface : MonoBehaviour {

	private bool doingAnimation;

	public EndGameManager manager;

	private int nextRewardToShow;

	public GameObject rewardPrefab;
	private List<LootRewardUnopenedGO> rewardGOs = new List<LootRewardUnopenedGO>();

	private bool isDone;

	public Text buttonText;

	// Use this for initialization
	void Start () {
		
		doingAnimation = false;
		isDone = false;

		List<Card_Loot> loot = EndGameInfoHolder.instance.getLoot ();

		manager = new EndGameManager (loot);

		nextRewardToShow = 0;

		//make some prefabs
		for (int i = 0; i < manager.rewards.Count; i++) {
			LootRewardUnopenedGO lootGO = Instantiate (rewardPrefab).GetComponent<LootRewardUnopenedGO> ();
			lootGO.setup (manager.rewards [i], i, this);
			rewardGOs.Add (lootGO);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (nextRewardToShow >= rewardGOs.Count) {
			isDone = true;
			buttonText.text = "DONE";
		} else {
			if (!rewardGOs [nextRewardToShow].IsActive) {
				nextRewardToShow++;
			}
		}
	}

	public void continueHit(){
		if (isDone) {
			EndGameInfoHolder.instance.kill ();
			UnityEngine.SceneManagement.SceneManager.LoadScene ("deck_building");
		} else {
			if (rewardGOs [nextRewardToShow].DoingAnimation) {
				//skip the animation
				rewardGOs [nextRewardToShow].skipAnimation();
			} else {
				//clear any current rewards
				GameObject[] openRewards = GameObject.FindGameObjectsWithTag ("RewardItem");
				for (int i = 0; i < openRewards.Length; i++) {
					Destroy (openRewards [i]);
				}

				//and start the next batch
				rewardGOs [nextRewardToShow].startAnimation ();
			}
		}
	}

	public int NextRewardToShow {
		get {
			return this.nextRewardToShow;
		}
	}
}
