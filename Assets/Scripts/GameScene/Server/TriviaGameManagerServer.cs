using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class TriviaGameManagerServer : NetworkManager {

	private static int MAX_PLAYERS = 8;

	[Header("Game Settings")]

	public Dictionary<int, PlayerData> players = new Dictionary<int ,PlayerData>();

	public List<int> playerIds = new List<int>();

	public QuestionDataServer currentQuestionData;

	public TriviaGameManagerClient singlePlayerTest = null;

	public LobbyHudController lobbyHudController;

	public float questionAnswerTime = 10;
	public float resultsShowTime = 5;
	public float leaderboardShowTime = 5;
	public float screenSwitchTime = 1.25f;

	public int playersAnswered = 0;
	public int playerCount = 0;

	private bool gameRunning = false;
	private bool serverRunning = false;

	private QuestionDataClient currentQuestionDataClient = null;

	private void Start() {
		if(singlePlayerTest != null) {
			PlayerData playerData = new PlayerData(0, "Player", singlePlayerTest);

			playerData.triviaGameManagerClient.triviaGameManagerServer = this;
			playerData.triviaGameManagerClient.playerId = 0;

			players[0] = playerData;

			StartGameAsServer();
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

		PlayerData playerData = new PlayerData(players.Count, "Player", triviaPlayer.GetComponent<TriviaGameManagerClient>());

		playerData.triviaGameManagerClient.triviaGameManagerServer = this;

		int playerId = conn.connectionId;
		
		playerData.triviaGameManagerClient.playerId = playerId;

		if(currentQuestionDataClient != null) {
			playerData.chosenAnswerIndex = -1;

			playerData.triviaGameManagerClient.RpcChangeQuestion(currentQuestionDataClient);
		}

		players[playerId] = playerData;

		playerIds.Add(playerId);

		playerCount++;

		Debug.Log("Player " + conn.connectionId + " joined!");
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

		//print("Player left succeeded: " + players.Remove(players.Find(x => x.triviaGameManagerClient == player.gameObject.GetComponent<TriviaGameManagerClient>())).ToString());

	}

	public void StartGameAsClient() {
		if(!lobbyHudController.CheckInputValidity()) {
			Debug.Log("Invalid details.");
			// TODO: Return feedback why it didn't start.
			return;
		}

		serverBindAddress = lobbyHudController.ipAddress;
		networkPort = lobbyHudController.port;

		lobbyHudController.ToggleLobbyUI(false);

		gameRunning = true;

		StartClient();
	}

	public void StartGameAsServer() {
		if(!lobbyHudController.CheckInputValidity()) {
			Debug.Log("Invalid details.");
			// TODO: Return feedback why it didn't start.
			return;
		}

		lobbyHudController.ToggleLobbyUI(false);

		serverBindAddress = lobbyHudController.ipAddress;
		networkPort = lobbyHudController.port;

		StartHost();

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

	private void ResetToLobby() {
		serverRunning = false;
		gameRunning = false;

		lobbyHudController.ToggleLobbyUI(true);
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

			players[playerIndex].triviaGameManagerClient.RpcChangeQuestion(currentQuestionDataClient);
			players[playerIndex].chosenAnswerIndex = -1;
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
				currentPlayer.score += 1000;
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

			ChangeClientsScreens(0);

			yield return new WaitForSecondsRealtime(screenSwitchTime);

			float timeWaited = 0;

			while(timeWaited < questionAnswerTime) {
				timeWaited += Time.deltaTime;

				if(playersAnswered >= playerCount) {
					break;
				}

				yield return null;
			}

			ProcessAnswers();

			SendLeaderboardData();

			yield return new WaitForSecondsRealtime(resultsShowTime);
			
			ChangeClientsScreens(1);

			yield return new WaitForSeconds(screenSwitchTime);

			UpdateClientsLeaderboardPositons();
			
			yield return new WaitForSeconds(leaderboardShowTime);
		}
	}
}
