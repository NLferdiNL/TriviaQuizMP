using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class TriviaGameManagerServer : NetworkManager {

	public static int MAX_CHARACTERS_NAME = 13;

	private static int BASE_SCORE = 50;

	private static int TIMED_SCORE_ADDITION = 50;

	private static int SCORE_MULTIPLIER = 10;

	private static int MAX_PLAYERS = 8;

	[Header("Game Settings")]

	public Dictionary<int, PlayerData> players = new Dictionary<int ,PlayerData>();

	public List<int> playerIds = new List<int>();

	public QuestionDataServer currentQuestionData;

	public TriviaGameManagerClient singlePlayerTest = null;

	public LobbyHudController lobbyHudController;

	public float questionAnswerTime = 10;
	public float resultsShowTime = 5;
	public float leaderboardShowTime = 3;
	public float screenSwitchTime = 0.75f;

	public bool acceptingAnswers = false;

	public int playersAnswered = 0;
	public int playerCount = 0;

	public float timeWaited = 0;

	private bool gameRunning = false;
	private bool serverRunning = false;

	private QuestionDataClient currentQuestionDataClient = null;

	private void Start() {
		if(singlePlayerTest != null) {
			PlayerData playerData = new PlayerData(0, "Player", singlePlayerTest);

			playerData.triviaGameManagerClient.triviaGameManagerServer = this;
			playerData.triviaGameManagerClient.playerId = 0;

			players[0] = playerData;

			serverRunning = true;

			StartGame();
		}
	}
	
	public override void OnStartHost() {
		base.OnStartHost();

		Debug.Log("Server started!");

		serverRunning = true;
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		base.OnServerAddPlayer(conn, playerControllerId);

		GameObject triviaPlayer = conn.playerControllers[0].gameObject;

		TriviaGameManagerClient clientManager = triviaPlayer.GetComponent<TriviaGameManagerClient>();

		clientManager.RpcGetUsername();
		
		PlayerData playerData = new PlayerData(players.Count, "Player", clientManager);

		playerData.triviaGameManagerClient.triviaGameManagerServer = this;

		int playerId = conn.connectionId;
		
		playerData.triviaGameManagerClient.playerId = playerId;

		if(currentQuestionDataClient != null) {
			SendQuestionToClient(playerData, currentQuestionDataClient);
		}

		players[playerId] = playerData;

		playerIds.Add(playerId);

		playerCount++;

		if(gameRunning) {
			playerData.triviaGameManagerClient.RpcLobbyChangeState(ScreenState.Closed);
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn) {
		base.OnServerDisconnect(conn);

		int playerIndex = conn.connectionId;

		PlayerData playerData = players[playerIndex];
		GameObject playerRemains = players[playerIndex].triviaGameManagerClient.gameObject;

		Debug.Log("Player " + conn.connectionId + " left!");

		players.Remove(playerIndex);
		playerIds.Remove(playerIndex);

		Destroy(playerRemains);

		playerCount--;

		SendLobbyPlayerlist();
	}

	public void JoinServer() {
		if(!lobbyHudController.CheckInputValidity()) {
			Debug.Log("Invalid details.");
			// TODO: Return feedback why it didn't start.
			return;
		}

		networkAddress = lobbyHudController.ipAddress;
		
		networkPort = lobbyHudController.port;

		lobbyHudController.ToggleScreen(ScreenState.Lobby);

		gameRunning = true;

		StartClient();
	}

	new public void StartServer() {
		if(!lobbyHudController.CheckInputValidity()) {
			Debug.Log("Invalid details.");
			// TODO: Return feedback why it didn't start.
			return;
		}

		lobbyHudController.ToggleScreen(ScreenState.Lobby);
		lobbyHudController.ToggleServerStartButtonState(true);

		serverBindAddress = lobbyHudController.ipAddress;
		networkPort = lobbyHudController.port;

		StartHost();
	}

	new public void StopHost() {
		if(serverRunning) {
			base.StopHost();
		} else {
			StopClient();
		}

		players.Clear();
		playerIds.Clear();

		lobbyHudController.ToggleScreen(ScreenState.Menu);
	}

	public void StartGame() {
		if(!serverRunning) {
			return;
		}

		lobbyHudController.ToggleScreen(ScreenState.Closed);
		SendLobbyScreenStateToClients(ScreenState.Closed);

		gameRunning = true;

		StartCoroutine(GameLoop());
	}

	public override void OnStopClient() {
		base.OnStopClient();

		ResetToLobby();
	}

	public override void OnStopHost() {
		base.OnStopHost();

		ResetToLobby();
	}

	public void SendLobbyPlayerlist() {
		LeaderboardData[] leaderboardItems = GetLeaderboardItems();

		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcUpdatePlayerlistLobby(leaderboardItems);
		}
	}

	private void SendLobbyScreenStateToClients(ScreenState newState) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcLobbyChangeState(newState);
		}
	}

	private void SendQuestionToClient(PlayerData playerData, QuestionDataClient question) {
		playerData.chosenAnswerIndex = -1;

		playerData.triviaGameManagerClient.RpcChangeQuestion(question);
	}

	private void ResetToLobby() {
		serverRunning = false;
		gameRunning = false;

		lobbyHudController.ToggleScreen(ScreenState.Menu);
	}

	private IEnumerator SendQuestionToClients() {
		yield return QuestionReceiver.GetQuestion((returnedQuestionData) => {
			currentQuestionData = returnedQuestionData;
		});
		
		currentQuestionDataClient = new QuestionDataClient(currentQuestionData.question, currentQuestionData.shuffled_answers);

		playersAnswered = 0;

		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			SendQuestionToClient(players[playerIndex], currentQuestionDataClient);
		}
	}

	private void ClientsStartTimer(float time) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcStartTimer(time);
		}
	}

	private void ClientsSyncTimer(float time) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcSyncTimer(time);
		}
	}

	private void ClientsStopTimer() {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcStopTimer();
		}
	}

	private void ClientsResetTimer(float time) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcResetTimer(time);
		}
	}

	private void SendLeaderboardData() {
		LeaderboardData[] leaderboardItems = GetLeaderboardItems();

		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.leaderboardController.RpcSetLeaderboardData(leaderboardItems);
		}
	}

	private void UpdateClientsLeaderboardPositons() {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.leaderboardController.RpcUpdatePositions(false);	
		}
	}
	
	private void SetClientAnswerAcceptionState(bool newState) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.RpcChangeAnswerAcceptState(newState);
		}
	}

	private void ChangeClientsScreens(int newScreen) {
		for(int i = 0; i < playerIds.Count; i++) {
			if(players[playerIds[i]] == null) {
				continue;
			}

			int playerIndex = playerIds[i];

			players[playerIndex].triviaGameManagerClient.screenSwitchController.RpcSetScreen(newScreen);
		}
	}

	private void ProcessAnswers() {
		// TODO: Scores based on answer speed.
		for(int i = 0; i < players.Count; i++) {
			PlayerData currentPlayer = players[i];
			currentPlayer.triviaGameManagerClient.RpcDisplayCorrectAnswer(currentQuestionData.correct_answer_index);
			if(currentPlayer.chosenAnswerIndex == currentQuestionData.correct_answer_index) {
				currentPlayer.score += Mathf.RoundToInt(BASE_SCORE + TIMED_SCORE_ADDITION / (questionAnswerTime *  currentPlayer.timeTaken)) * SCORE_MULTIPLIER;
			}
		}
	}

	private LeaderboardData[] GetLeaderboardItems() {
		List<LeaderboardData> returnValue = new List<LeaderboardData>();

		for(int i = 0; i < players.Count; i++) {
			LeaderboardData currentPlayer = players[i].leaderboardData;

			returnValue.Add(currentPlayer);
		}

		return returnValue.ToArray();
	}

	private IEnumerator GameLoop() {

		while(gameRunning) {
			yield return SendQuestionToClients();

			yield return new WaitForSeconds(1);

			ChangeClientsScreens(0);

			yield return new WaitForSecondsRealtime(screenSwitchTime);

			acceptingAnswers = true;

			SetClientAnswerAcceptionState(true);

			timeWaited = 0;

			ClientsStartTimer(questionAnswerTime);

			float twoSecondsPassed = 0;

			while(timeWaited < questionAnswerTime) {
				timeWaited += Time.deltaTime;
				twoSecondsPassed += Time.deltaTime;

				if(playersAnswered >= playerCount) {
					break;
				}

				if(twoSecondsPassed >= 2) {
					twoSecondsPassed = 0;
					ClientsSyncTimer(questionAnswerTime - timeWaited);
				}

				yield return null;
			}

			ClientsStopTimer();

			acceptingAnswers = false;

			SetClientAnswerAcceptionState(false);

			ProcessAnswers();

			SendLeaderboardData();

			yield return new WaitForSecondsRealtime(resultsShowTime);
			
			ChangeClientsScreens(1);

			yield return new WaitForSeconds(screenSwitchTime);

			ClientsResetTimer(questionAnswerTime);

			UpdateClientsLeaderboardPositons();
			
			yield return new WaitForSeconds(leaderboardShowTime);
		}
	}
}
