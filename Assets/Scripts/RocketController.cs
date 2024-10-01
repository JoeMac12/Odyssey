using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
	public float thrust = 100f;
	public float rotationSpeed = 10f;
	public float maxVelocity = 1000f;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	void FixedUpdate()
	{
		if (Input.GetKey(KeyCode.Space))
		{
			rb.AddForce(transform.up * thrust);
		}

		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");

		Vector3 rotation = new Vector3(moveVertical, 0.0f, -moveHorizontal) * rotationSpeed;
		rb.AddRelativeTorque(rotation);

		if (rb.velocity.magnitude > maxVelocity)
		{
			rb.velocity = rb.velocity.normalized * maxVelocity;
		}
	}
}
