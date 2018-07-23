using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine.Profiling;

public class GameManager {

	public enum TurnPhase{Player, AI, CleanUp};
	
	private CameraControl cam;	//THIS SHOULD NOT BE HERE. MOVE THIS TO AN INTERFACE CLASS

	public Board board;
	private PodPlacement podPlacement;


	public Unit activePlayerUnit;
	public Unit activeAIUnit;

	public Card activeCard;

	public TargetInfoText targetInfoText;

	//tracking rounds
	//private bool isPlayerTurn;
	private TurnPhase curPhase;
	private int turnNum;

	//finishing
	private bool gameIsOver;
	private bool playerWins;

	public bool hasStoreKey;

	//getting and setting info about player progress
	string playerDocPath;
	XmlDocument playerDoc;
	private int curLevelNum, curAreaNum;

	//pooling objects
	ObjectPooler objectPool;


	public GameManager(){
		podPlacement = new PodPlacement ( UnitManager.instance.foeNodes );
		objectPool = new ObjectPooler ();
	}

	public void setup (string[] debugSpawnList) {

		//get the player doc
		playerDocPath = Application.dataPath + "/external_data/player/player_info.xml";
		playerDoc = new XmlDocument();
		playerDoc.Load(playerDocPath);

		XmlNode infoNode = playerDoc.GetElementsByTagName("info")[0];
		curLevelNum = int.Parse(infoNode["cur_level"].InnerXml);

		if ( bool.Parse(infoNode["generate_new_seed"].InnerXml) ){
			Debug.Log("my seed: "+Random.seed);
			infoNode ["seed"].InnerXml = ((int)Random.Range(0,9999999)).ToString ();
			infoNode ["generate_new_seed"].InnerXml = "false";
		}

		Random.seed = int.Parse (infoNode ["seed"].InnerXml) + (curLevelNum * 100);



		curAreaNum = (curLevelNum / GameManagerTacticsInterface.instance.levelsPerArea) + 1;
		if (curLevelNum < 0) {
			curAreaNum = 0;
		}

		Debug.Log ("level: " + curLevelNum+" area "+curAreaNum);

		//setup the board
		board = new Board ();
		board.mainBoardSetup ();
		board.reset (curLevelNum, curAreaNum);

		//throw in the store key
		Tile storeKeyTile = board.GetUnoccupiedTileWithSpawnProperty (Tile.SpawnProperty.StoreKey);
		board.passiveObjects.Add(new StoreKey(storeKeyTile.Pos));

		if (!GameManagerTacticsInterface.instance.debugIgnoreStandardSpawns) {
			List<Unit> activePlayerUnits = UnitManager.instance.getActivePlayerUnits ();
			for (int i = 0; i < activePlayerUnits.Count; i++) {
				Tile spawnTile = board.GetUnoccupiedTileWithSpawnProperty (Tile.SpawnProperty.Player);
				activePlayerUnits [i].setup (this, board, spawnTile);
				board.units.Add (activePlayerUnits [i]);
				activePlayerUnits [i].reset ();
				activePlayerUnits [i].setVisibleTiles ();
			}
			board.updateVisible ();	//make sure we know what the units can see

			if (!GameManagerTacticsInterface.instance.intoTheBreachMode) {
				podPlacement.placeFoes (this, board, curLevelNum, curAreaNum);
			}else{
				TilePos originPos = podPlacement.getEmptyMidpointTileBetweenPlayerAndEnd (board, getPlayerUnits (), 0.8f, 5);
				podPlacement.makePod (this, board, board.getTileFromPos(originPos), curLevelNum * 4, curAreaNum);
				//and give us a reinforcement too
				podPlacement.checkIfWeNeedReinforcements(curLevelNum, curAreaNum, turnNum, board, getPlayerUnits());

			}
		} else {

			for (int i = 0; i < debugSpawnList.Length; i++) {
				Unit unit = UnitManager.instance.getUnitFromIdName (debugSpawnList [i]);
				Tile spawnTile = board.GetUnoccupiedTileWithSpawnProperty (unit.isPlayerControlled ? Tile.SpawnProperty.Player : Tile.SpawnProperty.Foe);
				if (spawnTile != null) {
					unit.setup (this, board, spawnTile);
					unit.reset ();
					board.units.Add (unit);
				}
			}

			List<Unit> foes = getAIUnits ();
			for (int i = 0; i < foes.Count; i++) {
				for (int k = 0; k < foes.Count; k++) {
					if (i != k) {
						foes [i].podmates.Add (foes [k]);
					}
				}
			}
			foes [0].isPodLeader = true;
		}


		turnNum = 0;
		gameIsOver = false;
		playerWins = false;
			
		hasStoreKey = false;

		cam = GameObject.Find ("World Cam").GetComponent<CameraControl> ();

		if (!GameManagerTacticsInterface.instance.intoTheBreachMode) {
			board.resetUnitsAndLoot (curAreaNum);
		}

		startPlayerTurn ();

	}

