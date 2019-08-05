using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour {

	[HideInInspector]
	public TriviaGameManagerClient triviaGameManagerClient;

	public TextMeshProUGUI questionLabel;

	public Image[] answerImages;
	public TextMeshProUGUI[] answerLabels;

	private Button[] answerButtons;

	private Color defaultAnswerButtonColor;

	private void Start() {
		if(!isLocalPlayer) {
			enabled = false;
			return;
		}

		answerButtons = new Button[answerLabels.Length];

		for(int i = 0; i < answerButtons.Length; i++) {
			answerButtons[i] = answerImages[i].GetComponent<Button>();

			int copyOfIndex = i - 0;

			answerButtons[i].onClick.AddListener(() => {
				OnAnswerPressed(copyOfIndex);
			});
		}

		defaultAnswerButtonColor = answerImages[0].color;
	}

	public void ChangeQuestion(QuestionDataClient questionData) {
		questionLabel.text = questionData.Question;
		
		for(int i = 0; i < 4; i++) {
			if(i < questionData.Answers.Length) {
				answerLabels[i].text = questionData.Answers[i];
				answerImages[i].enabled = true;
			} else {
				answerImages[i].enabled = false;
			}
		}
	}

	public void OnAnswerPressed(int answerIndex) {
		SelectAnswer(answerIndex);
	}

	public void SelectAnswer(int answerIndex) {
		if(triviaGameManagerClient.questionAnswered) {
			return;
		}

		ColorAnswer(answerIndex, defaultAnswerButtonColor - new Color(.1f, .1f, .1f, 0));
		triviaGameManagerClient.SendAnswer(answerIndex);
	}

	public void ColorAnswer(int answerIndex) {
		ColorAnswer(answerIndex, defaultAnswerButtonColor);
	}

	public void ColorAnswer(int answerIndex, Color color) {
		if(answerIndex < 0 || answerIndex >= answerImages.Length) {
			return;
		}

		answerImages[answerIndex].color = color;
	}
}
