using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBall : MonoBehaviour
{
	public static HashSet<FollowBall> all = new();

	public Vector3 multiplier = Vector3.one;

	private Vector3 startPosition;
	private Vector3 relativePosition;

	private void Awake()
	{
		all.Add(this);
	}

	private void OnDestroy()
	{
		all.Remove(this);
	}

	public void InitializeRelativePosition(Vector3 initialBallPos)
	{
		startPosition = transform.position;
		relativePosition = startPosition - initialBallPos;
	}

	public void UpdateBallPosition(Vector3 newBallPos)
	{
		Vector3 transformedPosition = newBallPos + relativePosition;
		transform.position = LerpByVector(startPosition, transformedPosition, multiplier);
	}

	private static Vector3 LerpByVector(Vector3 a, Vector3 b, Vector3 t)
	{
		return new Vector3(
			Mathf.Lerp(a.x, b.x, t.x),
			Mathf.Lerp(a.y, b.y, t.y),
			Mathf.Lerp(a.z, b.z, t.z)
		);
	}
}
