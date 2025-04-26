using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfHole : MonoBehaviour
{
	public float extraArrowDistance = 0.0f;
	public GameObject flagArrowPrefab;
	public float liftRadiusScaling;
	public float liftRadiusConstant;
	public GameObject flagParent;
	public float flagMoveLambda;
	public float liftOffset;

	private GolfBall ball;
	private ScalingWorld scalingWorld;
	private Vector3 baseFlagOffset;
	private float liftProgress = 0.0f;

	private void Awake()
	{
		GameObject flagInstance = Instantiate(flagArrowPrefab, transform.position, Quaternion.identity);
		FlagArrow flagArrowInstance = flagInstance.GetComponent<FlagArrow>();
		flagArrowInstance.UpdateExtraHeightOffset(extraArrowDistance);
	}

	private void Start()
	{
		baseFlagOffset = flagParent.transform.localPosition;
		ball = FindObjectOfType<GolfBall>();
		scalingWorld = FindObjectOfType<ScalingWorld>();
	}

	// Doing this in Update probably will cause frame delays, but it doesn't matter at all
	// because this is a purely cosmetic effect, and it has a visual delay anyways
	private void Update()
	{
		bool flagIsDown = true;
		if (ball != null && scalingWorld != null)
		{
			float distanceThreshold = liftRadiusConstant + scalingWorld.GetCurrentScale() * liftRadiusScaling;
			Vector3 toBall = ball.transform.position - transform.position;
			toBall.y = 0.0f;
			flagIsDown = toBall.magnitude > distanceThreshold;
		}

		liftProgress = Damp(liftProgress, flagIsDown ? 0.0f : 1.0f, flagMoveLambda, Time.deltaTime);
		flagParent.transform.localPosition = baseFlagOffset + Vector3.up * liftOffset * liftProgress;
	}

	private float Damp(float a, float b, float lambda, float dt)
	{
		return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
	}
}
