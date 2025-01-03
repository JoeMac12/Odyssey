using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxChangeManager : MonoBehaviour
{
	public RocketController rocketController;
	public float minHeight = 0f;
	public float maxHeight = 50000f;

	[Header("Rotation Settings")]
	public Vector3 startRotation = new Vector3(20f, 220f, 180f);
	public Vector3 endRotation = new Vector3(-10f, 223f, 210f);
	public float rotationSmoothSpeed = 2f;

	[Header("Skybox Settings")]
	public float startExposure = 1f;
	public float endExposure = 0.2f;
	public float exposureSmoothSpeed = 1f;

	[Header("Light Settings")]
	public Light directionalLight;
	public float startLightIntensity = 1f;
	public float endLightIntensity = 0f;
	public float lightSmoothSpeed = 1f;

	private Vector3 currentTargetRotation;
	private Quaternion initialRotation;
	private Material skyboxMaterial;
	private float currentExposure;
	private float currentLightIntensity;

	private void Start()
	{
		if (rocketController == null)
		{
			enabled = false;
			return;
		}

		initialRotation = transform.rotation;
		transform.rotation = Quaternion.Euler(startRotation);

		skyboxMaterial = RenderSettings.skybox;
		if (skyboxMaterial == null)
		{
			Debug.LogWarning("uh no material");
		}
		else
		{
			currentExposure = startExposure;
			skyboxMaterial.SetFloat("_Exposure", currentExposure);
		}

		if (directionalLight != null)
		{
			currentLightIntensity = startLightIntensity;
			directionalLight.intensity = currentLightIntensity;
		}
		else
		{
			Debug.LogWarning("bruh");
		}
	}

	private void Update()
	{
		if (rocketController == null || rocketController.IsExploded) return;

		float currentHeight = Mathf.Clamp(rocketController.transform.position.y, minHeight, maxHeight);
		float progress = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);

		currentTargetRotation = Vector3.Lerp(startRotation, endRotation, progress);
		Quaternion targetRotation = Quaternion.Euler(currentTargetRotation);
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothSpeed);

		if (skyboxMaterial != null)
		{
			float targetExposure = Mathf.Lerp(startExposure, endExposure, progress);
			currentExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * exposureSmoothSpeed);
			skyboxMaterial.SetFloat("_Exposure", currentExposure);
		}

		if (directionalLight != null)
		{
			float targetLightIntensity = Mathf.Lerp(startLightIntensity, endLightIntensity, progress);
			currentLightIntensity = Mathf.Lerp(currentLightIntensity, targetLightIntensity, Time.deltaTime * lightSmoothSpeed);
			directionalLight.intensity = currentLightIntensity;
		}
	}

	public void ResetEnvironment()
	{
		transform.rotation = Quaternion.Euler(startRotation);

		if (skyboxMaterial != null)
		{
			currentExposure = startExposure;
			skyboxMaterial.SetFloat("_Exposure", startExposure);
		}

		if (directionalLight != null)
		{
			currentLightIntensity = startLightIntensity;
			directionalLight.intensity = startLightIntensity;
		}
	}

	private void OnDisable()
	{
		if (skyboxMaterial != null)
		{
			skyboxMaterial.SetFloat("_Exposure", startExposure);
		}

		if (directionalLight != null)
		{
			directionalLight.intensity = startLightIntensity;
		}
	}
}
