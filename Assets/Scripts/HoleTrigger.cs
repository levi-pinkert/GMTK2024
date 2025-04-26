using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleTrigger : MonoBehaviour
{
	public float overlapTimeThreshold;

	private bool hasCompleted = false;
	private float overlapTime = 0.0f;
	private GolfBall overlappingBall;

	private void OnTriggerEnter(Collider other)
	{
		GolfBall collidedGolfBall = FindGolfBallComponent(other);
		if (overlappingBall == null && collidedGolfBall != null)
		{
			overlappingBall = collidedGolfBall;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		GolfBall collidedGolfBall = FindGolfBallComponent(other);
		if (overlappingBall == collidedGolfBall)
		{
			overlappingBall = null;
		}
	}

	private GolfBall FindGolfBallComponent(Collider other)
	{
		GolfBall collidedGolfBall = other.GetComponent<GolfBall>();
		if(collidedGolfBall != null)
		{
			return collidedGolfBall;
		}
		if (other.transform.parent != null)
		{
			return other.transform.parent.GetComponent<GolfBall>();
		}
		return null;
	}

	private void Update()
	{
		if (hasCompleted)
		{
			return;
		}

		if (overlappingBall == null)
		{
			overlapTime = 0.0f;
		}
		else
		{
			overlapTime += Time.deltaTime;
			if (overlapTime > overlapTimeThreshold)
			{
				hasCompleted = true;
				overlappingBall.golfPlayer.OnLevelCompletion();
			}
		}
	}
}
