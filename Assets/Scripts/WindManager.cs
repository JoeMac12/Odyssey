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

	private float baseWindSpeed;
	private Vector3 windDirection;
	private RocketController rocket;

	private readonly string[] windText = { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE",
												"S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW" };

	public void Initialize(RocketController rocketController)
	{
		rocket = rocketController;
		GenerateNewWind();
	}

	void FixedUpdate()
	{
		if (rocket != null && !rocket.IsExploded)
		{
			ApplyWindForce();
			UpdateWindUI();
		}
	}

	public void GenerateNewWind()
	{
		baseWindSpeed = Random.Range(minWindSpeed, maxWindSpeed);
		float randomAngle = Random.Range(0f, 360f);
		windDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
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
		windSpeedText.text = $"Wind Speed: {currentWindSpeed:F1} MPH";
	}

	public Vector3 GetCurrentWindForce()
	{
		float altitudeEffect = 1f + (rocket.transform.position.y * altitudeWindMultiplier);
		float currentWindSpeed = baseWindSpeed * altitudeEffect;
		return windDirection * currentWindSpeed;
	}
}
