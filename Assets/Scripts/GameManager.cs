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

	[Header("UI Panels")]
	public GameObject rocketUI;
	public GameObject weatherUI;
	public GameObject rocketPanelUI;
	public GameObject performancePanel;
	public GameObject upgradePanel;
	public GameObject pauseMenuPanel;
	public GameObject winPanel;
	public float panelFadeDuration = 1f;

	[Header("Performance UI")]
	public TMP_Text maxAltitudeText;
	public TMP_Text maxSpeedText;
	public TMP_Text flightTimeText;
	public TMP_Text distanceTravelledText;
	public TMP_Text moneyEarnedText;
	public Button openUpgradeMenuButton;
	public Button closeUpgradeMenuButton;
	public Button selfDestructButton;

	[Header("Pause Menu")]
	public Button resumeButton;
	public Button mainMenuButton;
	public Button quitGameButton;

	[Header("Win Condition")]
	public float winAltitude = 10000f;
	public Button resetGameButton;

	private bool isPaused = false;
	private bool hasWon = false;

	[Header("Money Multipliers")]
	public float altitudeMultiplier = 0.1f;
	public float speedMultiplier = 0.5f;
	public float timeMultiplier = 10f;
	public float distanceMultiplier = 0.2f;

	[Header("Effects")]
	public BlurEffectManager blurManager;

	private Vector3 initialPosition;
	private float maxAltitude;
	private float maxSpeed;
	private float distanceTravelled;
	private float totalMoneyEarned = 0f;

	private const float metersToFeet = 3.28084f;
	private const float metersToMPH = 2.23694f;

	private void Start()
	{
		initialPosition = rocketController.transform.position;
		windManager.Initialize(rocketController);
		SetUIState();
		ResetFlightStats();

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
		rocketUI.SetActive(true);
		weatherUI.SetActive(true);
		rocketPanelUI.SetActive(true);
		performancePanel.SetActive(false);
		upgradePanel.SetActive(false);
		pauseMenuPanel.SetActive(false);
		winPanel.SetActive(false);
	}

	private void Update()
	{
		if (!rocketController.IsExploded && !hasWon)
		{
			UpdateFlightStats();
			CheckWinCondition();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
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
			StartCoroutine(ShowWinPanel());
		}
	}

	private IEnumerator ShowWinPanel()
	{
		winPanel.SetActive(true);
		CanvasGroup canvasGroup = winPanel.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0;

		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	public void OnRocketExploded()
	{
		if (blurManager != null)
		{
			blurManager.EnableBlur();
		}

		StartCoroutine(FadeOutGameplayUI());

		if (!hasWon)
		{
			StartCoroutine(ShowPerformancePanel());
		}
	}

	private void SelfDestructRocket()
	{
		if (rocketController != null && !rocketController.IsExploded)
		{
			rocketController.SelfDestruct();
		}
	}

	private IEnumerator FadeOutGameplayUI()
	{
		CanvasGroup rocketUIGroup = rocketUI.GetComponent<CanvasGroup>();
		if (rocketUIGroup == null) rocketUIGroup = rocketUI.AddComponent<CanvasGroup>();

		CanvasGroup weatherUIGroup = weatherUI.GetComponent<CanvasGroup>();
		if (weatherUIGroup == null) weatherUIGroup = weatherUI.AddComponent<CanvasGroup>();

		CanvasGroup rocketPanelUIGroup = rocketPanelUI.GetComponent<CanvasGroup>();
		if (rocketPanelUIGroup == null) rocketPanelUIGroup = rocketPanelUI.AddComponent<CanvasGroup>();

		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			float alpha = Mathf.Lerp(1f, 0f, elapsedTime / panelFadeDuration);
			rocketUIGroup.alpha = alpha;
			weatherUIGroup.alpha = alpha;
			rocketPanelUIGroup.alpha = alpha;
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		rocketUI.SetActive(false);
		weatherUI.SetActive(false);
		rocketPanelUI.SetActive(false);
	}

	private IEnumerator ShowPerformancePanel()
	{
		yield return new WaitForSeconds(0f);

		performancePanel.SetActive(true);
		CanvasGroup canvasGroup = performancePanel.GetComponent<CanvasGroup>();
		StartCoroutine(FadeInPanel(canvasGroup));

		UpdatePerformanceUI();
	}

	private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
	{
		canvasGroup.alpha = 0f;
		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}

	private void UpdatePerformanceUI()
	{
		float flightTime = Time.time - rocketController.FlightStartTime;

		maxAltitudeText.text = $"Max Altitude: {maxAltitude:F2} ft";
		maxSpeedText.text = $"Max Speed: {maxSpeed:F2} MPH";
		flightTimeText.text = $"Flight Time: {flightTime:F2} s";
		distanceTravelledText.text = $"Distance Travelled: {distanceTravelled:F2} ft";

		float moneyEarned = CalculateMoneyEarned(maxAltitude, maxSpeed, flightTime, distanceTravelled);
		totalMoneyEarned += moneyEarned;
		moneyEarnedText.text = $"Money Earned: ${moneyEarned:F2}";

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
		performancePanel.SetActive(false);
		upgradePanel.SetActive(true);
		upgradeManager.UpdateCurrentMoneyText();
	}

	public void CloseUpgradeMenu()
	{
		upgradePanel.SetActive(false);
		rocketController.ResetRocket(initialPosition);
		ResetFlightStats();
		windManager.GenerateNewWind();

		if (blurManager != null)
		{
			blurManager.DisableBlur();
		}

		StartCoroutine(FadeInGameplayUI());
	}

	private IEnumerator FadeInGameplayUI()
	{
		rocketUI.SetActive(true);
		weatherUI.SetActive(true);
		rocketPanelUI.SetActive(true);

		CanvasGroup rocketUIGroup = rocketUI.GetComponent<CanvasGroup>();
		CanvasGroup weatherUIGroup = weatherUI.GetComponent<CanvasGroup>();
		CanvasGroup rocketPanelUIGroup = rocketPanelUI.GetComponent<CanvasGroup>();

		float elapsedTime = 0f;
		while (elapsedTime < panelFadeDuration)
		{
			float alpha = Mathf.Lerp(0f, 1f, elapsedTime / panelFadeDuration);
			rocketUIGroup.alpha = alpha;
			weatherUIGroup.alpha = alpha;
			rocketPanelUIGroup.alpha = alpha;
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		rocketUIGroup.alpha = 1f;
		weatherUIGroup.alpha = 1f;
		rocketPanelUIGroup.alpha = 1f;
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
		upgradeManager.ResetUpgrades();
		winPanel.SetActive(false);

		windManager.GenerateNewWind();

		StartCoroutine(FadeInGameplayUI());
	}

	private void TogglePause()
	{
		isPaused = !isPaused;
		pauseMenuPanel.SetActive(isPaused);
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
