using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TriviaGameManagerServer : NetworkManager {

	[Header("Game Settings")]

	private List<PlayerData> players = new List<PlayerData>();

	public QuestionDataServer currentQuestionData;

	public TriviaGameManagerClient singlePlayerTest = null;

	public float questionAnswerTime = 10;
	public float resultsShowTime = 5;
	public float leaderboardShowTime = 5;
	public float screenSwitchTime = 1.25f;

	public bool gameRunning = false;

	private int playersAnswered = 0;

	private void Start() {
		if(singlePlayerTest != null) {
			PlayerData playerData = new PlayerData(0, "Player", singlePlayerTest);

			playerData.triviaGameManagerClient.triviaGameManagerServer = this;
			playerData.triviaGameManagerClient.playerId = 0;

			players.Add(playerData);

			StartGame();
		}
	}

	public override void OnStartServer() {
		base.OnStartServer();

		gameRunning = true;

		StartGame();
	}

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId) {
		base.OnServerAddPlayer(conn, playerControllerId);

		GameObject triviaPlayer = conn.playerControllers[0].gameObject;

		PlayerData playerData = new PlayerData(players.Count, "Player", triviaPlayer.GetComponent<TriviaGameManagerClient>());

		playerData.triviaGameManagerClient.triviaGameManagerServer = this;
		playerData.triviaGameManagerClient.playerId = players.Count;

		players.Add(playerData);
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player) {
		base.OnServerRemovePlayer(conn, player);

		print("Removed: " + players.Remove(players.Find(x => x.triviaGameManagerClient == player.gameObject.GetComponent<TriviaGameManagerClient>())).ToString());
	}

	private void StartGame() {
		StartCoroutine(GameLoop());
	}

	private IEnumerator SendQuestionToClients() {
		// TODO: Actually transmit question/UNET it.

		yield return QuestionReceiver.GetQuestion((returnedQuestionData) => {
			currentQuestionData = returnedQuestionData;
		});
		
		QuestionDataClient questionDataClient = new QuestionDataClient(currentQuestionData.question, currentQuestionData.shuffled_answers);

		playersAnswered = 0;

		for(int i = 0; i < players.Count; i++) {
			players[i].triviaGameManagerClient.CmdChangeQuestion(questionDataClient);
			players[i].chosenAnswerIndex = -1;
		}
	}

	private void SendLeaderboardData() {
		LeaderboardData[] leaderboardItems = GetLeaderboardItems();

		for(int i = 0; i < players.Count; i++) {
			players[i].triviaGameManagerClient.leaderboardController.CmdSetLeaderboardData(leaderboardItems);
		}
	}

	private void UpdateClientsLeaderboardPositons() {
		for(int i = 0; i < players.Count; i++) {
			players[i].triviaGameManagerClient.leaderboardController.CmdUpdatePositions(false);	
		}
	}

	private void ChangeClientsScreens(int newScreen) {
		for(int i = 0; i < players.Count; i++) {
			players[i].triviaGameManagerClient.screenSwitchController.CmdSetScreen(newScreen);
		}
	}

	private void ProcessAnswers() {
		// TODO: Scores based on answer speed.
		for(int i = 0; i < players.Count; i++) {
			PlayerData currentPlayer = players[i];
			currentPlayer.triviaGameManagerClient.CmdDisplayCorrectAnswer(currentQuestionData.correct_answer_index);
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

				if(playersAnswered == players.Count) {
					break;
				}

				yield return null;
			}

			ProcessAnswers();

			SendLeaderboardData();

			yield return new WaitForSecondsRealtime(resultsShowTime);
			
			ChangeClientsScreens(1);

			yield return new WaitForSeconds(screenSwitchTime);



			yield return new WaitForSeconds(leaderboardShowTime);
		}
	}

	public void ReceiveAnswerFromClient(int playerId, int answerIndex) {
		if(players[playerId].chosenAnswerIndex < 0 && 
		   (answerIndex >= 0 && answerIndex < currentQuestionData.shuffled_answers.Length)) {
			playersAnswered++;
			players[playerId].chosenAnswerIndex = answerIndex;
		}
	}
}
