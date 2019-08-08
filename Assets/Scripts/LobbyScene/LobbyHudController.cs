using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class LobbyHudController : MonoBehaviour {

	public static LobbyHudController instance;

	private static int DEFAULT_PORT = 7777;

	public TMP_InputField usernameContainer;

	public TMP_InputField ipAddressContainer;

	public TMP_InputField portContainer;
	
	public Canvas lobbyCanvas;

	public GameObject menuScreen;

	public GameObject serverScreen;

	public GameObject playerlistEntryPrefab;

	public RectTransform lobbyPlayerList;

	public Button startServerButton;

	public string username {
		get {
			if(usernameContainer == null) {
				return "Player";
			}

			string name = usernameContainer.text;

			if(name.Length > TriviaGameManagerServer.MAX_CHARACTERS_NAME) {
				return name.Substring(0, TriviaGameManagerServer.MAX_CHARACTERS_NAME);
			}

			return name;
		}
	}

	public string ipAddress {
		get {
			if(ipAddressContainer == null) {
				return null;
			}

			return ipAddressContainer.text;
		}
	}

	public int port {
		get {
			if(portContainer == null) {
				return DEFAULT_PORT;
			}

			int tryParseReceiver = 0;

			if(!int.TryParse(portContainer.text, out tryParseReceiver)) {
				return DEFAULT_PORT;
			}

			return tryParseReceiver;
		}
	}

	private void Start() {
		if(instance != null && instance != this) {
			Destroy(this);
		}

		instance = this;

		if(ipAddressContainer != null) {
			ipAddressContainer.onValueChanged.AddListener(CheckIPValidity);
		}

		if(portContainer != null) {
			portContainer.onValueChanged.AddListener(CheckPortValidity);
		}
	}

	private void OnDestroy() {
		if(instance == this) {
			instance = null;
		}
	}

	public void ToggleServerStartButtonState(bool state) {
		startServerButton.interactable = state;
	}
	
	public void ToggleScreen(ScreenState state) {
		switch(state) {
			case ScreenState.Closed:
				CleanLobbyList();
				ToggleServerStartButtonState(false);
				menuScreen.SetActive(false);
				serverScreen.SetActive(false);
				break;
			case ScreenState.Lobby:
				menuScreen.SetActive(false);
				serverScreen.SetActive(true);
				break;
			case ScreenState.Menu:
				CleanLobbyList();
				ToggleServerStartButtonState(false);
				menuScreen.SetActive(true);
				serverScreen.SetActive(false);
				break;
		}
	}

	public void CleanLobbyList() {
		LeaderboardItem[] leaderboardItems = lobbyPlayerList.GetComponentsInChildren<LeaderboardItem>();

		for(int i = 0; i < leaderboardItems.Length; i++) {
			Destroy(leaderboardItems[i].gameObject);
		}
	}

	public void UpdateLobbyPlayerlist(LeaderboardData[] leaderboardData) {
		CleanLobbyList();

		for(int i = 0; i < leaderboardData.Length; i++) {
			LeaderboardData entry = leaderboardData[i];

			GameObject listItem = Instantiate(playerlistEntryPrefab, lobbyPlayerList);

			LeaderboardItem leaderboardItem = listItem.GetComponent<LeaderboardItem>();

			leaderboardItem.Username = entry.username;
		}
	}

	public bool CheckInputValidity() {
		int tryParseReceiver = 0;

		if(!int.TryParse(portContainer.text, out tryParseReceiver)) {
			return false;
		}

		if(ipAddress == null) {
			return false;
		}

		return true;
	}

	private void CheckIPValidity(string newText) {
		// TODO: Check this.
	}

	private void CheckPortValidity(string newText) {
		int tryParseReceiver = 0;

		if(!int.TryParse(newText, out tryParseReceiver)) {
			portContainer.text = DEFAULT_PORT.ToString();
		}

		portContainer.text = tryParseReceiver.ToString();
	}
}