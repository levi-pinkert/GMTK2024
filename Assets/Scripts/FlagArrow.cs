using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagArrow : MonoBehaviour
{
	public static HashSet<FlagArrow> all = new();

	public float arrowOffset;
	public float sizePerDistance;
	public float cutoffDistance;
	public float scaleLerpLambda;
	public Transform arrowTransform;

	private float size = 1.0f;
	private float baseAspectRatio;
	private float cameraDistance = 0.0f;
	private float worldScale = 1.0f;
	private float visibilityScaleMultiplier = 0.0f;
	private float extraHeightOffset = 0.0f;

	private void Awake()
	{
		baseAspectRatio = arrowTransform.localScale.x / arrowTransform.localScale.y;
		UpdateSize();
		all.Add(this);
	}

	private void OnDestroy()
	{
		all.Remove(this);
	}

	private void UpdateSize()
	{
		size = cameraDistance * sizePerDistance;
		arrowTransform.localScale = new Vector3(size * baseAspectRatio, size, 1.0f) * visibilityScaleMultiplier;
		arrowTransform.localPosition = Vector3.up * (size * arrowOffset + extraHeightOffset * worldScale);
	}

	public void UpdateCameraPosition(Vector3 newCameraPosition)
	{
		cameraDistance = Vector3.Distance(newCameraPosition, transform.position);
		UpdateSize();
		
		Vector3 toArrow = transform.position - newCameraPosition;
		toArrow.y = 0.0f;
		if (toArrow.sqrMagnitude <= float.Epsilon) { return; }
		toArrow.Normalize();

		transform.rotation = Quaternion.FromToRotation(Vector3.forward, toArrow);
	}

	public void UpdateWorldScale(float newScale)
	{
		worldScale = newScale;
		UpdateSize();
	}

	public void UpdateExtraHeightOffset(float newExtraHeightOffset)
	{
		extraHeightOffset = newExtraHeightOffset;
		UpdateSize();
	}

	private void Update()
	{
		float goalVisibilityScaleMultiplier = (cameraDistance < cutoffDistance * worldScale) ? 0.0f : 1.0f;
		visibilityScaleMultiplier = Damp(visibilityScaleMultiplier, goalVisibilityScaleMultiplier, scaleLerpLambda, Time.deltaTime);
		UpdateSize();
	}

	private float Damp(float a, float b, float lambda, float dt)
	{
		return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
	}

}
