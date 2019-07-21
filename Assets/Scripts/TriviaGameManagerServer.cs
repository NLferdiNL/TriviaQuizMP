using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriviaGameManagerServer : MonoBehaviour {

	private List<PlayerData> players = new List<PlayerData>();

	public QuestionDataServer currentQuestionData;

	public bool singlePlayerTest = true;

	public float questionAnswerTime = 10;
	public float resultsShowTime = 5;

	public bool gameRunning = true;

	private int playersAnswered = 0;
	
	private void Start() {
		if(singlePlayerTest) {
			PlayerData playerData = new PlayerData(0, "Player", GetComponent<TriviaGameManagerClient>());

			playerData.triviaGameManagerClient.triviaGameManagerServer = this;
			playerData.triviaGameManagerClient.playerId = 0;

			players.Add(playerData);
		}

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
			players[i].triviaGameManagerClient.ChangeQuestion(questionDataClient);
			players[i].chosenAnswerIndex = -1;
		}
	}

	private void ProcessAnswers() {
		for(int i = 0; i < players.Count; i++) {
			players[i].triviaGameManagerClient.DisplayCorrectAnswer(currentQuestionData.correct_answer_index);
			//print(players[i].chosenAnswerIndex == currentQuestionData.correct_answer_index);
		}
	}

	private IEnumerator GameLoop() {

		while(gameRunning) {
			yield return SendQuestionToClients();
			
			float timeWaited = 0;

			while(timeWaited < questionAnswerTime) {
				timeWaited += Time.deltaTime;

				if(playersAnswered == players.Count) {
					break;
				}

				yield return null;
			}

			ProcessAnswers();

			yield return new WaitForSecondsRealtime(resultsShowTime);

			// TODO: Show clients correct and wrong answer.
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
