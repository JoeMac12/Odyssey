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

	public TMP_Text speedText;
	public TMP_Text altitudeText;
	public TMP_Text flightTimeText;
	public TMP_Text bankAngleText;

	public AudioSource thrustSound;
	public Light rocketLight;
	public float minIntensity = 8f;
	public float maxIntensity = 10f;

	private float currentFuel;
	private Rigidbody rb;
	private Quaternion initialRotation;
	private float flightTime;
	private bool isFlying;
	private bool thrustLightRunning = false;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialRotation = transform.rotation;
		currentFuel = maxFuel;
		UpdateFuelBar();
		flightTime = 0f;
		isFlying = false;

		if (thrustSound != null)
		{
			thrustSound.Stop();
		}

		if (rocketLight != null)
		{
			rocketLight.enabled = false;
		}
	}

	void FixedUpdate()
	{
		bool isThrusting = Input.GetKey(KeyCode.Space);
		if (isThrusting && currentFuel > 0f)
		{
			rb.AddForce(transform.up * thrust);
			ConsumeFuel();
			if (!isFlying)
			{
				isFlying = true;
				flightTime = 0f;
			}

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

		if (isFlying)
		{
			flightTime += Time.fixedDeltaTime;
		}

		float moveHorizontal = -Input.GetAxis("Horizontal");
		float moveVertical = -Input.GetAxis("Vertical");
		Vector3 rotationInput = new Vector3(moveVertical, 0.0f, -moveHorizontal);

		Quaternion currentRotation = transform.rotation;
		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

		Vector3 deltaEulerAngles = deltaRotation.eulerAngles;
		deltaEulerAngles.x = NormalizeAngle(deltaEulerAngles.x);
		deltaEulerAngles.y = NormalizeAngle(deltaEulerAngles.y);
		deltaEulerAngles.z = NormalizeAngle(deltaEulerAngles.z);

		ApplyRotation(rotationInput, deltaEulerAngles);

		if (rotationInput.magnitude == 0f && isThrusting && currentFuel > 0f)
		{
			StraightenRocket(deltaEulerAngles);
		}

		if (rb.velocity.magnitude > maxVelocity)
		{
			rb.velocity = rb.velocity.normalized * maxVelocity;
		}

		UpdateUI(deltaEulerAngles);
	}

	float NormalizeAngle(float angle)
	{
		while (angle > 180f) angle -= 360f;
		while (angle < -180f) angle += 360f;
		return angle;
	}

	void ApplyRotation(Vector3 rotationInput, Vector3 deltaEulerAngles)
	{
		if (Mathf.Abs(deltaEulerAngles.x) < maxTurnAngle || Mathf.Sign(rotationInput.x) != Mathf.Sign(deltaEulerAngles.x))
		{
			rb.AddRelativeTorque(rotationInput.x * rotationSpeed, 0f, 0f);
		}

		if (Mathf.Abs(deltaEulerAngles.z) < maxTurnAngle || Mathf.Sign(rotationInput.z) != Mathf.Sign(deltaEulerAngles.z))
		{
			rb.AddRelativeTorque(0f, 0f, rotationInput.z * rotationSpeed);
		}
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

	void UpdateUI(Vector3 deltaEulerAngles)
	{
		float speedMPH = rb.velocity.magnitude * 2.237f;
		speedText.text = "Speed: " + speedMPH.ToString("F1") + " MPH";

		float altitude = transform.position.y * 3.281f;
		altitudeText.text = "Altitude: " + altitude.ToString("F1") + " ft";

		flightTimeText.text = "Flight Time: " + flightTime.ToString("F1") + " s";

		float bankAngle = deltaEulerAngles.z;
		bankAngleText.text = "Bank Angle: " + bankAngle.ToString("F1") + "°";
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
