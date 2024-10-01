using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform target;
	public float distance = 10.0f;
	public float minDistance = 5f;
	public float maxDistance = 20f;
	public float zoomSpeed = 2f;
	public float rotationSpeed = 5f;
	public float minYAngle = 10f;
	public float maxYAngle = 80f;

	public bool smoothZoom = false;
	public float zoomSmoothSpeed = 10f;

	private float currentX = 0f;
	private float currentY = 20f;
	private float setDistance;

	void Start()
	{
		setDistance = distance;
	}

	void Update()
	{
		float scrollInput = Input.GetAxis("Mouse ScrollWheel");
		if (scrollInput != 0f)
		{
			setDistance -= scrollInput * zoomSpeed;
			setDistance = Mathf.Clamp(setDistance, minDistance, maxDistance);
		}

		if (Input.GetMouseButton(1))
		{
			currentX += Input.GetAxis("Mouse X") * rotationSpeed;
			currentY -= Input.GetAxis("Mouse Y") * rotationSpeed;
			currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);
		}
	}

	void LateUpdate()
	{
		if (target == null)
			return;

		if (smoothZoom)
		{
			distance = Mathf.Lerp(distance, setDistance, Time.deltaTime * zoomSmoothSpeed);
		}
		else
		{
			distance = setDistance;
		}

		Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
		Vector3 direction = new Vector3(0, 0, -distance);
		Vector3 position = rotation * direction + target.position;

		transform.position = position;
		transform.LookAt(target.position);
	}
}
