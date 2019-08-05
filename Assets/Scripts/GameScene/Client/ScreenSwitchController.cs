using DG.Tweening;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ScreenSwitchController : NetworkBehaviour {
	
	public RectTransform canvasRectTransform;
	public RectTransform screenContainer;

	public float screenSwitchDuration = 1.25f;

	private Tween currentScreenTween;
	
	private void Start() {
		RectTransform[] screens = screenContainer.GetComponentsInChildren<RectTransform>();

		screens.OrderBy(x => -x.GetComponent<ScreenData>().screenId);

		for(int i = 0; i < screens.Length; i++) {
			RectTransform screen = screens[i];

			//screen.anchoredPosition = new Vector2(canvasRectTransform.sizeDelta.x * i, screen.anchoredPosition.y);
		}
	}

	[ClientRpc]
	public void RpcSetScreen(int newScreen) {
		if(!isLocalPlayer) {
			return;
		}

		return;
		
		if(currentScreenTween != null && currentScreenTween.IsPlaying()) {
			currentScreenTween.Kill();
		}

		currentScreenTween = screenContainer.DOAnchorPosX(-newScreen * canvasRectTransform.sizeDelta.x, screenSwitchDuration);
		currentScreenTween.Play();
	}
}
