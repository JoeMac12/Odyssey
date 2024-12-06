using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderSoundController : MonoBehaviour
{
	public AudioSource audioSource;
	public AudioClip[] thunderSounds;
	public float volume = 1f;

	private void Start()
	{
		if (audioSource == null)
		{
			audioSource = GetComponent<AudioSource>();
		}

		if (audioSource == null)
		{
			enabled = false;
			return;
		}

		audioSource.volume = volume;
	}

	public void PlayThunderSound()
	{
		if (thunderSounds == null || thunderSounds.Length == 0)
		{
			return;
		}

		int randomIndex = Random.Range(0, thunderSounds.Length);
		audioSource.clip = thunderSounds[randomIndex];
		audioSource.Play();
	}
}
