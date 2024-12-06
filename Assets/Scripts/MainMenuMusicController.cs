using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuMusicController : MonoBehaviour
{
	public AudioSource menuMusic;
	public Slider volumeSlider;
	public TMP_Text volumeText;

	private void Start()
	{
		if (menuMusic == null || volumeSlider == null)
		{
			enabled = false;
			return;
		}

		volumeSlider.value = menuMusic.volume;
		SetupVolumeSlider();
		UpdateVolumeText(menuMusic.volume);
	}

	private void SetupVolumeSlider()
	{
		volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
	}

	private void OnVolumeChanged(float volume)
	{
		menuMusic.volume = volume;
		UpdateVolumeText(volume);
	}

	private void UpdateVolumeText(float volume)
	{
		if (volumeText != null)
		{
			volumeText.text = $"Music Volume: {(volume * 100):F0}%";
		}
	}
}
