﻿using UnityEngine;
using UnityEngine.Networking;

public class TriviaGameManagerClient : NetworkBehaviour {

	public int playerId = -1;

	public TriviaGameManagerServer triviaGameManagerServer;

	public UIManager uiManager;

	public ScreenSwitchController screenSwitchController;

	public LeaderboardController leaderboardController;

	public bool questionAnswered {
		get {
			return chosenAnswerIndex >= 0;
		}
	}

	private int correctAnswerIndex = -1;
	private int chosenAnswerIndex = -1;
	
	public void Start() {
		if(isLocalPlayer == false) {
			enabled = false;
			gameObject.SetActive(false);
			return;
		}

		uiManager.triviaGameManagerClient = this;
		DG.Tweening.DOTween.Init(true, true);
	}

	[ClientRpc]
	public void RpcChangeQuestion(QuestionDataClient questionData) {
		if(!isLocalPlayer) {
			return;
		}
		
		if(correctAnswerIndex != chosenAnswerIndex && chosenAnswerIndex >= 0) {
			uiManager.ColorAnswer(chosenAnswerIndex);
		}
		
		if(correctAnswerIndex >= 0) {
			uiManager.ColorAnswer(correctAnswerIndex);
		}
		
		uiManager.ChangeQuestion(questionData);
		correctAnswerIndex = -1;
		chosenAnswerIndex = -1;
	}

	[ClientRpc]
	public void RpcDisplayCorrectAnswer(int correctAnswerIndex) {
		if(!isLocalPlayer) {
			return;
		}

		this.correctAnswerIndex = correctAnswerIndex;

		if(correctAnswerIndex != chosenAnswerIndex && chosenAnswerIndex >= 0) {
			uiManager.ColorAnswer(chosenAnswerIndex, Color.red);
		}

		uiManager.ColorAnswer(correctAnswerIndex, Color.green);
	}
	
	public void SendAnswer(int answerIndex) {
		if(chosenAnswerIndex >= 0) {
			return;
		}

		chosenAnswerIndex = answerIndex;
		
		CmdReceiveAnswerFromClient(answerIndex);
	}

	[Command]
	public void CmdReceiveAnswerFromClient(int answerIndex) {
		if(!isServer) {
			return;
		}
		
		if(triviaGameManagerServer.players[playerId].chosenAnswerIndex < 0 &&
		   (answerIndex >= 0 && answerIndex < triviaGameManagerServer.currentQuestionData.shuffled_answers.Length)) {
			triviaGameManagerServer.playersAnswered++;
			triviaGameManagerServer.players[playerId].chosenAnswerIndex = answerIndex;
		}
	}
}
