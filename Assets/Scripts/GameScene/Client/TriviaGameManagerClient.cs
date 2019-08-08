using UnityEngine;
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
	public void RpcLobbyChangeState(ScreenState newState) {
		if(LobbyHudController.instance == null) {
			return;
		}

		LobbyHudController.instance.ToggleScreen(newState);
	}

	[ClientRpc]
	public void RpcChangeAnswerAcceptState(bool newState) {
		if(uiManager == null) {
			return;
		}

		uiManager.acceptAnswer = newState;
	}

	[ClientRpc]
	public void RpcUpdatePlayerlistLobby(LeaderboardData[] newLeaderboard) {
		if(LobbyHudController.instance == null) {
			return;
		}

		LobbyHudController.instance.UpdateLobbyPlayerlist(newLeaderboard);
	}

	[ClientRpc]
	public void RpcGetUsername() {
		if(LobbyHudController.instance == null) {
			CmdSendUsername("Player");
		}

		CmdSendUsername(LobbyHudController.instance.username);
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

	[ClientRpc]
	public void RpcStartTimer(float time) {
		uiManager.StartTimer(time);
	}

	[ClientRpc]
	public void RpcSyncTimer(float time) {
		uiManager.SyncTimer(time);
	}

	[ClientRpc]
	public void RpcStopTimer() {
		uiManager.StopTimer();
	}

	[ClientRpc]
	public void RpcResetTimer(float time) {
		uiManager.ResetTimer(time);
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

		if(!triviaGameManagerServer.acceptingAnswers) {
			return;
		}
		
		if(triviaGameManagerServer.players[playerId].chosenAnswerIndex < 0 &&
		   (answerIndex >= 0 && answerIndex < triviaGameManagerServer.currentQuestionData.shuffled_answers.Length)) {
			triviaGameManagerServer.playersAnswered++;
			triviaGameManagerServer.players[playerId].chosenAnswerIndex = answerIndex;
			triviaGameManagerServer.players[playerId].timeTaken = triviaGameManagerServer.timeWaited;
		}
	}

	[Command]
	public void CmdSendUsername(string username) {
		if(username.Length > TriviaGameManagerServer.MAX_CHARACTERS_NAME) {
			username = username.Substring(0, TriviaGameManagerServer.MAX_CHARACTERS_NAME);
		}

		triviaGameManagerServer.players[playerId].username = username;

		triviaGameManagerServer.SendLobbyPlayerlist();
	}

}
