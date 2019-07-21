using UnityEngine;

public class TriviaGameManagerClient : MonoBehaviour {

	public int playerId = -1;

	public TriviaGameManagerServer triviaGameManagerServer;

	public UIManager uiManager;

	public bool questionAnswered {
		get {
			return chosenAnswerIndex >= 0;
		}
	}

	private int correctAnswerIndex = -1;
	private int chosenAnswerIndex = -1;
	
	public void Start() {
		uiManager.triviaGameManagerClient = this;
	}

	public void ChangeQuestion(QuestionDataClient questionData) {
		if(correctAnswerIndex != chosenAnswerIndex && chosenAnswerIndex >= 0) {
			print(chosenAnswerIndex);
			uiManager.ColorAnswer(chosenAnswerIndex);
		}

		print(chosenAnswerIndex);
		
		if(correctAnswerIndex >= 0) {
			print(correctAnswerIndex);
			uiManager.ColorAnswer(correctAnswerIndex);
		}
		
		uiManager.ChangeQuestion(questionData);
		correctAnswerIndex = -1;
		chosenAnswerIndex = -1;
	}

	public void DisplayCorrectAnswer(int correctAnswerIndex) {
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

		triviaGameManagerServer.ReceiveAnswerFromClient(playerId, answerIndex);
	}
}
