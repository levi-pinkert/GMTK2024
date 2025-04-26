using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaintainWorldPosition : MonoBehaviour
{
    public static HashSet<MaintainWorldPosition> all = new();

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

	public void InitializeWorldTransform(Transform startWorldTransform)
	{
		startPosition = transform.position;
		relativePosition = startWorldTransform.InverseTransformPoint(startPosition);
	}

	public void UpdateWorldTransform(Transform newWorldTransform)
	{
		Vector3 transformedPosition = newWorldTransform.TransformPoint(relativePosition);
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
