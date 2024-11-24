using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unloader : MonoBehaviour
{
	[Header("Stuff")]
	public RocketController rocketController;
	public GameObject oceanObject;

	[Header("Settings")]
	public float unloadHeight = 2000f;
	public float timeCheck = 0.5f;

	private float nextCheckTime;
	private bool isOceanLoaded = true;
	private Vector3 initialRocketPosition;

	private void Start()
	{
		if (rocketController == null || oceanObject == null)
		{
			enabled = false;
			return;
		}

		initialRocketPosition = rocketController.transform.position;
		nextCheckTime = Time.time + timeCheck;
		oceanObject.SetActive(true);
	}

	private void Update()
	{
		if (Time.time < nextCheckTime)
			return;

		nextCheckTime = Time.time + timeCheck;

		float currentHeight = rocketController.transform.position.y - initialRocketPosition.y;

		if (currentHeight > unloadHeight && isOceanLoaded)
		{
			oceanObject.SetActive(false);
			isOceanLoaded = false;
		}
		else if (currentHeight <= unloadHeight && !isOceanLoaded)
		{
			oceanObject.SetActive(true);
			isOceanLoaded = true;
		}

		if (rocketController.IsExploded && !isOceanLoaded)
		{
			oceanObject.SetActive(true);
			isOceanLoaded = true;
		}
	}

	public void ResetOceanState()
	{
		oceanObject.SetActive(true);
		isOceanLoaded = true;
		nextCheckTime = Time.time + timeCheck;
	}

	private void OnDisable()
	{
		if (oceanObject != null)
		{
			oceanObject.SetActive(true);
		}
	}
}
