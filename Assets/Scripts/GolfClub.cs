using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfClub : MonoBehaviour
{
	public enum State
	{
		Idle,
		Aiming,
		Swinging
	}

	[Header("Refs")]
	public GolfPlayer golfPlayer;
	public GameObject rotationParent;
	[Header("Params")]
	public float maxAimRotation;
	public float maxFollowThroughRotation;
	public float idleLerpLambda;
	public float aimLerpLambda;
	public float rotationRoll;
	public float swingAcceleration;
	public float swingDeceleration;

	private State state = State.Idle;
	private Vector3 baseRotation;   // local rotation of rotationParent, in euler angles
	private float aimPowerLevel = 0.0f;
	private float currentAngle = 0.0f;
	private float swingVelocity = 0.0f;
	private bool hasTriggeredBallMovement = false;

	private void Awake()
	{
		SetAngle(0.0f);
	}

	public void UpdateGolfClub()
	{
		if (state == State.Idle)
		{
			SetAngle(Damp(currentAngle, 0.0f, idleLerpLambda, Time.deltaTime));
		}
		else if (state == State.Aiming)
		{
			float goalAngle = aimPowerLevel * maxAimRotation;
			SetAngle(Damp(currentAngle, goalAngle, aimLerpLambda, Time.deltaTime));
		}
		else if (state == State.Swinging)
		{
			if (currentAngle <= 0.0f)
			{
				swingVelocity += swingDeceleration * Time.deltaTime;
				swingVelocity = Mathf.Min(swingVelocity, 0.0f);
			}
			else
			{
				swingVelocity += -1.0f * swingAcceleration * Time.deltaTime;
			}

			float newAngle = currentAngle + swingVelocity * Time.deltaTime;
			newAngle = Mathf.Max(-maxFollowThroughRotation, newAngle);
			SetAngle(newAngle);

			if(!hasTriggeredBallMovement && currentAngle <= 0.0f)
			{
				golfPlayer.OnClubReachBall();
				hasTriggeredBallMovement = true;
			}
		}
	}

	public void SetPosition(Vector3 bottomOfBall, float yaw)
	{
		transform.position = bottomOfBall;
		transform.rotation = Quaternion.Euler(Vector3.up * yaw);
	}

	public void BeginAiming()
	{
		state = State.Aiming;
		aimPowerLevel = 0.0f;
	}

	public void UpdateAiming(float powerLevel)
	{
		if (state == State.Aiming)
		{
			aimPowerLevel = powerLevel;
		}
	}

	public void BeginSwinging()
	{
		if (state == State.Aiming)
		{
			swingVelocity = 0.0f;
			hasTriggeredBallMovement = false;
			state = State.Swinging;
		}
	}

	public void CancelAiming()
	{
		state = State.Idle;
	}

	public void ResetGolfClub()
	{
		SetAngle(0.0f);
		state = State.Idle;
	}

	private void SetAngle(float angle)
	{
		Quaternion rollRot = Quaternion.Euler(Vector3.forward * rotationRoll * (angle / maxAimRotation));
		Quaternion pitchRot = Quaternion.Euler(Vector3.right * angle);
		rotationParent.transform.localRotation = pitchRot * rollRot;
		currentAngle = angle;
	}

	private float Damp(float a, float b, float lambda, float dt)
	{
		return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
	}
}