	//starting and ending turns
	public void startPlayerTurn(){
		Debug.Log ("start player turn");
		if (checkGameOver()) {
			endGame ();
			return;
		}

		turnNum++;

		//isPlayerTurn = true;
		curPhase = TurnPhase.Player;

		GameObjectManager.instance.newPlayerTurn ();

		List<Unit> unitsPlayer = getPlayerUnits ();
		foreach(Unit unit in unitsPlayer){
			unit.resetRound ();
		}
		setActivePlayerUnit (unitsPlayer [0]);

		if (activeAIUnit != null) {
			activeAIUnit.setActive (false);
			Debug.Log ("null yur dad");
			activeAIUnit = null;
		}

		clearActiveCard ();

		//any passive object effects?
		for (int i = board.passiveObjects.Count-1; i>=0; i--) {
			board.passiveObjects [i].playerTurnStart();
		}
	}

	void startAITurn(){
		curPhase = TurnPhase.AI;
		//isPlayerTurn = false;

		if (checkGameOver()) {
			endGame ();
			return;
		} else {
			//start the AI
			List<Unit> unitsAI = getAIUnits ();
			foreach (Unit unit in unitsAI) {
				unit.resetRound ();
			}
			int startID = (int)Random.Range (0, unitsAI.Count);
			if (GameManagerTacticsInterface.instance.debugDoNotShuffle) {
				startID = 0;//unitsAI.Count-1;
			}
			if (unitsAI.Count > 0) {
				activeAIUnit = unitsAI [startID];
			} else {
				activeAIUnit = null;
			}
			tabActiveAIUnit (0);
		}

		clearActiveCard ();

		//any passive object effects?
		for (int i = board.passiveObjects.Count-1; i>=0; i--) {
			board.passiveObjects [i].AITurnStart();
		}
	}


	public void startCleanupPhase(){
		Debug.Log ("go clean up on turn "+turnNum);
		curPhase = TurnPhase.CleanUp;

		//check if we should flash in some reinforcements next turn
		if (!GameManagerTacticsInterface.instance.debugIgnoreStandardSpawns) {
			podPlacement.checkIfWeNeedReinforcements (curLevelNum, curAreaNum, turnNum, board, getPlayerUnits ());
		}
	}

	public void markAIStart(){
		//Debug.Log ("reset all AI flags");
		board.refreshAllyAndEnemyListsForBoardCompare(true);
		foreach (Unit unit in board.units) {
			unit.markAIStart ();
		}
	}
	public void markAIEnd(){
		foreach (Unit unit in board.units) {
			unit.markAIEnd ();
		}
	}

	public Tile spawnReinforcementAtNextMarker(){
		if (GameManagerTacticsInterface.instance.intoTheBreachMode) {
			for (int i=0; i<board.passiveObjects.Count; i++){
				PassiveObject passive = board.passiveObjects [i];
				if (passive.type == PassiveObject.PassiveObjectType.ReinforcementMarker) {
					ReinforcementMarker thisMarker = (ReinforcementMarker)passive;
					thisMarker.isDone = true;
					Tile originTile = board.getTileFromPos (thisMarker.CurTilePos);
					podPlacement.makePod (this, board, originTile, thisMarker.challengeRating, curAreaNum);
					//manually remove the marker
					board.passiveObjects.RemoveAt(i);
					return originTile;	//just one at a time
				}
			}
		}

		return null;
	}
	public ReinforcementMarker getNextReinforcementMarker(){
		if (GameManagerTacticsInterface.instance.intoTheBreachMode) {
			foreach (PassiveObject passive in board.passiveObjects) {
				if (passive.type == PassiveObject.PassiveObjectType.ReinforcementMarker) {
					return (ReinforcementMarker)passive;	
				}
			}
		}

		return null;
	}

