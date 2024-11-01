using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISoundSystem : MonoBehaviour
{
	[System.Serializable]
	public class SoundEffect
	{
		public string name;
		public AudioClip clip;
		[Range(0f, 1f)]
		public float volume = 1f;
		[Range(0.1f, 3f)]
		public float pitch = 1f;
	}

	[Header("Audio Source")]
	public AudioSource audioSource;

	[Header("UI Sound Effects")]
	public SoundEffect buttonClick;
	public SoundEffect buttonHover;
	public SoundEffect upgradeSuccess;
	public SoundEffect upgradeFail;
	public SoundEffect menuOpen;
	public SoundEffect menuClose;

	[Header("Settings")]
	[Range(0f, 1f)]
	public float masterVolume = 1f;
	public bool enableHoverSounds = true;

	private void Awake()
	{
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		audioSource.playOnAwake = false;

		SetupUIElements();
	}

	private void SetupUIElements()
	{
		Button[] buttons = FindObjectsOfType<Button>(true);
		foreach (Button button in buttons)
		{
			SetupButton(button);
		}
	}

	private void SetupButton(Button button)
	{
		button.onClick.AddListener(() => PlayButtonClickSound());

		EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
		if (eventTrigger == null)
		{
			eventTrigger = button.gameObject.AddComponent<EventTrigger>();
		}

		EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
		hoverEntry.eventID = EventTriggerType.PointerEnter;
		hoverEntry.callback.AddListener((data) => PlayButtonHoverSound());
		eventTrigger.triggers.Add(hoverEntry);
	}

	private void PlaySound(SoundEffect sound)
	{
		if (sound?.clip == null || audioSource == null) return;

		audioSource.pitch = sound.pitch;
		audioSource.PlayOneShot(sound.clip, sound.volume * masterVolume);
	}

	public void PlayButtonClickSound()
	{
		PlaySound(buttonClick);
	}

	public void PlayButtonHoverSound()
	{
		if (enableHoverSounds)
		{
			PlaySound(buttonHover);
		}
	}

	public void PlayUpgradeSuccessSound()
	{
		PlaySound(upgradeSuccess);
	}

	public void PlayUpgradeFailSound()
	{
		PlaySound(upgradeFail);
	}

	public void PlayMenuOpenSound()
	{
		PlaySound(menuOpen);
	}

	public void PlayMenuCloseSound()
	{
		PlaySound(menuClose);
	}

	public void SetupNewButton(Button button)
	{
		SetupButton(button);
	}

	public void SetMasterVolume(float volume)
	{
		masterVolume = Mathf.Clamp01(volume);
	}

	public void ToggleHoverSounds(bool enable)
	{
		enableHoverSounds = enable;
	}
}
