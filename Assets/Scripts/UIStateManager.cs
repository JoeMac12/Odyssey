using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class UIStateManager : MonoBehaviour
{
	public enum UIState
	{
		IntroductionUI,
		GameControlsInfoUI,
		GameplayUI,
		FlightPerformanceUI,
		UpgradeUI,
		PauseUI,
		OptionsUI,
		WinUI
	}

	[Header("UI Panels")]
	public GameObject introductionPanel;
	public GameObject gameControlsPanel;
	public GameObject rocketUI;
	public GameObject weatherUI;
	public GameObject rocketPanelUI;
	public GameObject performancePanel;
	public GameObject upgradePanel;
	public GameObject pauseMenuPanel;
	public GameObject optionsPanel;
	public GameObject winPanel;

	[Header("Navigation Buttons")]
	public Button controlsButton;
	public Button proceedButton;
	public Button optionsButton;
	public Button backToPauseButton;

	[Header("Settings")]
	public float panelFadeDuration = 1f;

	private UIState currentState;
	private UIState previousState;

	public void Initialize()
	{
		SetupButtonListeners();
		SetState(UIState.IntroductionUI);
	}

	private void SetupButtonListeners()
	{
		if (controlsButton != null)
			controlsButton.onClick.AddListener(() => SetState(UIState.GameControlsInfoUI));

		if (proceedButton != null)
			proceedButton.onClick.AddListener(() => SetState(UIState.GameplayUI));

		if (optionsButton != null)
			optionsButton.onClick.AddListener(() => SetState(UIState.OptionsUI));

		if (backToPauseButton != null)
			backToPauseButton.onClick.AddListener(() => SetState(UIState.PauseUI));
	}

	public void SetState(UIState newState)
	{
		if (newState == UIState.PauseUI)
		{
			previousState = currentState;
			StartCoroutine(ShowPauseMenu());
			currentState = newState;
			return;
		}
		else if (newState == UIState.OptionsUI)
		{
			previousState = currentState;
			StartCoroutine(ShowOptionsMenu());
			currentState = newState;
			return;
		}

		StartCoroutine(TransitionState(newState));
	}

	public void ReturnFromPause()
	{
		StartCoroutine(HidePauseMenu());
		currentState = previousState;
	}

	public void ReturnFromOptions()
	{
		StartCoroutine(HideOptionsMenu());
		SetState(UIState.PauseUI);
	}

	private IEnumerator ShowPauseMenu()
	{
		pauseMenuPanel.SetActive(true);
		CanvasGroup pauseGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeInPanel(pauseGroup));
	}

	private IEnumerator ShowOptionsMenu()
	{
		optionsPanel.SetActive(true);
		CanvasGroup optionsGroup = optionsPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeInPanel(optionsGroup));
	}

	private IEnumerator HidePauseMenu()
	{
		CanvasGroup pauseGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeOutPanel(pauseGroup));
		pauseMenuPanel.SetActive(false);
	}

	private IEnumerator HideOptionsMenu()
	{
		CanvasGroup optionsGroup = optionsPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeOutPanel(optionsGroup));
		optionsPanel.SetActive(false);
	}

	private IEnumerator TransitionState(UIState newState)
	{
		if (currentState != UIState.PauseUI && currentState != UIState.OptionsUI)
		{
			yield return StartCoroutine(FadeOutCurrentState());
		}

		DisableAllUI(preservePauseMenu: currentState == UIState.PauseUI || currentState == UIState.OptionsUI);

		switch (newState)
		{
			case UIState.IntroductionUI:
				introductionPanel.SetActive(true);
				yield return StartCoroutine(FadeInPanel(introductionPanel.GetComponent<CanvasGroup>()));
				break;

			case UIState.GameControlsInfoUI:
				gameControlsPanel.SetActive(true);
				yield return StartCoroutine(FadeInPanel(gameControlsPanel.GetComponent<CanvasGroup>()));
				break;

			case UIState.GameplayUI:
				yield return StartCoroutine(EnableGameplayUI());
				break;

			case UIState.FlightPerformanceUI:
				performancePanel.SetActive(true);
				yield return StartCoroutine(FadeInPanel(performancePanel.GetComponent<CanvasGroup>()));
				break;

			case UIState.UpgradeUI:
				upgradePanel.SetActive(true);
				yield return StartCoroutine(FadeInPanel(upgradePanel.GetComponent<CanvasGroup>()));
				break;

			case UIState.WinUI:
				winPanel.SetActive(true);
				yield return StartCoroutine(FadeInPanel(winPanel.GetComponent<CanvasGroup>()));
				break;
		}

		currentState = newState;
	}

	private void DisableAllUI(bool preservePauseMenu = false)
	{
		introductionPanel.SetActive(false);
		gameControlsPanel.SetActive(false);
		rocketUI.SetActive(false);
		weatherUI.SetActive(false);
		rocketPanelUI.SetActive(false);
		performancePanel.SetActive(false);
		upgradePanel.SetActive(false);
		winPanel.SetActive(false);

		if (!preservePauseMenu)
		{
			pauseMenuPanel.SetActive(false);
			optionsPanel.SetActive(false);
		}
	}

	private IEnumerator EnableGameplayUI()
	{
		rocketUI.SetActive(true);
		weatherUI.SetActive(true);
		rocketPanelUI.SetActive(true);

		CanvasGroup[] groups = {
			rocketUI.GetComponent<CanvasGroup>(),
			weatherUI.GetComponent<CanvasGroup>(),
			rocketPanelUI.GetComponent<CanvasGroup>()
		};

		foreach (var group in groups)
		{
			group.alpha = 0f;
		}

		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			float alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeDuration);
			foreach (var group in groups)
			{
				group.alpha = alpha;
			}
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		foreach (var group in groups)
		{
			group.alpha = 1f;
		}
	}

	private IEnumerator FadeOutCurrentState()
	{
		CanvasGroup[] activeGroups = GetActiveCanvasGroups();
		if (activeGroups.Length == 0) yield break;

		yield return StartCoroutine(FadeOutPanels(activeGroups));
	}

	private IEnumerator FadeOutPanels(CanvasGroup[] groups)
	{
		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			float alpha = Mathf.Lerp(1f, 0f, elapsedTime / panelFadeDuration);
			foreach (var group in groups)
			{
				group.alpha = alpha;
			}
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}

		foreach (var group in groups)
		{
			group.alpha = 0f;
		}
	}

	private IEnumerator FadeOutPanel(CanvasGroup group)
	{
		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			group.alpha = Mathf.Lerp(1f, 0f, elapsedTime / panelFadeDuration);
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}
		group.alpha = 0f;
	}

	private CanvasGroup[] GetActiveCanvasGroups()
	{
		var activeGroups = new List<CanvasGroup>();

		if (introductionPanel.activeSelf) activeGroups.Add(introductionPanel.GetComponent<CanvasGroup>());
		if (gameControlsPanel.activeSelf) activeGroups.Add(gameControlsPanel.GetComponent<CanvasGroup>());
		if (rocketUI.activeSelf) activeGroups.Add(rocketUI.GetComponent<CanvasGroup>());
		if (weatherUI.activeSelf) activeGroups.Add(weatherUI.GetComponent<CanvasGroup>());
		if (rocketPanelUI.activeSelf) activeGroups.Add(rocketPanelUI.GetComponent<CanvasGroup>());
		if (performancePanel.activeSelf) activeGroups.Add(performancePanel.GetComponent<CanvasGroup>());
		if (upgradePanel.activeSelf) activeGroups.Add(upgradePanel.GetComponent<CanvasGroup>());
		if (winPanel.activeSelf) activeGroups.Add(winPanel.GetComponent<CanvasGroup>());

		return activeGroups.ToArray();
	}

	private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
	{
		canvasGroup.alpha = 0f;
		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeDuration);
			elapsedTime += Time.unscaledDeltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public UIState GetCurrentState()
	{
		return currentState;
	}

	public bool IsGameplayState()
	{
		return currentState == UIState.GameplayUI;
	}
}