	//checking input
	public void checkClick(){
		//see if they clicked a card to play	
		//if (activeCard == null) {					//I was only doing this if they were not in the process of playing one, but I want the user to be able to cancel by clicking a different card
			activePlayerUnit.checkActiveClick ();
		//}
		//check potential targets
		board.checkClick ();

	}

	public void endPlayerTurn(){
		Debug.Log ("end player turn");
		clearActiveCard ();
		List<Unit> unitsPlayer = getPlayerUnits ();
		for(int i=unitsPlayer.Count-1; i>=0; i--){
			unitsPlayer[i].endTurn ();
			//testing
			//unitsPlayer[i].deck.printDeck();
		}
		//check if anything (including foes) has clean up effects
		for (int i = board.units.Count-1; i >= 0; i--) {
			board.units [i].turnEndCleanUp ();
		}

		startAITurn ();
	}

	public void advanceAITurn(){
		Debug.Log ("do ai turn step " + activeAIUnit.curAITurnStep);
		board.resolveMove (activeAIUnit.aiTurnInfo.moves [activeAIUnit.curAITurnStep]);
		board.updateVisible ();
		activeAIUnit.curAITurnStep++;
		Debug.Log ("advance");
	}
	public void endAITurn(){
		activeAIUnit.endTurn ();
		//check if anything has clean up effects
		for (int i = board.units.Count-1; i >= 0; i--) {
			board.units [i].turnEndCleanUp ();
		}
		//go to the next one
		tabActiveAIUnit(1);
	}

	public void cancel(){
		activePlayerUnit.deck.cancel ();
	}

	//Input for cards that are about to be played
	public void tileClicked(Tile tile){
		if (activeCard != null) {
			activeCard.passInTile (tile);
		}
	}
	public void unitClicked(Unit unit){
		if (activeCard != null) {
			activeCard.passInUnit (unit);
		}
	}

	public void setCardActive(Card newCard){
		activeCard = newCard;
	}

	public void clearActiveCard(){
		if (activeCard != null){
			activeCard = null;
			board.clearHighlights ();
		}
	}

	//switching units

	public void setActivePlayerUnit(Unit newActive){
//		if (newActive == activePlayerUnit) {
//			return;
//		}
		activePlayerUnit = newActive;
		List<Unit> playerUnits = getPlayerUnits ();
		foreach (Unit unit in playerUnits){
			unit.setActive ( unit==activePlayerUnit);
			if (!unit.IsActive) {
				unit.deck.cancel ();
			}
		}
		cam.setTarget (newActive);
	}

	public void setActiveAIUnit(Unit newActive, bool startTurn){
		activeAIUnit = newActive;
		List<Unit> aiUnits = getAIUnits ();
		foreach (Unit unit in aiUnits){
			unit.setActive ( unit==activeAIUnit);
		}
		if ( (newActive.getIsVisibleToPlayer () || GameManagerTacticsInterface.instance.intoTheBreachMode) && curPhase == TurnPhase.AI) {
			cam.setTarget (newActive);
		}
		if (startTurn) {
			GameManagerTacticsInterface.instance.startNewAIUnitTurn ();
		}
	}

	public void tab(int dir){
		clearActiveCard ();
		tabActivePlayerUnit (dir);
		GameManagerTacticsInterface.instance.cancelTabAfterAnimations();
	}

	public void tabActivePlayerUnitSkippingExausted(int dir){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activePlayerUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		idNum = (idNum + dir + unitsPlayer.Count) % unitsPlayer.Count;
		while (unitsPlayer [idNum].ActionsLeft == 0 && count < unitsPlayer.Count + 1) {
			idNum = (idNum + 1) % unitsPlayer.Count;
			count++;
		}

		setActivePlayerUnit (unitsPlayer [idNum]);
		GameManagerTacticsInterface.instance.cancelTabAfterAnimations();
	}

