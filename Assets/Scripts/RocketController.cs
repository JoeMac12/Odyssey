using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RocketController : MonoBehaviour
{
	public float thrust = 2500f;
	public float rotationSpeed = 1000f;
	public float maxVelocity = 9999f;
	public float maxTurnAngle = 30f;
	public float maxFuel = 100f;
	public float fuelRate = 10f;
	public Image fuelBar;

	private float currentFuel;
	private Rigidbody rb;
	private Quaternion initialRotation;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		initialRotation = transform.rotation;
		currentFuel = maxFuel;
		UpdateFuelBar();
	}

	void FixedUpdate()
	{
		bool isThrusting = Input.GetKey(KeyCode.Space);
		if (isThrusting && currentFuel > 0f)
		{
			rb.AddForce(transform.up * thrust);
			ConsumeFuel();
		}

		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		Vector3 rotationInput = new Vector3(moveVertical, 0.0f, -moveHorizontal);

		Quaternion currentRotation = transform.rotation;
		Quaternion deltaRotation = Quaternion.Inverse(initialRotation) * currentRotation;

		Vector3 deltaEulerAngles = deltaRotation.eulerAngles;
		deltaEulerAngles.x = NormalizeAngle(deltaEulerAngles.x);
		deltaEulerAngles.y = NormalizeAngle(deltaEulerAngles.y);
		deltaEulerAngles.z = NormalizeAngle(deltaEulerAngles.z);

		ApplyRotation(rotationInput, deltaEulerAngles);

		if (rotationInput.magnitude == 0f && isThrusting)
		{
			StraightenRocket(deltaEulerAngles);
		}

		if (rb.velocity.magnitude > maxVelocity)
		{
			rb.velocity = rb.velocity.normalized * maxVelocity;
		}
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
}
