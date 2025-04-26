using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientBounds : MonoBehaviour
{
	private void Start()
	{	
		CalculateBounds();
	}

	private void CalculateBounds()
	{
		Vector3 center = transform.position;
		Vector3 extents = transform.lossyScale;
		Vector3 minPosition = center - (extents / 2.0f);
		float maxDistance = extents.x + extents.z;
		GameManager.instance.InitializeGradient(minPosition, maxDistance);
	}
}
