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

	[Header("Options Settings")]
	public Slider musicVolumeSlider;
	public TMP_Text musicVolumeText;
	public MusicManager musicManager;

	[Header("Settings")]
	public float panelFadeDuration = 1f;

	private UIState currentState;
	private UIState previousState;
	private Dictionary<UIState, GameObject[]> stateToUI;

	public delegate void StateChangeHandler();
	public event StateChangeHandler OnStateChanged;

	public void Initialize()
	{
		SetupButtonListeners();
		SetupOptionsControls();
		StateMapping();
		SetState(UIState.IntroductionUI);
	}

	private void StateMapping()
	{
		stateToUI = new Dictionary<UIState, GameObject[]>
		{
			{ UIState.IntroductionUI, new[] { introductionPanel } },
			{ UIState.GameControlsInfoUI, new[] { gameControlsPanel } },
			{ UIState.GameplayUI, new[] { rocketUI, weatherUI, rocketPanelUI } },
			{ UIState.FlightPerformanceUI, new[] { performancePanel } },
			{ UIState.UpgradeUI, new[] { upgradePanel } },
			{ UIState.PauseUI, new[] { pauseMenuPanel } },
			{ UIState.OptionsUI, new[] { optionsPanel } },
			{ UIState.WinUI, new[] { winPanel } }
		};
	}

	private void SetupOptionsControls()
	{
		if (musicVolumeSlider != null && musicManager != null)
		{
			musicVolumeSlider.value = musicManager.GetMasterMusicVolume();
			musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
			UpdateMusicVolumeText();
		}
	}

	private void OnMusicVolumeChanged(float value)
	{
		if (musicManager != null)
		{
			musicManager.SetMasterMusicVolume(value);
			UpdateMusicVolumeText();
		}
	}

	private void UpdateMusicVolumeText()
	{
		if (musicVolumeText != null)
		{
			musicVolumeText.text = $"Music Volume: {(musicVolumeSlider.value * 100):F0}%";
		}
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
			if (currentState != UIState.OptionsUI)
			{
				previousState = currentState;
			}
			StartCoroutine(ShowPauseMenu());
			return;
		}
		else if (newState == UIState.OptionsUI)
		{
			StartCoroutine(ShowOptionsMenu());
			return;
		}

		StartCoroutine(TransitionState(newState));
	}

	public void ReturnFromPause()
	{
		StartCoroutine(TransitionFromPause());
	}

	public void ReturnFromOptions()
	{
		StartCoroutine(HideOptionsMenu());
		SetState(UIState.PauseUI);
	}

	private IEnumerator ShowPauseMenu()
	{
		yield return StartCoroutine(FadeOutCurrentState());
		DisableAllUI();

		pauseMenuPanel.SetActive(true);
		CanvasGroup pauseGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeInPanel(pauseGroup));
		currentState = UIState.PauseUI;
		OnStateChanged?.Invoke();
	}

	private IEnumerator TransitionFromPause()
	{
		CanvasGroup pauseGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeOutPanel(pauseGroup));
		pauseMenuPanel.SetActive(false);

		if (stateToUI.TryGetValue(previousState, out GameObject[] uiElements))
		{
			foreach (var element in uiElements)
			{
				element.SetActive(true);
				if (element.TryGetComponent<CanvasGroup>(out var canvasGroup))
				{
					yield return StartCoroutine(FadeInPanel(canvasGroup));
				}
			}
		}

		currentState = previousState;
		OnStateChanged?.Invoke();
	}

	private IEnumerator ShowOptionsMenu()
	{
		yield return StartCoroutine(FadeOutCurrentState());
		DisableAllUI();

		optionsPanel.SetActive(true);
		CanvasGroup optionsGroup = optionsPanel.GetComponent<CanvasGroup>();
		yield return StartCoroutine(FadeInPanel(optionsGroup));
		currentState = UIState.OptionsUI;
		OnStateChanged?.Invoke();
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

		DisableAllUI();

		if (stateToUI.TryGetValue(newState, out GameObject[] newStateUI))
		{
			foreach (var element in newStateUI)
			{
				element.SetActive(true);
				if (element.TryGetComponent<CanvasGroup>(out var canvasGroup))
				{
					yield return StartCoroutine(FadeInPanel(canvasGroup));
				}
			}
		}

		currentState = newState;
		OnStateChanged?.Invoke();
	}

	private void DisableAllUI()
	{
		foreach (var uiElements in stateToUI.Values)
		{
			foreach (var element in uiElements)
			{
				element.SetActive(false);
			}
		}
	}

	private IEnumerator FadeOutCurrentState()
	{
		if (stateToUI.TryGetValue(currentState, out GameObject[] currentStateUI))
		{
			var activeGroups = new List<CanvasGroup>();
			foreach (var element in currentStateUI)
			{
				if (element.activeSelf && element.TryGetComponent<CanvasGroup>(out var canvasGroup))
				{
					activeGroups.Add(canvasGroup);
				}
			}

			if (activeGroups.Count > 0)
			{
				yield return StartCoroutine(FadeOutPanels(activeGroups.ToArray()));
			}
		}
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
