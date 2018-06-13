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

	public GameObject button;
	public Text buttonText;

	public TextMesh topText;

	// Use this for initialization
	void Start () {
		
		doingAnimation = false;
		isDone = false;

		List<Card_Loot> loot = EndGameInfoHolder.instance.getLoot ();

		manager = new EndGameManager (loot);
		manager.hasStoreKey = EndGameInfoHolder.instance.hasStoreKey;

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
		} else {
			if (!rewardGOs [nextRewardToShow].IsActive) {
				nextRewardToShow++;
			}
		}
	}

	public void selectLoot(LootRewardOpenGO lootGO){
		manager.selectLoot (lootGO.card, lootGO.moneyVal);
	}

	public void continueToNext(){

		//clear any current rewards
		GameObject[] openRewards = GameObject.FindGameObjectsWithTag ("RewardItem");
		for (int i = 0; i < openRewards.Length; i++) {
			Destroy (openRewards [i]);
		}

		if (!isDone) {
			rewardGOs [nextRewardToShow].startAnimation ();
		} else {
			buttonText.text = "GO TO DECK-BUILDING";
			button.gameObject.SetActive (true);
			topText.text = "That's it!";
		}
	}

	public void continueHit(){
		if (isDone) {
			manager.saveCards ();
			manager.saveMoney ();
			EndGameInfoHolder.instance.kill ();
			UnityEngine.SceneManagement.SceneManager.LoadScene ("deck_building");
		} 
		//the continue button opens the firts pack
		else {
			continueToNext ();
			button.gameObject.SetActive (false);
		}
	}

	public int NextRewardToShow {
		get {
			return this.nextRewardToShow;
		}
	}
}
