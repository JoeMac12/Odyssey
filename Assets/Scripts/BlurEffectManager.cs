using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class BlurEffectManager : MonoBehaviour
{
	public PostProcessVolume postProcessVolume;
	public float maxBlurIntensity = 5f;
	public float blurTransitionDuration = 1f;

	private DepthOfField depthOfField;
	private float currentBlurTime = 0f;
	private bool isBlurring = false;

	void Start()
	{
		if (postProcessVolume != null && postProcessVolume.profile.TryGetSettings(out DepthOfField dof))
		{
			depthOfField = dof;
			depthOfField.active = false;
		}
	}

	public void EnableBlur()
	{
		if (depthOfField != null)
		{
			isBlurring = true;
			currentBlurTime = 0f;
			depthOfField.active = true;
		}
	}

	public void DisableBlur()
	{
		if (depthOfField != null)
		{
			isBlurring = false;
			currentBlurTime = 0f;
		}
	}

	void Update()
	{
		if (depthOfField == null) return;

		if (isBlurring)
		{
			currentBlurTime += Time.deltaTime;
			float progress = Mathf.Clamp01(currentBlurTime / blurTransitionDuration);
			depthOfField.focusDistance.value = Mathf.Lerp(50f, 0.1f, progress);
			depthOfField.aperture.value = Mathf.Lerp(0.1f, maxBlurIntensity, progress);
		}
		else if (depthOfField.active)
		{
			currentBlurTime += Time.deltaTime;
			float progress = Mathf.Clamp01(currentBlurTime / blurTransitionDuration);
			depthOfField.focusDistance.value = Mathf.Lerp(0.1f, 50f, progress);
			depthOfField.aperture.value = Mathf.Lerp(maxBlurIntensity, 0.1f, progress);

			if (progress >= 1f)
			{
				depthOfField.active = false;
			}
		}
	}
}