	public void tabActivePlayerUnit(int dir){
		List<Unit> unitsPlayer = getPlayerUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsPlayer.Count; i++) {
			if (unitsPlayer [i] == activePlayerUnit) {
				idNum = i;
			}
		}
		int newIdNum = (idNum + dir + unitsPlayer.Count) % unitsPlayer.Count;
		setActivePlayerUnit (unitsPlayer [newIdNum]);
	}

	public void tabActiveAIUnit(int dir){
		List<Unit> unitsAI = getAIUnits ();
		int idNum = -1;
		for (int i = 0; i < unitsAI.Count; i++) {
			if (unitsAI [i] == activeAIUnit) {
				idNum = i;
			}
		}

		//find the next unit who's turn is not over
		int count = 0;
		if (unitsAI.Count > 0) {
			idNum = (idNum + dir + unitsAI.Count) % unitsAI.Count;
			while (unitsAI [idNum].isExausted && count < unitsAI.Count + 1) {
				idNum = (idNum + 1) % unitsAI.Count;
				count++;
			}
		} else {
			count = 99999;
		}

		//if we found one, great, otherwise, go to the next turn
		if (count <= unitsAI.Count) {
			setActiveAIUnit (unitsAI [idNum], true);
		} else {
			Debug.Log ("im so done");
			activeAIUnit = null;

		}
	}



	public void doUserSidePostCardPlayActions(){
		//make sure the info box is off
		targetInfoText.turnOff ();

		//does that change anything for the active unit
		if (activePlayerUnit != null) {
			activePlayerUnit.deck.updateCardsDisabled ();
		}
		if (activeAIUnit != null) {
			activeAIUnit.deck.updateCardsDisabled ();
		}
	}

	public bool checkGameOver(){
		if (!GameManagerTacticsInterface.instance.intoTheBreachMode) {
			List<Unit> unitsAI = getAIUnits ();
			if (unitsAI.Count == 0) {
				playerWins = true;
				return true;
			}
		}

		List<Unit> unitsPlayer = getPlayerUnits ();
		if (unitsPlayer.Count == 0) {
			playerWins = false;
			return true;
		}

		bool allOnEnd = true;
		foreach (Unit unit in getPlayerUnits()) {
			if (unit.CurTile.spawnProperty != Tile.SpawnProperty.Exit) {
				allOnEnd = false;
			}
		}
		if (allOnEnd) {
			playerWins = true;
			return true;
		}

		return false;
	}

	public void endGame(){
		Debug.Log ("END GAME");
		gameIsOver = true;


		List <Unit> playerUnits = getPlayerUnits ();

		List<Card_Loot> loot = new List<Card_Loot> ();
		for (int i = 0; i < playerUnits.Count; i++) {
			playerUnits [i].endGame ();
			List<Card_Loot> unitLoot = playerUnits [i].syphonLoot ();
			foreach (Card_Loot lootCard in unitLoot) {
				loot.Add (lootCard);
			}
		}

		//save the decks
		for (int i = 0; i < playerUnits.Count; i++) {
			playerUnits [i].saveDeckFile ();
		}

		//increase the level
		if (playerWins) {
			curLevelNum++;
			Debug.Log ("graduate to level " + curLevelNum);
			XmlNode infoNode = playerDoc.GetElementsByTagName ("info") [0];
			int curMoney = int.Parse (infoNode ["cur_level"].InnerXml);
			infoNode ["cur_level"].InnerXml = curLevelNum.ToString ();
			Debug.Log ("seed: " + infoNode ["seed"].InnerXml);
		}

		//save
		playerDoc.Save(playerDocPath);

		//save whatever info needs to be passed to the next scene
		EndGameInfoHolder.instance.lootList = loot;
		EndGameInfoHolder.instance.hasStoreKey = hasStoreKey;
	}

	//AI shit
	public TurnInfo getAIMove(int unitID, Board curBoard, Board originalBoard, int curDepth){
		if(curDepth == 0)	Profiler.BeginSample("AI Thinking");
		float startTime = Time.realtimeSinceStartup;
		if (GameManagerTacticsInterface.instance.debugPrintAIInfo && curDepth == 0) {
			Board.debugBoardCount = 0;
			Board.debugCounter = 0;
		}

		List<MoveInfo> allMoves = curBoard.getAllMovesForUnit (unitID);

		if (allMoves.Count == 0) {
			//Debug.Log ("null at depth " + curDepth);
			return null;
		}

		//find all possible moves that could follow
		List<TurnInfo> potentialTurns = new List<TurnInfo>();

		foreach (MoveInfo move in allMoves) {
			TurnInfo turn = new TurnInfo (move);
			//generate a board with this move
			Board newBoard = curBoard.resolveMoveAndReturnResultingBoard (move);

			//find all of the moves the AI could make from there
			TurnInfo followingMoves = null;
			if (!newBoard.units [unitID].isDead && !move.passMove) {
				followingMoves = getAIMove (unitID, newBoard, originalBoard, curDepth + 1);
			} else {
				//Debug.Log ("I'm dead lol");
			}

			//if there were move moves, add them to the turn
			if (followingMoves != null) {
				turn.addMoves (followingMoves);
			} else {
				//if there were no further moves, this is the end of this set and we should evaluate the board
				newBoard.compareBoardSates (originalBoard, newBoard.units [unitID], ref turn, false);
			}

			//return the board to the pool
			if (ObjectPooler.instance.checkIfBoardHasBeenRetired (newBoard)) {
				Debug.Log ("huh oh fuck");
			}
			newBoard.returnToPool ();	//PUT THIS BACK IN A WAY THAT DOES NOT BREAK debugResultingBoard

			potentialTurns.Add (turn);
		}

		//find the best one
		float bestValue = -9999;
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].val > bestValue) {
				bestValue = potentialTurns [i].val;
			}
		}
		//get a list of all moves with that value
		List<TurnInfo> goodTurns = new List<TurnInfo>();
		for (int i = 0; i < potentialTurns.Count; i++) {
			if (potentialTurns [i].val == bestValue) {
				goodTurns.Add(potentialTurns [i]);
			}
		}

		//select one at random to return
		TurnInfo returnVal = goodTurns[ (int)Random.Range(0,goodTurns.Count) ];
		if (GameManagerTacticsInterface.instance.debugDoNotShuffle) {
			returnVal = goodTurns [0];
		}


		//if just passing the turn is in the list, do that (otherwise they sometimes just attack and then heal themselves or heal a player unit at full health. Dumb stuff)
		foreach (TurnInfo turn in goodTurns) {
			if (turn.moves [turn.moves.Count-1].passMove == true){
				returnVal = turn;
			}
		}

		if (curDepth == 0) {
			Debug.Log ("---AI THINKING---");
			Debug.Log ("it took " + (Time.realtimeSinceStartup - startTime) + " seconds and " + Board.debugBoardCount + " boards to generate move");
			Debug.Log ("on frame " + (Time.frameCount-1));
			Debug.Log ("debug counter val: " + Board.debugCounter);
			returnVal.print (board);

		}
		//print info if we should
		if (GameManagerTacticsInterface.instance.debugPrintAIInfo && curDepth == 0) {
			Profiler.BeginSample ("print AI info");
			//in order to see what the hell the board evaluation is doing, we'll do one more but have it print info as it goes
			Debug.Log ("------TEST-------");
			//TurnInfo temp = new TurnInfo (new MoveInfo(unitID));

			Board tempBoard = ObjectPooler.instance.getBoard();
			tempBoard.setFromParent(originalBoard);

			foreach (MoveInfo move in returnVal.moves) {
				tempBoard.resolveMove (move);
			}
			tempBoard.compareBoardSates (originalBoard, curBoard.units [unitID], ref returnVal, true);
			Profiler.EndSample();
		}


		if(curDepth == 0)	Profiler.EndSample();
		return returnVal;
	}


	//unit tools
	public List<Unit> getAIUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in board.units){
			if (!unit.isPlayerControlled && !unit.isDead) {
				aiUnits.Add (unit);
			}
		}
		return aiUnits;
	}
	public List<Unit> getPlayerUnits(){
		List<Unit> aiUnits = new List<Unit> ();
		foreach (Unit unit in board.units){
			if (unit.isPlayerControlled && !unit.isDead) {
				aiUnits.Add (unit);
			}
		}
		return aiUnits;
	}

	public bool areAllPlayerUnitsExausted(){
		bool isExausted = true;
		List<Unit> playerUnits = getPlayerUnits ();
		foreach (Unit unit in playerUnits) {
			if (!unit.isExausted) {
				isExausted = false;
			}
		}
		return isExausted;
	}

	//checking for things
	public bool areAnimationsHappening(){
//		foreach(Unit unit in units){
//			if (unit.areAnimationsHappening()){
//				return true;
//			}
//		}

		return false;
	}

	//setters and getters

//	public bool IsPlayerTurn {
//		get {
//			return this.isPlayerTurn;
//		}
//	}
	public TurnPhase CurPhase{
		get {
			return this.curPhase;
		}
	}

	public bool GameIsOver{
		get{
			return this.gameIsOver;
		}
	}

	public int CurAreaNum {
		get {
			if (this.curAreaNum < 0) {
				return 0;
			}
			return this.curAreaNum;
		}
	}

	public bool PlayerWins {
		get {
			return this.playerWins;
		}
	}

	public int TurnNum{
		get{
			return this.turnNum;
		}
	}
}
