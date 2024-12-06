using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
	public static LevelManager Instance;
	public GameObject loadingScreen;
	public GameObject mainMenuUI;
	public Image loadingBar;
	public TMP_Text loadingText;
	public float fadeDuration = 1f;

	[Header("Credits")]
	public GameObject creditsUI;
	public Button creditsButton;
	public Button closeCreditsButton;

	[Header("Options")]
	public GameObject optionsUI;
	public Button optionsButton;
	public Button closeOptionsButton;

	void Awake()
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

	void Start()
	{
		loadingScreen.SetActive(false);
		UpdateMainMenu();
		SetupCreditsButtons();
		SetupOptionsButtons();
	}

	void SetupCreditsButtons()
	{
		if (creditsButton != null)
		{
			creditsButton.onClick.AddListener(ShowCredits);
		}

		if (closeCreditsButton != null)
		{
			closeCreditsButton.onClick.AddListener(HideCredits);
		}

		if (creditsUI != null)
		{
			creditsUI.SetActive(false);
		}
	}

	void SetupOptionsButtons()
	{
		if (optionsButton != null)
		{
			optionsButton.onClick.AddListener(ShowOptions);
		}

		if (closeOptionsButton != null)
		{
			closeOptionsButton.onClick.AddListener(HideOptions);
		}

		if (optionsUI != null)
		{
			optionsUI.SetActive(false);
		}
	}

	void ShowCredits()
	{
		if (creditsUI != null)
		{
			creditsUI.SetActive(true);
			mainMenuUI.SetActive(false);
			if (optionsUI != null)
			{
				optionsUI.SetActive(false);
			}
		}
	}

	void HideCredits()
	{
		if (creditsUI != null)
		{
			creditsUI.SetActive(false);
			mainMenuUI.SetActive(true);
		}
	}

	void ShowOptions()
	{
		if (optionsUI != null)
		{
			optionsUI.SetActive(true);
			mainMenuUI.SetActive(false);
			if (creditsUI != null)
			{
				creditsUI.SetActive(false);
			}
		}
	}

	void HideOptions()
	{
		if (optionsUI != null)
		{
			optionsUI.SetActive(false);
			mainMenuUI.SetActive(true);
		}
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		UpdateMainMenu();
	}

	void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void UpdateMainMenu()
	{
		if (mainMenuUI != null)
		{
			bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
			mainMenuUI.SetActive(isMainMenu);
			if (creditsUI != null)
			{
				creditsUI.SetActive(false);
			}
			if (optionsUI != null)
			{
				optionsUI.SetActive(false);
			}
		}
	}

	public void PlayGame()
	{
		StartCoroutine(LoadSceneAsync("Gameplay"));
	}

	IEnumerator LoadSceneAsync(string sceneName)
	{
		loadingScreen.SetActive(true);
		CanvasGroup canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0;

		float time = 0f;
		while (time < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(0, 1, time / fadeDuration);
			time += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 1;

		AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

		while (!operation.isDone)
		{
			float progress = Mathf.Clamp01(operation.progress / 0.9f);
			loadingBar.fillAmount = progress;
			yield return null;
		}

		time = 0f;
		while (time < fadeDuration)
		{
			canvasGroup.alpha = Mathf.Lerp(1, 0, time / fadeDuration);
			time += Time.deltaTime;
			yield return null;
		}
		canvasGroup.alpha = 0;

		loadingScreen.SetActive(false);
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
