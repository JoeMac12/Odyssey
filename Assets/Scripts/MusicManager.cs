using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	[System.Serializable]
	public class MusicTrack
	{
		public string name;
		public AudioClip clip;
		[Range(0f, 1f)]
		public float volume = 1f;
	}

	[Header("Music Tracks")]
	public MusicTrack gameplayMusic;
	public MusicTrack interfaceMusic;

	[Header("Transition Settings")]
	public float crossFadeDuration = 1.5f;
	public float maxVolume = 0.8f;
	public float pauseMulti = 0.25f;

	private AudioSource gameplaySource;
	private AudioSource interfaceSource;
	private bool isFading = false;

	private float targetGameplayVolume = 0f;
	private float targetInterfaceVolume = 0f;
	private Coroutine volTime;

	private void Awake()
	{
		gameplaySource = gameObject.AddComponent<AudioSource>();
		interfaceSource = gameObject.AddComponent<AudioSource>();

		SetupAuto(gameplaySource, gameplayMusic);
		SetupAuto(interfaceSource, interfaceMusic);

		gameplaySource.volume = 0;
		interfaceSource.volume = 0;
	}

	private void SetupAuto(AudioSource source, MusicTrack track)
	{
		if (track != null && track.clip != null)
		{
			source.clip = track.clip;
			source.loop = true;
			source.playOnAwake = false;
			source.volume = 0;
		}
	}

	public void StartGameplayMusic()
	{
		if (!gameplaySource.isPlaying)
		{
			gameplaySource.Play();
		}
		CrossFadeToGameplay();
	}

	public void StartInterfaceMusic()
	{
		if (!interfaceSource.isPlaying)
		{
			interfaceSource.Play();
		}
		CrossFadeToInterface();
	}

	private void CrossFadeToGameplay()
	{
		if (isFading)
		{
			StopAllCoroutines();
		}
		StartCoroutine(CrossFade(interfaceSource, gameplaySource));
	}

	private void CrossFadeToInterface()
	{
		if (isFading)
		{
			StopAllCoroutines();
		}
		StartCoroutine(CrossFade(gameplaySource, interfaceSource));
	}

	public void AdjustMusicVolume(bool isPaused)
	{
		if (volTime != null)
		{
			StopCoroutine(volTime);
		}
		volTime = StartCoroutine(AdjustVolume(isPaused));
	}

	private IEnumerator AdjustVolume(bool isPaused)
	{
		float startGameplayVolume = gameplaySource.volume;
		float startInterfaceVolume = interfaceSource.volume;

		float targetMultiplier = isPaused ? pauseMulti : 1f;
		float targetGameplay = targetGameplayVolume * targetMultiplier;
		float targetInterface = targetInterfaceVolume * targetMultiplier;

		float elapsed = 0f;
		float duration = 0.25f;

		while (elapsed < duration)
		{
			elapsed += Time.unscaledDeltaTime;
			float t = elapsed / duration;

			gameplaySource.volume = Mathf.Lerp(startGameplayVolume, targetGameplay, t);
			interfaceSource.volume = Mathf.Lerp(startInterfaceVolume, targetInterface, t);

			yield return null;
		}

		gameplaySource.volume = targetGameplay;
		interfaceSource.volume = targetInterface;
	}

	private IEnumerator CrossFade(AudioSource fadeOutSource, AudioSource fadeInSource)
	{
		isFading = true;
		float startTime = Time.time;
		float initialFadeOutVolume = fadeOutSource.volume;
		float initialFadeInVolume = fadeInSource.volume;
		float targetVolume = maxVolume * (fadeInSource == gameplaySource ? gameplayMusic.volume : interfaceMusic.volume);

		if (fadeInSource == gameplaySource)
		{
			targetGameplayVolume = targetVolume;
			targetInterfaceVolume = 0f;
		}
		else
		{
			targetGameplayVolume = 0f;
			targetInterfaceVolume = targetVolume;
		}

		while (Time.time - startTime < crossFadeDuration)
		{
			float t = (Time.time - startTime) / crossFadeDuration;
			fadeOutSource.volume = Mathf.Lerp(initialFadeOutVolume, 0f, t);
			fadeInSource.volume = Mathf.Lerp(initialFadeInVolume, targetVolume, t);
			yield return null;
		}

		fadeOutSource.volume = 0f;
		fadeInSource.volume = targetVolume;
		isFading = false;
	}

	public void StopAllMusic()
	{
		StopAllCoroutines();
		StartCoroutine(FadeOutAll());
	}

	private IEnumerator FadeOutAll()
	{
		isFading = true;
		float startTime = Time.time;
		float initialGameplayVolume = gameplaySource.volume;
		float initialInterfaceVolume = interfaceSource.volume;

		while (Time.time - startTime < crossFadeDuration)
		{
			float t = (Time.time - startTime) / crossFadeDuration;
			gameplaySource.volume = Mathf.Lerp(initialGameplayVolume, 0f, t);
			interfaceSource.volume = Mathf.Lerp(initialInterfaceVolume, 0f, t);
			yield return null;
		}

		gameplaySource.volume = 0f;
		interfaceSource.volume = 0f;
		gameplaySource.Stop();
		interfaceSource.Stop();
		isFading = false;
	}
}
