using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StormCloudTrigger : MonoBehaviour
{
	private StormCloudManager stormCloudManager;

	private void Start()
	{
		stormCloudManager = FindObjectOfType<StormCloudManager>();
		if (stormCloudManager == null)
		{
			enabled = false;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			stormCloudManager.OnRocketEnterStormCloud(GetComponent<Collider>());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			stormCloudManager.OnRocketExitStormCloud(GetComponent<Collider>());
		}
	}
}
