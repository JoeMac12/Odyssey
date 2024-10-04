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
	public Image loadingBar;
	public TMP_Text loadingText;
	public float fadeDuration = 1f;

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
