	using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class LeaderboardController : NetworkBehaviour {

	public GameObject playerListingPrefab;

	public Transform leaderboardContainer;

	public float topPadding = -112.5f;
	public float itemSize = 47 + 9; //Item + padding.

	public float listMovementDuration = 2;

	private List<LeaderboardItem> players = new List<LeaderboardItem>();

	private Sequence currentLeaderboardSequence;

	private void Start() {
		if(isLocalPlayer == false) {
			enabled = false;
			return;
		}
	}

	/*private void Start() {
		for(int i = 0; i < 8; i++) {
			AddPlayer(i, "Player" + i);
		}

		UpdatePositions();

		Invoke("DebugUpdate",5);
	}

	private void DebugUpdate() {
		for(int i = 0; i < players.Count; i++) {
			UpdatePlayer(i, GetPlayer(i).Score + Random.Range(100, 1000));
		}

		UpdatePositions();

		Invoke("DebugUpdate", 5);
	}*/

	[Command]
	public void CmdSetLeaderboardData(LeaderboardData[] newLeaderboard) {
		players = players.OrderBy(i => i.PlayerId).ToList();

		for(int i = players.Count - 1; i >= 0; i--) {
			LeaderboardItem currentPlayer = players[i];

			LeaderboardData possibleMatch = Array.Find(newLeaderboard, x => x.playerId == currentPlayer.PlayerId);

			if(possibleMatch != null) {
				newLeaderboard[Array.IndexOf(newLeaderboard, possibleMatch)] = null;

				currentPlayer.Username = possibleMatch.username;
				currentPlayer.Score = possibleMatch.score;

				continue;
			}

			players.Remove(currentPlayer);

			Destroy(currentPlayer.gameObject);
		}

		for(int i = 0; i < newLeaderboard.Length; i++) {
			if(newLeaderboard[i] == null) {
				continue;
			}

			LeaderboardData currentPlayer = newLeaderboard[i];

			AddPlayer(currentPlayer.playerId, currentPlayer.username, currentPlayer.score);
		}

		//UpdatePositions(true);
	}

	public void AddPlayer(int playerId, string username) {
		AddPlayer(playerId, username, 0);
	}

	public void AddPlayer(int playerId, string username, int score) {
		GameObject newPlayerListing = Instantiate(playerListingPrefab, leaderboardContainer);

		LeaderboardItem newPlayer = newPlayerListing.GetComponent<LeaderboardItem>();

		newPlayer.PlayerId = playerId;
		newPlayer.Username = username;
		newPlayer.Score = score;

		players.Add(newPlayer);

		RectTransform newPlayerPos = newPlayerListing.GetComponent<RectTransform>();

		newPlayerPos.anchoredPosition = new Vector2(newPlayerPos.anchoredPosition.x, GetPositionYOnLeaderboard(players.Count - 1));
	}

	public void UpdatePlayer(int playerId, int newScore) {
		// Tween number?
		LeaderboardItem player = GetPlayer(playerId);
		
		if(player != null) {
			player.Score = newScore;
		}
	}
	
	[Command]
	public void CmdUpdatePositions(bool skipAnim) {
		if(currentLeaderboardSequence != null && currentLeaderboardSequence.IsPlaying()) {
			currentLeaderboardSequence.Kill();
		}

		players = players.OrderBy(i => -i.Score).ToList();

		currentLeaderboardSequence = DOTween.Sequence();

		for(int i = 0; i < players.Count; i++) {
			LeaderboardItem currentPlayer = players[i];

			float newYPosition = GetPositionYOnLeaderboard(i);

			currentLeaderboardSequence.Insert(0, currentPlayer.GetComponent<RectTransform>().DOAnchorPosY(newYPosition, skipAnim ? 0 : listMovementDuration));
		}

		currentLeaderboardSequence.Play();
	}

	private float GetPositionYOnLeaderboard(int index) {
		return topPadding - index * itemSize;
	}

	private LeaderboardItem GetPlayer(int playerId) {
		return players.Find(x => x.PlayerId == playerId);
	}
}
