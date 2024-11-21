using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public RocketController rocketController;
	public WindManager windManager;
	public UpgradeManager upgradeManager;
	public CameraController cameraController;
	public UIStateManager uiStateManager;

	[Header("UI Elements")]
	public TMP_Text maxAltitudeText;
	public TMP_Text maxSpeedText;
	public TMP_Text flightTimeText;
	public TMP_Text distanceTravelledText;
	public TMP_Text moneyEarnedText;
	public Button openUpgradeMenuButton;
	public Button closeUpgradeMenuButton;
	public Button selfDestructButton;

	[Header("Progress Bar")]
	public RectTransform progressBarBackground;
	public RectTransform rocketIcon;
	public float maxBarHeight = 500f;

	[Header("Pause Menu")]
	public Button resumeButton;
	public Button mainMenuButton;
	public Button quitGameButton;

	[Header("Win Condition")]
	public float winAltitude = 100000f;
	public Button resetGameButton;

	[Header("Money Multipliers")]
	public float altitudeMultiplier = 0.1f;
	public float speedMultiplier = 0.5f;
	public float timeMultiplier = 10f;
	public float distanceMultiplier = 0.2f;

	[Header("Effects")]
	public BlurEffectManager blurManager;

	[Header("Audio")]
	public MusicManager musicManager;
	public UISoundSystem uiSoundSystem;

	private Vector3 initialPosition;
	private float maxAltitude;
	private float maxSpeed;
	private float distanceTravelled;
	private float totalMoneyEarned = 0f;
	private bool isPaused = false;
	private bool hasWon = false;

	private const float metersToFeet = 3.28084f;
	private const float metersToMPH = 2.23694f;

	private void Start()
	{
		initialPosition = rocketController.transform.position;
		windManager.Initialize(rocketController);
		SetUIState();
		ResetFlightStats();

		if (musicManager != null)
		{
			musicManager.StartGameplayMusic();
		}

		SetupButtonListeners();
		UpdateRocketControlState();
	}

	private void SetupButtonListeners()
	{
		openUpgradeMenuButton.onClick.AddListener(OpenUpgradeMenu);
		closeUpgradeMenuButton.onClick.AddListener(CloseUpgradeMenu);
		resumeButton.onClick.AddListener(ResumeGame);
		mainMenuButton.onClick.AddListener(ReturnToMainMenu);
		quitGameButton.onClick.AddListener(QuitGame);
		resetGameButton.onClick.AddListener(ResetGame);
		selfDestructButton.onClick.AddListener(SelfDestructRocket);
	}

	private void SetUIState()
	{
		uiStateManager.Initialize();
		uiStateManager.OnStateChanged += UpdateRocketControlState;
	}

	private void OnDestroy()
	{
		if (uiStateManager != null)
		{
			uiStateManager.OnStateChanged -= UpdateRocketControlState;
		}
	}

	private void UpdateRocketControlState()
	{
		UIStateManager.UIState currentState = uiStateManager.GetCurrentState();
		bool shouldEnableControls = currentState == UIStateManager.UIState.GameplayUI;

		if (rocketController != null)
		{
			rocketController.enabled = shouldEnableControls && !isPaused;
		}

		if (cameraController != null)
		{
			cameraController.controlsEnabled = shouldEnableControls && !isPaused;
		}
	}

	private void UpdateProgressBar()
	{
		if (rocketController == null || rocketIcon == null) return;

		float currentAltitude = (rocketController.transform.position.y - initialPosition.y) * metersToFeet;
		float progress = Mathf.Clamp01(currentAltitude / winAltitude);

		Vector2 iconAnchoredPosition = rocketIcon.anchoredPosition;
		iconAnchoredPosition.y = maxBarHeight * progress;
		rocketIcon.anchoredPosition = iconAnchoredPosition;
	}

	private void Update()
	{
		if (!rocketController.IsExploded && !hasWon)
		{
			UpdateFlightStats();
			UpdateProgressBar();
			CheckWinCondition();
		}

		if (Input.GetKeyDown(KeyCode.Escape) && uiStateManager.GetCurrentState() != UIStateManager.UIState.OptionsUI)
		{
			TogglePause();
		}
	}

	private void ReturnToMainMenu()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene("MainMenu");
	}

	private void UpdateFlightStats()
	{
		float currentAltitude = (rocketController.transform.position.y - initialPosition.y) * metersToFeet;
		maxAltitude = Mathf.Max(maxAltitude, currentAltitude);

		float currentSpeed = rocketController.rb.velocity.magnitude * metersToMPH;
		maxSpeed = Mathf.Max(maxSpeed, currentSpeed);

		distanceTravelled = Vector3.Distance(initialPosition, rocketController.transform.position) * metersToFeet;
	}

	private void CheckWinCondition()
	{
		if (maxAltitude >= winAltitude && !hasWon)
		{
			hasWon = true;
			if (rocketController != null)
			{
				rocketController.enabled = false;
				rocketController.gameObject.SetActive(false);
			}
			StartCoroutine(ShowWinPanel());
		}
	}

	private IEnumerator ShowWinPanel()
	{
		uiStateManager.SetState(UIStateManager.UIState.WinUI);
		yield return null;
	}

	public void OnRocketExploded()
	{
		if (blurManager != null)
		{
			blurManager.EnableBlur();
		}

		if (!hasWon)
		{
			uiStateManager.SetState(UIStateManager.UIState.FlightPerformanceUI);
			if (musicManager != null)
			{
				musicManager.StartInterfaceMusic();
			}
			UpdatePerformanceUI();
		}
	}

	private void SelfDestructRocket()
	{
		if (rocketController != null && !rocketController.IsExploded)
		{
			rocketController.SelfDestruct();
		}
	}

	private void UpdatePerformanceUI()
	{
		float flightTime = Time.time - rocketController.FlightStartTime;

		float altitudeEarnings = maxAltitude * altitudeMultiplier;
		float speedEarnings = maxSpeed * speedMultiplier;
		float timeEarnings = flightTime * timeMultiplier;
		float distanceEarnings = distanceTravelled * distanceMultiplier;

		maxAltitudeText.text = $"Max Altitude: {maxAltitude:F0} ft (<color=orange>${altitudeEarnings:F0}</color>)";
		maxSpeedText.text = $"Max Speed: {maxSpeed:F0} MPH (<color=orange>${speedEarnings:F0}</color>)";
		flightTimeText.text = $"Flight Time: {flightTime:F0} s (<color=orange>${timeEarnings:F0}</color>)";
		distanceTravelledText.text = $"Distance Travelled: {distanceTravelled:F0} ft (<color=orange>${distanceEarnings:F0}</color>)";

		float totalEarnings = altitudeEarnings + speedEarnings + timeEarnings + distanceEarnings;
		totalMoneyEarned += totalEarnings;
		moneyEarnedText.text = $"Total Money Earned: <color=orange>${totalEarnings:F0}</color>";

		upgradeManager.UpdateCurrentMoneyText();
	}

	private float CalculateMoneyEarned(float altitude, float speed, float time, float distance)
	{
		return (altitude * altitudeMultiplier) +
			(speed * speedMultiplier) +
			(time * timeMultiplier) +
			(distance * distanceMultiplier);
	}

	public void OpenUpgradeMenu()
	{
		uiStateManager.SetState(UIStateManager.UIState.UpgradeUI);
		upgradeManager.UpdateCurrentMoneyText();
	}

	public void CloseUpgradeMenu()
	{
		uiStateManager.SetState(UIStateManager.UIState.GameplayUI);
		rocketController.ResetRocket(initialPosition);
		if (cameraController != null)
		{
			cameraController.ResetCamera();
		}
		ResetFlightStats();
		windManager.GenerateNewWind();

		if (blurManager != null)
		{
			blurManager.DisableBlur();
		}

		if (musicManager != null)
		{
			musicManager.StartGameplayMusic();
		}
	}

	private void ResetFlightStats()
	{
		maxAltitude = 0f;
		maxSpeed = 0f;
		distanceTravelled = 0f;
		hasWon = false;
	}

	public float GetTotalMoneyEarned()
	{
		return totalMoneyEarned;
	}

	public void SpendMoney(float amount)
	{
		if (amount <= totalMoneyEarned)
		{
			totalMoneyEarned -= amount;
			upgradeManager.UpdateCurrentMoneyText();
		}
		else
		{
			Debug.LogWarning("Not enough money!");
		}
	}

	public void ResetGame()
	{
		totalMoneyEarned = 0f;
		ResetFlightStats();
		rocketController.ResetRocket(initialPosition);
		if (cameraController != null)
		{
			cameraController.ResetCamera();
		}
		upgradeManager.ResetUpgrades();

		uiStateManager.SetState(UIStateManager.UIState.GameplayUI);
		windManager.GenerateNewWind();
	}

	private void TogglePause()
	{
		isPaused = !isPaused;

		if (isPaused)
		{
			uiSoundSystem.PlayMenuOpenSound();
			cameraController.controlsEnabled = false;
			uiStateManager.SetState(UIStateManager.UIState.PauseUI);
		}
		else
		{
			uiSoundSystem.PlayMenuCloseSound();
			cameraController.controlsEnabled = true;
			uiStateManager.ReturnFromPause();
		}

		Time.timeScale = isPaused ? 0f : 1f;

		if (rocketController != null)
		{
			rocketController.enabled = !isPaused;
		}
	}

	private void ResumeGame()
	{
		TogglePause();
	}

	private void QuitGame()
	{
		#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
		#else
			Application.Quit();
		#endif
	}
}
