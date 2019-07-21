using DG.Tweening;
using UnityEngine;
using UnityEngine.Networking;

public class ScreenSwitchController : NetworkBehaviour {

	public RectTransform screen;
	public RectTransform screenContainer;

	public float screenSwitchDuration = 1.25f;

	private Tween currentScreenTween;
	
	private void Start() {
		screen = GetComponent<RectTransform>();
	}

	[Command]
	public void CmdSetScreen(int newScreen) {
		if(currentScreenTween != null && currentScreenTween.IsPlaying()) {
			currentScreenTween.Kill();
		}

		currentScreenTween = screenContainer.DOAnchorPosX(-newScreen * screen.sizeDelta.x, screenSwitchDuration);
		currentScreenTween.Play();
	}
}
