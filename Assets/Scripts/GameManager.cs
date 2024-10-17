using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public RocketController rocketController;
	public UpgradeManager upgradeManager;
	public GameObject performancePanel;
	public GameObject upgradePanel;
	public float panelFadeDuration = 1f;

	[Header("Performance UI")]
	public TMP_Text maxAltitudeText;
	public TMP_Text maxSpeedText;
	public TMP_Text flightTimeText;
	public TMP_Text distanceTravelledText;
	public TMP_Text moneyEarnedText;
	public Button openUpgradeMenuButton;
	public Button closeUpgradeMenuButton;

	[Header("Pause Menu")]
	public GameObject pauseMenuPanel;
	public Button resumeButton;
	public Button mainMenuButton;

	private bool isPaused = false;

	[Header("Money Multipliers")]
	public float altitudeMultiplier = 0.1f;
	public float speedMultiplier = 0.5f;
	public float timeMultiplier = 10f;
	public float distanceMultiplier = 0.2f;

	private Vector3 initialPosition;
	private float maxAltitude;
	private float maxSpeed;
	private float distanceTravelled;
	private float totalMoneyEarned = 0f;

	// This should be right I think?
	private const float metersToFeet = 3.28084f;
	private const float metersToMPH = 2.23694f;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		initialPosition = rocketController.transform.position;
		performancePanel.SetActive(false);
		upgradePanel.SetActive(false);
		ResetFlightStats();

		openUpgradeMenuButton.onClick.AddListener(OpenUpgradeMenu);
		closeUpgradeMenuButton.onClick.AddListener(CloseUpgradeMenu);

		pauseMenuPanel.SetActive(false);
		resumeButton.onClick.AddListener(ResumeGame);
		mainMenuButton.onClick.AddListener(LoadMainMenu);
	}

	private void Update()
	{
		if (!rocketController.IsExploded)
		{
			UpdateFlightStats();
		}

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			TogglePause();
		}
	}

	private void UpdateFlightStats()
	{
		float currentAltitude = (rocketController.transform.position.y - initialPosition.y) * metersToFeet;
		maxAltitude = Mathf.Max(maxAltitude, currentAltitude);

		float currentSpeed = rocketController.rb.velocity.magnitude * metersToMPH;
		maxSpeed = Mathf.Max(maxSpeed, currentSpeed);

		distanceTravelled = Vector3.Distance(initialPosition, rocketController.transform.position) * metersToFeet;
	}

	public void OnRocketExploded()
	{
		StartCoroutine(ShowPerformancePanel());
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
	}

	private void ResetFlightStats()
	{
		maxAltitude = 0f;
		maxSpeed = 0f;
		distanceTravelled = 0f;
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

	// This will be used to reset the whole game but we don't need that right now
	public void ResetGame()
	{
		totalMoneyEarned = 0f;
		ResetFlightStats();
		rocketController.ResetRocket(initialPosition);
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

	private void LoadMainMenu()
	{
		Time.timeScale = 1f;
		SceneManager.LoadScene("MainMenu");
	}
}
