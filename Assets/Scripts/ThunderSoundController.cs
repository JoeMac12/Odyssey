using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is some cursed code. i'll fix it another time
public class ThunderSoundController : MonoBehaviour
{
	public AudioClip[] thunderSounds;
	public float volume = 1f;
	public int maxSounds = 3;

	private Queue<AudioSource> availableAudioSources;
	private List<AudioSource> activeAudioSources;

	private void Start()
	{
		InitializeAudioSources();
	}

	private void InitializeAudioSources()
	{
		availableAudioSources = new Queue<AudioSource>();
		activeAudioSources = new List<AudioSource>();

		for (int i = 0; i < maxSounds; i++)
		{
			AudioSource source = gameObject.AddComponent<AudioSource>();
			source.volume = volume;
			source.playOnAwake = false;
			availableAudioSources.Enqueue(source);
		}
	}

	private void Update()
	{
		for (int i = activeAudioSources.Count - 1; i >= 0; i--)
		{
			if (!activeAudioSources[i].isPlaying)
			{
				availableAudioSources.Enqueue(activeAudioSources[i]);
				activeAudioSources.RemoveAt(i);
			}
		}
	}

	public void PlayThunderSound()
	{
		if (thunderSounds == null || thunderSounds.Length == 0 || availableAudioSources.Count == 0)
		{
			return;
		}

		AudioSource audioSource = availableAudioSources.Dequeue();
		audioSource.clip = thunderSounds[Random.Range(0, thunderSounds.Length)];
		audioSource.Play();
		activeAudioSources.Add(audioSource);
	}
}
