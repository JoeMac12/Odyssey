using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StormCloudManager : MonoBehaviour
{
	[Header("References")]
	public RocketController rocketController;
	public GameObject stormCloudPrefab;
	public GameObject lightningPrefab;
	public ThunderSoundController thunderSoundController;

	[Header("Spawn Settings")]
	public float minSpawnHeight = 1000f;
	public float maxSpawnHeight = 4000f;
	public float horizontalSpawnRange = 100f;
	public float minCloudLifetime = 15f;
	public float maxCloudLifetime = 30f;
	public int maxActiveStorms = 3;
	[Range(0f, 1f)]
	public float stormSpawnChance = 0.4f;
	public float minHeightAboveRocket = 100f;
	public float maxHeightAboveRocket = 300f;

	[Header("Lightning Settings")]
	public float minLightningInterval = 2f;
	public float maxLightningInterval = 5f;
	public float lightningLifetime = 0.5f;
	public float lightningDamage = 10f;
	public float lightningSpawnRadius = 15f;
	public float insideLightningRad = 5f;
	public float insideLightningHeight = 10f;

	[Header("Cloud Settings")]
	public float cloudDriftSpeed = 5f;
	public float heightVariationSpeed = 2f;
	public float heightVariationAmount = 10f;

	private List<StormCloud> activeStormClouds = new List<StormCloud>();
	private float nextSpawnTime;
	private const float spawnCheckInterval = 3f;

	private class StormCloud
	{
		public GameObject cloudObject;
		public GameObject activeLightning;
		public float despawnTime;
		public float nextLightningTime;
		public Vector3 initialPosition;
		public float heightOffset;
		public float driftPhase;
		public SphereCollider cloudSphere;
		public bool isRocketInside;
	}

	private void Start()
	{
		if (rocketController == null || stormCloudPrefab == null || lightningPrefab == null)
		{
			enabled = false;
			return;
		}

		nextSpawnTime = Time.time + spawnCheckInterval;
	}

	private void Update()
	{
		if (Time.time >= nextSpawnTime)
		{
			ConsiderSpawningCloud();
			nextSpawnTime = Time.time + spawnCheckInterval;
		}

		UpdateStormClouds();
	}

	private void ConsiderSpawningCloud()
	{
		if (rocketController.IsExploded) return;
		if (activeStormClouds.Count >= maxActiveStorms) return;

		float rocketHeight = rocketController.transform.position.y;
		if (rocketHeight > minSpawnHeight && Random.value < stormSpawnChance)
		{
			SpawnStormCloud();
		}
	}

	private void SpawnStormCloud()
	{
		Vector3 rocketPosition = rocketController.transform.position;
		float heightAboveRocket = Random.Range(minHeightAboveRocket, maxHeightAboveRocket);
		float spawnHeight = Mathf.Min(rocketPosition.y + heightAboveRocket, maxSpawnHeight);

		Vector2 randomCircle = Random.insideUnitCircle * horizontalSpawnRange;
		Vector3 spawnPosition = new Vector3(
			rocketPosition.x + randomCircle.x,
			spawnHeight,
			rocketPosition.z + randomCircle.y
		);

		GameObject cloudObject = Instantiate(stormCloudPrefab, spawnPosition, Quaternion.identity);
		SphereCollider cloudSphere = cloudObject.GetComponent<SphereCollider>();

		StormCloud stormCloud = new StormCloud
		{
			cloudObject = cloudObject,
			despawnTime = Time.time + Random.Range(minCloudLifetime, maxCloudLifetime),
			nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval),
			initialPosition = spawnPosition,
			heightOffset = 0f,
			driftPhase = Random.Range(0f, 2f * Mathf.PI),
			cloudSphere = cloudSphere,
			isRocketInside = false
		};

		activeStormClouds.Add(stormCloud);
	}

	private void UpdateStormClouds()
	{
		for (int i = activeStormClouds.Count - 1; i >= 0; i--)
		{
			StormCloud cloud = activeStormClouds[i];

			if (Time.time >= cloud.despawnTime)
			{
				RemoveStormCloud(cloud);
				activeStormClouds.RemoveAt(i);
				continue;
			}

			UpdateCloudMovement(cloud);
			HandleLightning(cloud);
		}
	}

	private void UpdateCloudMovement(StormCloud cloud)
	{
		cloud.driftPhase += Time.deltaTime;
		cloud.heightOffset = Mathf.Sin(cloud.driftPhase * heightVariationSpeed) * heightVariationAmount;

		Vector3 newPosition = cloud.initialPosition;
		newPosition.y += cloud.heightOffset;
		newPosition.x += Mathf.Sin(cloud.driftPhase * 0.5f) * cloudDriftSpeed;
		newPosition.z += Mathf.Cos(cloud.driftPhase * 0.5f) * cloudDriftSpeed;

		cloud.cloudObject.transform.position = newPosition;
	}

	private void HandleLightning(StormCloud cloud)
	{
		if (Time.time >= cloud.nextLightningTime)
		{
			if (cloud.activeLightning != null)
			{
				Destroy(cloud.activeLightning);
			}

			Vector3 lightningPosition;
			if (cloud.isRocketInside)
			{
				Vector3 randomOffset = Random.insideUnitSphere * insideLightningRad;
				randomOffset.y = insideLightningHeight;
				lightningPosition = rocketController.transform.position + randomOffset;
			}
			else
			{
				Vector3 randomDirection = Random.insideUnitSphere * lightningSpawnRadius;
				lightningPosition = cloud.cloudObject.transform.position + randomDirection;
			}

			cloud.activeLightning = Instantiate(lightningPrefab, lightningPosition, Random.rotation);
			cloud.activeLightning.transform.SetParent(cloud.cloudObject.transform);

			if (cloud.isRocketInside && thunderSoundController != null)
			{
				thunderSoundController.PlayThunderSound();
			}

			if (cloud.isRocketInside && !rocketController.IsExploded)
			{
				float damageReduction = rocketController.armorPercentage / 100f;
				float finalDamage = lightningDamage * (1f - damageReduction);
				rocketController.TakeDamage(finalDamage);
			}

			StartCoroutine(RemoveLightningAfterDelay(cloud));
			cloud.nextLightningTime = Time.time + Random.Range(minLightningInterval, maxLightningInterval);
		}
	}

	private IEnumerator RemoveLightningAfterDelay(StormCloud cloud)
	{
		yield return new WaitForSeconds(lightningLifetime);

		if (cloud.activeLightning != null)
		{
			Destroy(cloud.activeLightning);
			cloud.activeLightning = null;
		}
	}

	private void RemoveStormCloud(StormCloud cloud)
	{
		if (cloud.activeLightning != null)
		{
			Destroy(cloud.activeLightning);
		}
		Destroy(cloud.cloudObject);
	}

	public void OnRocketEnterStormCloud(Collider cloudTrigger)
	{
		foreach (StormCloud cloud in activeStormClouds)
		{
			if (cloud.cloudObject.GetComponent<Collider>() == cloudTrigger)
			{
				cloud.isRocketInside = true;
				break;
			}
		}
	}

	public void OnRocketExitStormCloud(Collider cloudTrigger)
	{
		foreach (StormCloud cloud in activeStormClouds)
		{
			if (cloud.cloudObject.GetComponent<Collider>() == cloudTrigger)
			{
				cloud.isRocketInside = false;
				break;
			}
		}
	}

	public void ClearAllStormClouds()
	{
		foreach (var cloud in activeStormClouds)
		{
			RemoveStormCloud(cloud);
		}
		activeStormClouds.Clear();
	}

	private void OnDisable()
	{
		foreach (var cloud in activeStormClouds)
		{
			RemoveStormCloud(cloud);
		}
		activeStormClouds.Clear();
	}
}
