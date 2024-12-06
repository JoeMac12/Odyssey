using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderstormManager : MonoBehaviour
{
	public RocketController rocketController;

	[Header("Height Settings")]
	public float stormStartHeight = 1000f;
	public float maxStormHeight = 5000f;

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

	private ParticleSystem.EmissionModule rainEmission;
	private float currentFogDensity;
	private Color currentFogColor;

	private void Start()
	{
		SetupRain();
		SetupFog();
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
		UpdateRainSystem(currentHeight);
		UpdateFogSystem(currentHeight);
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
}
