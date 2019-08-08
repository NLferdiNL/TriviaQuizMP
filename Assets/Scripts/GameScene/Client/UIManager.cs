using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class UIManager : NetworkBehaviour {

	private static Color CHOSEN_COLOR_DARKENER = new Color(.3f, .3f, .3f, 0);

	[HideInInspector]
	public TriviaGameManagerClient triviaGameManagerClient;

	public TextMeshProUGUI questionLabel;

	public Image[] answerImages;
	public TextMeshProUGUI[] answerLabels;

	public Image timerClockImage;

	public TextMeshProUGUI timerLabel;

	public bool acceptAnswer = false;

	private Button[] answerButtons;

	private Color defaultAnswerButtonColor;

	private float maxQuestionTimer = 0;
	private float questionTimer = 0;

	private Coroutine timerCoroutineInstance = null;

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
				answerLabels[i].enabled = true;
				answerImages[i].enabled = true;
			} else {
				answerLabels[i].enabled = false;
				answerImages[i].enabled = false;
			}
		}
	}

	public void OnAnswerPressed(int answerIndex) {
		if(!acceptAnswer) {
			return;
		}

		SelectAnswer(answerIndex);
	}

	public void SelectAnswer(int answerIndex) {
		if(!acceptAnswer) {
			return;
		}

		if(triviaGameManagerClient.questionAnswered) {
			return;
		}

		ColorAnswer(answerIndex, defaultAnswerButtonColor - CHOSEN_COLOR_DARKENER);
		triviaGameManagerClient.SendAnswer(answerIndex);
	}

	public void StartTimer(float time) {
		if(timerCoroutineInstance != null) {
			StopTimer();
		}

		maxQuestionTimer = time;
		questionTimer = time;

		timerCoroutineInstance = StartCoroutine(TimerCoroutine());
	}

	public void SyncTimer(float time) {
		questionTimer = time;
	}

	public void StopTimer() {
		if(timerCoroutineInstance != null) {
			StopCoroutine(timerCoroutineInstance);
		}
	}

	public void ResetTimer(float time) {
		maxQuestionTimer = time;
		questionTimer = time;

		if(timerClockImage != null) {
			timerClockImage.fillAmount = 1;
		}

		if(timerLabel != null) {
			timerLabel.text = ((int)time).ToString();
		}
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

	private IEnumerator TimerCoroutine() {
		while(questionTimer >= 0) {
			questionTimer -= Time.deltaTime;

			if(questionTimer < 0) {
				questionTimer = 0;
			}

			if(timerClockImage != null) {
				timerClockImage.fillAmount = questionTimer / maxQuestionTimer;
			}

			if(timerLabel != null) {
				timerLabel.text = ((int)questionTimer).ToString();
			}

			yield return null;
		}
	}
}
