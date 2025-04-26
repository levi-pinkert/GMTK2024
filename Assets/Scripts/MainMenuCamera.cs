using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public float rotationRate;
	public Vector3 offsetToScaleFocus;
	public AnimationCurve scaleAnimation;
	public ScalingWorld scalingWorld;

	private float timer = 0.0f;

	private void Update()
	{
		// Rotate
		transform.rotation *= Quaternion.AngleAxis(rotationRate * Time.deltaTime, Vector3.up);

		// Scale world
		timer += Time.deltaTime;
		float maxTime = scaleAnimation.keys[scaleAnimation.length - 1].time;
		if (timer >= maxTime)
		{
			timer -= maxTime;
		}
		scalingWorld.ChangeScale(transform.position + offsetToScaleFocus, Mathf.Pow(2.0f, scaleAnimation.Evaluate(timer)));
	}
}
