using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RocketController : MonoBehaviour
{
	public float thrust = 2500f;
	public float rotationSpeed = 1000f;
	public float maxVelocity = 9999f;
	public float maxTurnAngle = 30f;
	public float maxFuel = 100f;
	public float fuelRate = 10f;
	public Image fuelBar;

	public float maxHealth = 100f;
	public float currentHealth;
	public float armor = 0f;

	//public GameObject explosionPrefab;
	public float explosionDelay = 3f;

	public TMP_Text speedText;
	public TMP_Text altitudeText;
	public TMP_Text flightTimeText;
	public TMP_Text bankAngleText;

	public AudioSource thrustSound;
	public Light rocketLight;
	public float minIntensity = 8f;
	public float maxIntensity = 10f;

	[HideInInspector]
	public Rigidbody rb;

	private float currentFuel;
	private Quaternion initialRotation;
	private float lastPositiveVerticalSpeed;
	private bool isExploding = false;
	private bool hasLaunched = false;
	private float flightTime;
	private bool thrustLightRunning = false;

	public bool IsThrusting { get; private set; }
	public bool IsExploded { get; private set; }
	public float FlightStartTime { get; private set; }

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialRotation = transform.rotation;
		ResetRocket(transform.position);
	}

	void FixedUpdate()
	{
		if (IsExploded) return;

		IsThrusting = Input.GetKey(KeyCode.Space) && currentFuel > 0f;

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

	void ApplyRotation()
	{
		float moveHorizontal = -Input.GetAxis("Horizontal");
		float moveVertical = -Input.GetAxis("Vertical");

		Vector3 rotation = new Vector3(moveVertical, 0.0f, -moveHorizontal);

		Quaternion currentRotation = transform.rotation;
		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

		Vector3 deltaEulerAngles = deltaRotation.eulerAngles;
		deltaEulerAngles.x = NormalizeAngle(deltaEulerAngles.x);
		deltaEulerAngles.y = NormalizeAngle(deltaEulerAngles.y);
		deltaEulerAngles.z = NormalizeAngle(deltaEulerAngles.z);

		if (Mathf.Abs(deltaEulerAngles.x) < maxTurnAngle || Mathf.Sign(rotation.x) != Mathf.Sign(deltaEulerAngles.x))
		{
			rb.AddRelativeTorque(rotation.x * rotationSpeed, 0f, 0f);
		}

		if (Mathf.Abs(deltaEulerAngles.z) < maxTurnAngle || Mathf.Sign(rotation.z) != Mathf.Sign(deltaEulerAngles.z))
		{
			rb.AddRelativeTorque(0f, 0f, rotation.z * rotationSpeed);
		}

		if (rotation.magnitude == 0f && IsThrusting && currentFuel > 0f)
		{
			StraightenRocket(deltaEulerAngles);
		}
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
		speedText.text = "Speed: " + speedMPH.ToString("F1") + " MPH";

		float altitude = transform.position.y * 3.281f;
		altitudeText.text = "Altitude: " + altitude.ToString("F1") + " ft";

		flightTimeText.text = "Flight Time: " + flightTime.ToString("F1") + " s";

		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * transform.rotation;
		float bankAngle = NormalizeAngle(deltaRotation.eulerAngles.z);
		bankAngleText.text = "Bank Angle: " + bankAngle.ToString("F1") + "°";
	}

	public void TakeDamage(float damage)
	{
		float damageAfterArmor = Mathf.Max(0, damage - armor);
		currentHealth -= damageAfterArmor;

		if (currentHealth <= 0 && !isExploding)
		{
			StartCoroutine(ExplodeRocket());
		}
	}

	IEnumerator ExplodeRocket()
	{
		isExploding = true;
		IsExploded = true;

		this.enabled = false;

		//Instantiate(explosionPrefab, transform.position, Quaternion.identity);

		gameObject.SetActive(false);

		GameManager.Instance.OnRocketExploded();

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
		this.enabled = true;
		gameObject.SetActive(true);
		rb.isKinematic = false;
		UpdateFuelBar();

		if (thrustSound != null)
		{
			thrustSound.Stop();
		}

		if (rocketLight != null)
		{
			rocketLight.enabled = false;
			thrustLightRunning = false;
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
