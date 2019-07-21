using DG.Tweening;
using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour {

	public TextMeshProUGUI usernameLabel;
	public TextMeshProUGUI scoreLabel;

	public int PlayerId = -1;

	private string username;
	private int score = 0;
	private int oldScore = 0;

	public string Username {
		get {
			return username;
		}

		set {
			username = value;
			usernameLabel.text = value;
		}
	}

	public int Score {
		get {
			return score;
		}

		set {
			score = value;
			UpdateScoreText();
		}
	}

	public void UpdateScoreText() {
		/*Tween intTween = DOTween.To(oldScore, x => {
			oldScore = x;
			scoreLabel.text = IntToReadableNumber(x);
		}, score, 1.25f);*/
		scoreLabel.text = IntToReadableNumber(score);

		oldScore = score;
	}

	private string IntToReadableNumber(int v) {
		return string.Format("{0:n0}", v);
	}
}
