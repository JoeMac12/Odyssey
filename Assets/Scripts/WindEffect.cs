using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindEffect : MonoBehaviour
{
	private Rigidbody rb;
	public WindZone windZone;

	public TMP_Text windDirectionText;
	public TMP_Text windSpeedText;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		if (windZone != null)
		{
			ApplyWindForce(windZone);
			UpdateWindUI(windZone);
		}
	}

	void ApplyWindForce(WindZone windZone)
	{
		Vector3 windDir;
		float windStrength;

		if (windZone.mode == WindZoneMode.Directional)
		{
			windDir = windZone.transform.forward;
			windStrength = windZone.windMain;
		}
		else
		{
			Vector3 toObject = transform.position - windZone.transform.position;
			float distance = toObject.magnitude;
			windDir = toObject.normalized;
			windStrength = windZone.windMain / (distance * distance);
		}

		windStrength += Mathf.PerlinNoise(Time.time * windZone.windPulseFrequency, 0.0f) * windZone.windPulseMagnitude;
		windStrength += Random.Range(-windZone.windTurbulence, windZone.windTurbulence);

		rb.AddForce(windDir * windStrength);
	}

	void UpdateWindUI(WindZone windZone)
	{
		Vector3 windDir = windZone.mode == WindZoneMode.Directional ? windZone.transform.forward : (transform.position - windZone.transform.position).normalized;
		float windSpeed = windZone.windMain;

		float windDirectionDegrees = Mathf.Atan2(windDir.z, windDir.x) * Mathf.Rad2Deg;
		windDirectionText.text = "Wind Direction: " + windDirectionDegrees.ToString("F1") + "°";

		float windSpeedMPH = windSpeed * 2.237f;

		windSpeedText.text = "Wind Speed: " + windSpeedMPH.ToString("F1") + " MPH";
	}
}
