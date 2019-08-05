using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyHudController : MonoBehaviour {

	private static int DEFAULT_PORT = 7777;

	public TMP_InputField ipAddressContainer;

	public TMP_InputField portContainer;

	public Canvas lobbyCanvas;

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
		if(ipAddressContainer != null) {
			ipAddressContainer.onValueChanged.AddListener(CheckIPValidity);
		}

		if(portContainer != null) {
			portContainer.onValueChanged.AddListener(CheckPortValidity);
		}
	}

	public void ToggleLobbyUI(bool state) {
		lobbyCanvas.enabled = state;
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