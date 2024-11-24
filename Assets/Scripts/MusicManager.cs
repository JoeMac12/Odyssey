using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
	public MusicData musicData;

	private AudioSource gameplaySource;
	private AudioSource interfaceSource;
	private bool isFading = false;

	private float targetGameplayVolume = 0f;
	private float targetInterfaceVolume = 0f;
	private float masterMusicVolume = 1f;

	private void Awake()
	{
		if (musicData == null)
		{
			enabled = false;
			return;
		}

		gameplaySource = gameObject.AddComponent<AudioSource>();
		interfaceSource = gameObject.AddComponent<AudioSource>();

		SetupAudio(gameplaySource, musicData.gameplayMusic);
		SetupAudio(interfaceSource, musicData.interfaceMusic);

		PreloadAudio(gameplaySource);
		PreloadAudio(interfaceSource);

		gameplaySource.volume = 0;
		interfaceSource.volume = 0;
	}

	private void SetupAudio(AudioSource source, MusicData.MusicTrack track)
	{
		if (track != null && track.clip != null)
		{
			source.clip = track.clip;
			source.loop = true;
			source.playOnAwake = false;
			source.volume = 0;
			source.clip.LoadAudioData();
		}
	}

	private void PreloadAudio(AudioSource source)
	{
		if (source != null && source.clip != null)
		{
			source.Play();
			source.Pause();
			source.time = 0;
		}
	}

	public void StartGameplayMusic()
	{
		if (!gameplaySource.isPlaying)
		{
			gameplaySource.UnPause();
		}
		CrossFadeToGameplay();
	}

	public void StartInterfaceMusic()
	{
		if (!interfaceSource.isPlaying)
		{
			interfaceSource.UnPause();
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

	public void SetMasterMusicVolume(float volume)
	{
		masterMusicVolume = Mathf.Clamp01(volume);
		UpdateSourceVolumes();
	}

	private void UpdateSourceVolumes()
	{
		if (gameplaySource != null)
		{
			gameplaySource.volume = targetGameplayVolume * masterMusicVolume;
		}
		if (interfaceSource != null)
		{
			interfaceSource.volume = targetInterfaceVolume * masterMusicVolume;
		}
	}

	private IEnumerator CrossFade(AudioSource fadeOutSource, AudioSource fadeInSource)
	{
		isFading = true;
		float startTime = Time.time;
		float initialFadeOutVolume = fadeOutSource.volume;
		float initialFadeInVolume = fadeInSource.volume;
		float targetVolume = musicData.maxVolume *
			(fadeInSource == gameplaySource ? musicData.gameplayMusic.volume : musicData.interfaceMusic.volume) *
			masterMusicVolume;

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

		while (Time.time - startTime < musicData.crossFadeDuration)
		{
			float t = (Time.time - startTime) / musicData.crossFadeDuration;
			fadeOutSource.volume = Mathf.Lerp(initialFadeOutVolume, 0f, t);
			fadeInSource.volume = Mathf.Lerp(initialFadeInVolume, targetVolume, t);
			yield return null;
		}

		fadeOutSource.volume = 0f;
		fadeInSource.volume = targetVolume;
		fadeOutSource.Pause();
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

		while (Time.time - startTime < musicData.crossFadeDuration)
		{
			float t = (Time.time - startTime) / musicData.crossFadeDuration;
			gameplaySource.volume = Mathf.Lerp(initialGameplayVolume, 0f, t);
			interfaceSource.volume = Mathf.Lerp(initialInterfaceVolume, 0f, t);
			yield return null;
		}

		gameplaySource.volume = 0f;
		interfaceSource.volume = 0f;
		gameplaySource.Pause();
		interfaceSource.Pause();
		isFading = false;
	}

	public float GetMasterMusicVolume()
	{
		return masterMusicVolume;
	}
}
