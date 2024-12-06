using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindManager : MonoBehaviour
{
	public TMP_Text windDirectionText;
	public TMP_Text windSpeedText;

	public float minWindSpeed = 2f;
	public float maxWindSpeed = 18f;
	public float altitudeWindMultiplier = 0.001f;

	public float minGustInterval = 1.5f;
	public float maxGustInterval = 4f;
	public float gustChance = 0.3f;
	public float minGustTime = 0.5f;
	public float maxGustTime = 1.5f;
	public float minGustRotationForce = 50f;
	public float maxGustRotationForce = 150f;
	public float gustModifier = 0.8f;

	private float baseWindSpeed;
	private Vector3 windDirection;
	private RocketController rocket;
	private float nextGustTime;
	private bool isGusting;
	private Vector3 currentGustRotation;
	private float gustEndTime;

	private readonly string[] windText = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
										"S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

	public void Initialize(RocketController rocketController)
	{
		rocket = rocketController;
		GenerateNewWind();
		SetNextGustTime();
	}

	void FixedUpdate()
	{
		if (rocket != null && !rocket.IsExploded)
		{
			GustCheck();
			ApplyWindForce();
			UpdateWindUI();
		}
	}

	private void GustCheck()
	{
		if (Time.time >= nextGustTime && !isGusting)
		{
			if (Random.value < gustChance)
			{
				StartGust();
			}
			SetNextGustTime();
		}

		if (isGusting)
		{
			ApplyGustRotation();

			if (Time.time >= gustEndTime)
			{
				EndGust();
			}
		}
	}

	private void StartGust()
	{
		isGusting = true;
		float gustDuration = Random.Range(minGustTime, maxGustTime);
		gustEndTime = Time.time + gustDuration;

		currentGustRotation = new Vector3(
			Random.Range(-1f, 1f),
			0f,
			Random.Range(-1f, 1f)
		).normalized * Random.Range(minGustRotationForce, maxGustRotationForce);

		float altitudeEffect = 1f + (rocket.transform.position.y * altitudeWindMultiplier);
		currentGustRotation *= altitudeEffect;
	}

	private void ApplyGustRotation()
	{
		if (rocket != null && rocket.rb != null)
		{
			float remainingTime = (gustEndTime - Time.time) / (gustEndTime - (Time.time - Time.fixedDeltaTime));
			Vector3 dampedRotation = currentGustRotation * remainingTime * gustModifier;

			Vector3 localRotation = rocket.transform.InverseTransformDirection(dampedRotation);
			rocket.rb.AddRelativeTorque(localRotation, ForceMode.Force);
		}
	}

	private void EndGust()
	{
		isGusting = false;
		currentGustRotation = Vector3.zero;
	}

	private void SetNextGustTime()
	{
		nextGustTime = Time.time + Random.Range(minGustInterval, maxGustInterval);
	}

	public void GenerateNewWind()
	{
		baseWindSpeed = Random.Range(minWindSpeed, maxWindSpeed);
		float randomAngle = Random.Range(0f, 360f);
		windDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
		SetNextGustTime();
		EndGust();
	}

	string GetDirection(float angle)
	{
		angle = NormalizeAngle(angle);
		float segmentSize = 360f / 16f;
		int index = Mathf.RoundToInt(angle / segmentSize);
		if (index == 16) index = 0;
		return windText[index];
	}

	float NormalizeAngle(float angle)
	{
		while (angle < 0f) angle += 360f;
		while (angle >= 360f) angle -= 360f;
		return angle;
	}

	void ApplyWindForce()
	{
		float altitudeEffect = 1f + (rocket.transform.position.y * altitudeWindMultiplier);
		float currentWindSpeed = baseWindSpeed * altitudeEffect;
		Vector3 windForce = windDirection * currentWindSpeed;
		rocket.rb.AddForce(windForce);
	}

	void UpdateWindUI()
	{
		float windAngle = Mathf.Atan2(windDirection.z, windDirection.x) * Mathf.Rad2Deg;
		string cardinalDirection = GetDirection(windAngle);
		windDirectionText.text = $"Wind Direction: {cardinalDirection}";

		float altitudeEffect = 1f + (rocket.transform.position.y * altitudeWindMultiplier);
		float currentWindSpeed = baseWindSpeed * altitudeEffect;
		string gustText = isGusting ? " (<color=red>GUSTING!</color>)" : "";
		windSpeedText.text = $"Wind Speed: {currentWindSpeed:F1} MPH{gustText}";
	}

	public Vector3 GetCurrentWindForce()
	{
		float altitudeEffect = 1f + (rocket.transform.position.y * altitudeWindMultiplier);
		float currentWindSpeed = baseWindSpeed * altitudeEffect;
		return windDirection * currentWindSpeed;
	}
}
