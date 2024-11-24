using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderstormManager : MonoBehaviour
{
	public RocketController rocketController;

	[Header("Height Settings")]
	public float stormStartHeight = 1000f;
	public float maxStormHeight = 5000f;

	[Header("Lightning Chance Settings")]
	public float minStrikeInterval = 3f;
	public float maxStrikeInterval = 7f;
	public float baseStrikeChance = 0.08f;
	public float maxStrikeChance = 0.25f;

	[Header("Lightning Damage Settings")]
	public float minDamage = 5f;
	public float maxDamage = 15f;
	public float minRotationForce = 100f;
	public float maxRotationForce = 300f;

	[Header("Lighting Effects")]
	public GameObject lightningParticlePrefab;
	public float lightningEffectTime = 0.5f;
	public float strikeRadius = 10f;
	public Vector2 heightOfLightning = new Vector2(-5f, 5f);

	[Header("Rain Settings")]
	public ParticleSystem rainParticleSystem;
	public float baseEmissionRate = 100f;
	public float maxEmissionRate = 1000f;
	public float rainStartHeight = 500f;

	[Header("Fog Settings")]
	public Color baseFogColor = new Color(0.5f, 0.5f, 0.5f, 1f);
	public Color stormFogColor = new Color(0.2f, 0.2f, 0.3f, 1f);
	public float baseFogDensity = 0.001f;
	public float maxFogDensity = 0.01f;
	public float fogChangeSpeed = 1f;

	[Header("Audio")]
	public AudioClip[] thunderSounds;
	public AudioSource thunderAudioSource;
	[Range(0f, 1f)]
	public float minThunderVolume = 0.8f;
	[Range(0f, 1f)]
	public float maxThunderVolume = 1f;
	public float minThunderDelay = 0.1f;
	public float maxThunderDelay = 0.5f;

	private float nextStrikeTime;
	private bool isInThunderstorm;
	private ParticleSystem.EmissionModule rainEmission;
	private float currentFogDensity;
	private Color currentFogColor;

	private void Start()
	{
		SetupRain();
		SetupFog();
		SetupAudio();
		SetNextStrikeTime();
	}

	private void SetupAudio()
	{
		if (thunderAudioSource == null)
		{
			thunderAudioSource = gameObject.AddComponent<AudioSource>();
			thunderAudioSource.playOnAwake = false;
			thunderAudioSource.spatialBlend = 1f;
			thunderAudioSource.minDistance = 5f;
			thunderAudioSource.maxDistance = 100f;
		}
	}

	private void SetupRain()
	{
		if (rainParticleSystem != null)
		{
			rainEmission = rainParticleSystem.emission;
			rainEmission.rateOverTime = 0;

			var main = rainParticleSystem.main;
			main.simulationSpace = ParticleSystemSimulationSpace.World;
		}
	}

	private void SetupFog()
	{
		RenderSettings.fog = true;
		currentFogDensity = baseFogDensity;
		currentFogColor = baseFogColor;
		RenderSettings.fogMode = FogMode.Exponential;
		RenderSettings.fogDensity = currentFogDensity;
		RenderSettings.fogColor = currentFogColor;
	}

	private void Update()
	{
		if (rocketController == null || rocketController.IsExploded) return;

		float currentHeight = rocketController.transform.position.y;
		isInThunderstorm = currentHeight >= stormStartHeight;

		UpdateRainSystem(currentHeight);
		UpdateFogSystem(currentHeight);

		if (isInThunderstorm && Time.time >= nextStrikeTime)
		{
			CheckForLightningStrike(currentHeight);
			SetNextStrikeTime();
		}
	}

	private void UpdateRainSystem(float currentHeight)
	{
		if (rainParticleSystem != null)
		{
			rainParticleSystem.transform.rotation = Quaternion.identity;

			float rainProgress = Mathf.InverseLerp(rainStartHeight, maxStormHeight, currentHeight);
			float targetEmissionRate = Mathf.Lerp(baseEmissionRate, maxEmissionRate, rainProgress);

			var currentRate = rainEmission.rateOverTime;
			float newRate = Mathf.Lerp(currentRate.constant, targetEmissionRate, Time.deltaTime * 2f);
			rainEmission.rateOverTime = newRate;

			var main = rainParticleSystem.main;
			main.startSpeedMultiplier = Mathf.Lerp(10f, 30f, rainProgress);
		}
	}

	private void UpdateFogSystem(float currentHeight)
	{
		float fogProgress = Mathf.InverseLerp(rainStartHeight, maxStormHeight, currentHeight);

		float targetFogDensity = Mathf.Lerp(baseFogDensity, maxFogDensity, fogProgress);
		Color targetFogColor = Color.Lerp(baseFogColor, stormFogColor, fogProgress);

		currentFogDensity = Mathf.Lerp(currentFogDensity, targetFogDensity, Time.deltaTime * fogChangeSpeed);
		currentFogColor = Color.Lerp(currentFogColor, targetFogColor, Time.deltaTime * fogChangeSpeed);

		RenderSettings.fogDensity = currentFogDensity;
		RenderSettings.fogColor = currentFogColor;
	}

	private void CheckForLightningStrike(float currentHeight)
	{
		float heightProgress = Mathf.InverseLerp(stormStartHeight, maxStormHeight, currentHeight);
		float strikeChance = Mathf.Lerp(baseStrikeChance, maxStrikeChance, heightProgress);

		if (Random.value <= strikeChance)
		{
			StartCoroutine(StrikeLightning());
		}
	}

	private void SetNextStrikeTime()
	{
		nextStrikeTime = Time.time + Random.Range(minStrikeInterval, maxStrikeInterval);
	}

	private void PlayRandomThunderSound()
	{
		if (thunderSounds == null || thunderSounds.Length == 0 || thunderAudioSource == null) return;

		int randomIndex = Random.Range(0, thunderSounds.Length);
		AudioClip randomThunder = thunderSounds[randomIndex];

		if (randomThunder != null)
		{
			thunderAudioSource.clip = randomThunder;
			thunderAudioSource.pitch = Random.Range(0.9f, 1.1f);
			thunderAudioSource.volume = Random.Range(minThunderVolume, maxThunderVolume);
			thunderAudioSource.Play();
		}
	}

	private IEnumerator StrikeLightning()
	{
		Vector2 randomCircle = Random.insideUnitCircle * strikeRadius;
		Vector3 strikePosition = rocketController.transform.position +
			new Vector3(randomCircle.x, Random.Range(heightOfLightning.x, heightOfLightning.y), randomCircle.y);

		if (lightningParticlePrefab != null)
		{
			Quaternion effectRotation = Quaternion.Euler(
				Random.Range(0f, 360f),
				Random.Range(0f, 360f),
				Random.Range(0f, 360f)
			);

			GameObject lightningEffect = Instantiate(lightningParticlePrefab,
				strikePosition,
				effectRotation);

			Destroy(lightningEffect, lightningEffectTime);
		}

		float damage = Random.Range(minDamage, maxDamage);
		rocketController.TakeDamage(damage);

		Vector3 torqueRotation = new Vector3(
			Random.Range(-1f, 1f),
			Random.Range(-1f, 1f),
			Random.Range(-1f, 1f)
		);

		float rotationForce = Random.Range(minRotationForce, maxRotationForce);
		rocketController.rb.AddTorque(torqueRotation * rotationForce, ForceMode.Impulse);

		float thunderDelay = Random.Range(minThunderDelay, maxThunderDelay);
		yield return new WaitForSeconds(thunderDelay);

		PlayRandomThunderSound();
	}
}
