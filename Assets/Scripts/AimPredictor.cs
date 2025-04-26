using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimPredictor : MonoBehaviour
{
	public GameObject predictorBallPrefab;
	public LineRenderer lineRenderer;
	public float gravity;
	public AnimationCurve timeStepByDistance;
	public float maxDistance;

	public void UpdateAimPredictor(bool visible, float speed, float yaw, float pitch, Vector3 startPosition)
	{
		lineRenderer.enabled = visible;
		if(!visible)
		{
			return;
		}
		
		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0.0f);
		Vector3 velocity = (rotation * Vector3.forward) * speed;
		Vector3 position = startPosition;

		List<Vector3> lineRendererPositions = new()
		{
			startPosition
		};
		float totalDistance = 0.0f;
		while (totalDistance < maxDistance)
		{
			float timeStep = timeStepByDistance.Evaluate(totalDistance);
			velocity += Vector3.down * gravity * timeStep;
			position += velocity * timeStep;
			lineRendererPositions.Add(position);
			totalDistance += velocity.magnitude * timeStep;
		}
		lineRenderer.positionCount = lineRendererPositions.Count;
		lineRenderer.SetPositions(lineRendererPositions.ToArray());
	}
}
