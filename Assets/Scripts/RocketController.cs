using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RocketController : MonoBehaviour
{
	public GameManager gameManager;
	public float thrust = 1250f;
	public float rotationSpeed = 250f;
	public float yawRotationSpeed = 250f;
	public float maxVelocity = 9999f;
	public float maxFuel = 100f;
	public float fuelRate = 10f;
	public Image fuelBar;

	public CameraController cameraController;

	public float maxHealth = 100f;
	public float currentHealth;
	[Range(0f, 100f)]
	public float armorPercentage = 0f;

	public TMP_Text healthText;
	public TMP_Text armorText;

	[Header("Drag Settings")]
	public float normalDrag = 0.05f;
	public float endDrag = 3f;

	[Header("Engine Effects")]
	public GameObject thrustFlame;

	[Header("Thrust Materials")]
	public Material thrustFlameMaterial;
	public Material thrustSmokeMaterial;

	[Header("Fade Settings")]
	public float fadeInSpeed = 4f;
	public float fadeOutSpeed = 2f;
	public float maxAlpha = 0.9f;

	private float currentEffectsAlpha = 0f;
	private static readonly string alphaProb = "_Alpha";

	[Header("Sound Effects")]
	public AudioSource explosionSound;
	public float explosionVolume = 1f;

	public GameObject explosionPrefab;
	public float explosionDelay = 3f;

	public TMP_Text speedText;
	public TMP_Text altitudeText;
	public TMP_Text flightTimeText;
	public TMP_Text bankAngleText;

	public AudioSource thrustSound;
	public Light rocketLight;
	public float minIntensity = 8f;
	public float maxIntensity = 10f;

	[Header("Collision Settings")]
	public float minDamageSpeed = 25f;
	public float damageMultiplier = 1f;
	public float speedMemoryTime = 0.25f;

	[Header("Direction Indicators")]
	public Image altitudeArrowUp;
	public Image altitudeArrowDown;
	public Image altitudeArrowNeutral;

	[Header("Fall Warning")]
	public AudioSource fallWarningSound;
	public float warningBeepInterval = 0.5f;
	public float fallSpeedThreshold = -5f;

	[Header("Damage Effects")]
	public GameObject[] damageSpawnPoints;
	public GameObject sparkParticlePrefab;
	public AudioClip damageSound;
	public float particleLifetime = 2f;
	public AudioSource damageAudioSource;

	private bool isWarningActive = false;
	private float nextBeepTime = 0f;

	[HideInInspector]
	public Rigidbody rb;

	private float currentFuel;
	private Quaternion initialRotation;
	private float lastPositiveVerticalSpeed;
	private bool isExploding = false;
	private bool hasLaunched = false;
	private float flightTime;
	private bool thrustLightRunning = false;
	private float highestRecentSpeed = 0f;
	private float lastSpeedUpdateTime = 0f;

	public bool IsThrusting { get; private set; }
	public bool IsExploded { get; private set; }
	public float FlightStartTime { get; private set; }

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialRotation = transform.rotation;
		ResetRocket(transform.position);

		if (gameManager == null)
		{
			enabled = false;
			return;
		}

		if (explosionSound == null)
		{
			explosionSound = gameObject.AddComponent<AudioSource>();
			explosionSound.playOnAwake = false;
			explosionSound.volume = explosionVolume;
		}

		InitializeThrustEffects();
	}

	private void InitializeThrustEffects()
	{
		if (thrustFlameMaterial != null)
		{
			thrustFlameMaterial.SetFloat(alphaProb, 0f);
		}
		if (thrustSmokeMaterial != null)
		{
			thrustSmokeMaterial.SetFloat(alphaProb, 0f);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("DeathTrigger") && !isExploding)
		{
			StartCoroutine(ExplodeRocket());
		}
	}

	void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("GameObject") && !IsExploded)
		{
			float damageSpeed = Mathf.Max(highestRecentSpeed, rb.velocity.magnitude * 2.237f);

			if (damageSpeed > minDamageSpeed)
			{
				float overSpeed = damageSpeed - minDamageSpeed;
				float baseDamage = overSpeed * damageMultiplier;

				float impactForce = collision.impulse.magnitude;
				baseDamage *= (impactForce * 0.1f);

				float damageReduction = armorPercentage / 100f;
				float finalDamage = baseDamage * (1f - damageReduction);

				TakeDamage(finalDamage);
			}
		}
	}

	void FixedUpdate()
	{
		if (IsExploded) return;

		float currentSpeedMPH = rb.velocity.magnitude * 2.237f;

		if (Time.time - lastSpeedUpdateTime > speedMemoryTime)
		{
			highestRecentSpeed = currentSpeedMPH;
			lastSpeedUpdateTime = Time.time;
		}
		else if (currentSpeedMPH > highestRecentSpeed)
		{
			highestRecentSpeed = currentSpeedMPH;
			lastSpeedUpdateTime = Time.time;
		}

		IsThrusting = Input.GetKey(KeyCode.Space) && currentFuel > 0f;

		if (thrustFlame != null)
		{
			UpdateThrustEffectsVisibility();
		}

		UpdateDrag();
		CheckFallWarning();

		if (IsThrusting)
		{
			if (!hasLaunched)
			{
				hasLaunched = true;
				FlightStartTime = Time.time;
				lastPositiveVerticalSpeed = Time.time;
			}

			rb.AddForce(transform.up * thrust);
			ConsumeFuel();

			if (thrustSound != null && !thrustSound.isPlaying)
			{
				thrustSound.Play();
			}

			if (rocketLight != null && !thrustLightRunning)
			{
				rocketLight.enabled = true;
				StartCoroutine(RandomLightPower());
			}
		}
		else
		{
			if (thrustSound != null && thrustSound.isPlaying)
			{
				thrustSound.Stop();
			}

			if (rocketLight != null && thrustLightRunning)
			{
				rocketLight.enabled = false;
				StopCoroutine(RandomLightPower());
				thrustLightRunning = false;
			}
		}

		if (hasLaunched)
		{
			flightTime += Time.fixedDeltaTime;

			if (rb.velocity.y > 0)
			{
				lastPositiveVerticalSpeed = Time.time;
			}
			else if (Time.time - lastPositiveVerticalSpeed > explosionDelay && !isExploding)
			{
				StartCoroutine(ExplodeRocket());
			}
		}

		ApplyRotation();
		LimitVelocity();
		UpdateUI();
	}

	private void CheckFallWarning()
	{
		if (!hasLaunched || IsExploded) return;

		bool isFalling = rb.velocity.y < fallSpeedThreshold;

		if (isFalling && !isWarningActive)
		{
			isWarningActive = true;
			nextBeepTime = Time.time;
		}
		else if (!isFalling && isWarningActive)
		{
			isWarningActive = false;
			if (fallWarningSound != null && fallWarningSound.isPlaying)
			{
				fallWarningSound.Stop();
			}
		}

		if (isWarningActive && Time.time >= nextBeepTime)
		{
			PlayWarningBeep();
			nextBeepTime = Time.time + warningBeepInterval;
		}
	}

	private void PlayWarningBeep()
	{
		if (fallWarningSound != null && fallWarningSound.clip != null)
		{
			fallWarningSound.Play();
		}
	}

	private void UpdateThrustEffectsVisibility()
	{
		if (thrustFlameMaterial == null && thrustSmokeMaterial == null) return;

		if (IsThrusting)
		{
			currentEffectsAlpha = Mathf.MoveTowards(currentEffectsAlpha, maxAlpha, fadeInSpeed * Time.fixedDeltaTime);
		}
		else
		{
			currentEffectsAlpha = Mathf.MoveTowards(currentEffectsAlpha, 0f, fadeOutSpeed * Time.fixedDeltaTime);
		}

		UpdateMaterialAlpha(thrustFlameMaterial, currentEffectsAlpha);
		UpdateMaterialAlpha(thrustSmokeMaterial, currentEffectsAlpha);

		if (!thrustFlame.activeSelf && currentEffectsAlpha > 0f)
		{
			thrustFlame.SetActive(true);
		}
		else if (thrustFlame.activeSelf && currentEffectsAlpha <= 0f)
		{
			thrustFlame.SetActive(false);
		}
	}

	private void UpdateMaterialAlpha(Material material, float alpha)
	{
		if (material != null)
		{
			material.SetFloat(alphaProb, alpha);
		}
	}

	void UpdateDrag()
	{
		rb.drag = normalDrag;

		if (currentFuel <= 0 && rb.velocity.y > 0)
		{
			Vector3 upwardVelocity = Vector3.up * Mathf.Max(0, Vector3.Dot(rb.velocity, Vector3.up));
			rb.AddForce(-upwardVelocity * endDrag);
		}
	}

	void ApplyRotation()
	{
		float moveHorizontal = -Input.GetAxis("Horizontal");
		float moveVertical = -Input.GetAxis("Vertical");

		float yawInput = 0f;
		if (Input.GetKey(KeyCode.Q)) yawInput = 1f;
		if (Input.GetKey(KeyCode.E)) yawInput = -1f;

		Vector3 cameraForward = cameraController.transform.forward;
		Vector3 cameraRight = cameraController.transform.right;

		cameraForward.y = 0;
		cameraRight.y = 0;
		cameraForward.Normalize();
		cameraRight.Normalize();

		Vector3 inputDirection = (cameraForward * moveVertical + cameraRight * moveHorizontal).normalized;

		Vector3 localInput = transform.InverseTransformDirection(inputDirection);

		Vector3 rotation = new Vector3(-localInput.z, 0.0f, localInput.x);

		Quaternion currentRotation = transform.rotation;
		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

		Vector3 deltaEulerAngles = deltaRotation.eulerAngles;
		deltaEulerAngles.x = NormalizeAngle(deltaEulerAngles.x);
		deltaEulerAngles.y = NormalizeAngle(deltaEulerAngles.y);
		deltaEulerAngles.z = NormalizeAngle(deltaEulerAngles.z);

		if (rotation.magnitude > 0.01f)
		{
			rb.AddRelativeTorque(rotation.x * rotationSpeed, 0f, rotation.z * rotationSpeed);
		}

		if (yawInput != 0)
		{
			rb.AddRelativeTorque(0f, yawInput * yawRotationSpeed, 0f);
		}

		/*if (rotation.magnitude < 0.01f && yawInput == 0f && IsThrusting && currentFuel > 0f)
		{
			StraightenRocket(deltaEulerAngles);
		}*/
	}

	float NormalizeAngle(float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	void StraightenRocket(Vector3 deltaEulerAngles)
	{
		if (Mathf.Abs(deltaEulerAngles.x) > 0.1f)
		{
			float correctionTorqueX = -Mathf.Sign(deltaEulerAngles.x) * rotationSpeed * 0.5f;
			rb.AddRelativeTorque(correctionTorqueX, 0f, 0f);
		}

		if (Mathf.Abs(deltaEulerAngles.z) > 0.1f)
		{
			float correctionTorqueZ = -Mathf.Sign(deltaEulerAngles.z) * rotationSpeed * 0.5f;
			rb.AddRelativeTorque(0f, 0f, correctionTorqueZ);
		}
	}

	void LimitVelocity()
	{
		if (rb.velocity.magnitude > maxVelocity)
		{
			rb.velocity = rb.velocity.normalized * maxVelocity;
		}
	}

	void ConsumeFuel()
	{
		currentFuel -= Time.fixedDeltaTime * fuelRate;
		currentFuel = Mathf.Clamp(currentFuel, 0f, maxFuel);
		UpdateFuelBar();
	}

	void UpdateFuelBar()
	{
		float fuelPercentage = currentFuel / maxFuel;
		fuelBar.fillAmount = fuelPercentage;

		if (fuelPercentage > 0.5f)
		{
			fuelBar.color = Color.white;
		}
		else if (fuelPercentage > 0.25f)
		{
			fuelBar.color = Color.yellow;
		}
		else
		{
			fuelBar.color = Color.red;
		}
	}

	void UpdateUI()
	{
		float speedMPH = rb.velocity.magnitude * 2.237f;
		speedText.text = $"Speed: {speedMPH:F0} MPH";

		float altitude = transform.position.y * 3.281f;
		altitudeText.text = $"Altitude: {altitude:F0} ft";

		float verticalVelocity = rb.velocity.y;
		float velocityThreshold = 0.1f;

		bool isGoingUp = verticalVelocity > velocityThreshold;
		bool isGoingDown = verticalVelocity < -velocityThreshold;

		altitudeArrowUp.gameObject.SetActive(isGoingUp);
		altitudeArrowDown.gameObject.SetActive(isGoingDown);
		altitudeArrowNeutral.gameObject.SetActive(!isGoingUp && !isGoingDown);

		flightTimeText.text = $"Flight Time: {flightTime:F0} s";

		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * transform.rotation;
		float bankAngle = NormalizeAngle(deltaRotation.eulerAngles.z);
		bankAngleText.text = $"Bank Angle: {bankAngle:F0}°";

		if (healthText != null)
		{
			healthText.text = $"Health: <color=green>{currentHealth:F0}</color>/{maxHealth:F0}";
		}
		if (armorText != null)
		{
			armorText.text = $"DMG Reduction: {armorPercentage:F0}%";
		}
	}

	public void TakeDamage(float damage)
	{
		currentHealth -= damage;

		if (!isExploding)
		{
			SpawnDamageEffects();
		}

		if (currentHealth <= 0 && !isExploding)
		{
			StartCoroutine(ExplodeRocket());
		}
	}

	public void SelfDestruct()
	{
		if (hasLaunched && !IsExploded)
		{
			TakeDamage(currentHealth);
		}
	}

	private void SpawnDamageEffects()
	{
		if (damageSpawnPoints == null || damageSpawnPoints.Length == 0 || sparkParticlePrefab == null)
			return;

		int randomIndex = Random.Range(0, damageSpawnPoints.Length);
		Transform spawnPoint = damageSpawnPoints[randomIndex].transform;

		if (spawnPoint != null)
		{
			GameObject sparkEffect = Instantiate(sparkParticlePrefab,
				spawnPoint.position,
				Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)));

			sparkEffect.transform.SetParent(transform);

			Destroy(sparkEffect, particleLifetime);
		}

		if (damageAudioSource != null && damageSound != null)
		{
			damageAudioSource.pitch = Random.Range(0.95f, 1.05f);
			damageAudioSource.PlayOneShot(damageSound, Random.Range(0.8f, 1f));
		}
	}

	IEnumerator ExplodeRocket()
	{
		isExploding = true;
		IsExploded = true;

		GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		Destroy(explosion, 1f);

		if (explosionSound != null)
		{
			GameObject soundObject = new GameObject("ExplosionSound");
			soundObject.transform.position = transform.position;
			AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();
			tempAudioSource.clip = explosionSound.clip;
			tempAudioSource.volume = explosionSound.volume;
			tempAudioSource.spatialBlend = 1f;
			tempAudioSource.minDistance = 5f;
			tempAudioSource.maxDistance = 100f;
			tempAudioSource.Play();

			Destroy(soundObject, tempAudioSource.clip.length + 0.1f);
		}

		this.enabled = false;
		gameObject.SetActive(false);

		if (cameraController != null)
		{
			cameraController.controlsEnabled = false;
		}

		gameManager.OnRocketExploded();

		yield return new WaitForSeconds(1f);

		rb.isKinematic = true;
	}

	public void ResetRocket(Vector3 position)
	{
		transform.position = position;
		transform.rotation = initialRotation;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
		currentFuel = maxFuel;
		currentHealth = maxHealth;
		isExploding = false;
		IsExploded = false;
		hasLaunched = false;
		flightTime = 0f;
		highestRecentSpeed = 0f;
		lastSpeedUpdateTime = 0f;
		this.enabled = true;
		gameObject.SetActive(true);
		rb.isKinematic = false;
		UpdateFuelBar();

		if (thrustSound != null)
		{
			thrustSound.Stop();
		}

		currentEffectsAlpha = 0f;
		InitializeThrustEffects();

		if (thrustFlame != null)
		{
			thrustFlame.SetActive(false);
		}

		if (rocketLight != null)
		{
			rocketLight.enabled = false;
			thrustLightRunning = false;
		}

		if (cameraController != null)
		{
			cameraController.controlsEnabled = true;
		}

		isWarningActive = false;
		if (fallWarningSound != null && fallWarningSound.isPlaying)
		{
			fallWarningSound.Stop();
		}
	}

	IEnumerator RandomLightPower()
	{
		thrustLightRunning = true;
		while (true)
		{
			float randomIntensity = Random.Range(minIntensity, maxIntensity);
			rocketLight.intensity = randomIntensity;
			yield return new WaitForSeconds(0.05f);
		}
	}
}
