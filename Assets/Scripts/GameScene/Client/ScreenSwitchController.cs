using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ScreenSwitchController : NetworkBehaviour {
	
	public RectTransform canvasRectTransform;
	public RectTransform screenContainer;

	public float screenSwitchDuration = 1.25f;

	private Tween currentScreenTween;

	private ScreenData[] screens;

	//private int currentScreen = 0;

	private void Start() {
		screens = screenContainer.GetComponentsInChildren<ScreenData>();

		screens.OrderBy(x => -x.screenId);

		for(int i = 0; i < screens.Length; i++) {
			ScreenData screen = screens[i];

			RectTransform screenRectTransform = screen.gameObject.GetComponent<RectTransform>();

			//if(i != currentScreen) {
			//	screen.gameObject.SetActive(false);
			//}

			//screenRectTransform.sizeDelta = canvasRectTransform.sizeDelta * canvasRectTransform.localScale;

			screenRectTransform.anchoredPosition = new Vector2(canvasRectTransform.sizeDelta.x * i, screenRectTransform.anchoredPosition.y);
		}
	}

	[ClientRpc]
	public void RpcSetScreen(int newScreen) {
		if(!isLocalPlayer) {
			return;
		}

		//screens[currentScreen].gameObject.SetActive(false);
		//screens[newScreen].gameObject.SetActive(true);

		//currentScreen = newScreen;

		if(currentScreenTween != null && currentScreenTween.IsPlaying()) {
			currentScreenTween.Kill();
		}

		currentScreenTween = screenContainer.DOAnchorPosX(-newScreen * canvasRectTransform.sizeDelta.x, screenSwitchDuration);
		currentScreenTween.Play();
	}
}
