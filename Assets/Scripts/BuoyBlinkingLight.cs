using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyBlinkingLight : MonoBehaviour
{
	public Light pointLight;
	public float blinkInterval = 3f;
	public float blinkDuration = 0.5f;

	private void Start()
	{
		if (pointLight == null)
		{
			pointLight = GetComponent<Light>();
		}

		StartCoroutine(BlinkLight());
	}

	private IEnumerator BlinkLight()
	{
		while (true)
		{
			pointLight.enabled = true;
			yield return new WaitForSeconds(blinkDuration);

			pointLight.enabled = false;
			yield return new WaitForSeconds(blinkInterval - blinkDuration);
		}
	}
}
