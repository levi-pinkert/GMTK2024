using System.Collections.Generic;
using UnityEngine;

public class GolfPlayer : MonoBehaviour
{
	public enum State
	{
		Idle,
		Aim,
		Scale,
		Swing,
		Move,
		Respawning,
		Done
	}

	[Header("Refs")]
	public GolfBall golfBall;
	public GolfCamera golfCamera;
	public AimPredictor aimPredictor;
	public GolfClub golfClub;
	[Header("Aim/Swing Params")]
	public float initialPowerLevel;
	public float minPowerLevel;
	public float powerLevelSensitivity;
	public float swingHoldTimeThreshold;
	public float powerLevelCumulativeDeltaThreshold;
	public float minAimPitch;
	public float maxAimPitch;
	public float maxBallSpeed;
	[Header("Scale Params")]
	public float scaleBase;
	public float scaleAnimDuration;
	public AnimationCurve scaleAnim;
	public int minScaleLevel;
	public int maxScaleLevel;
	[Header("Movement Params")]
	public float minMoveStateDuration;
	public float ballStoppingSpeed;
	public float ballStoppingTime;
	[Header("Respawn")]
	public float respawnDuration;
	[Header("Audio")]
	public List<AudioClip> lightHitSounds;
	public float mediumHitPowerCutoff;
	public List<AudioClip> mediumHitSounds;
	public float heavyHitPowerCutoff;
	public List<AudioClip> heavyHitSounds;
	public AudioClip swingSwooshSound;
	public AudioClip scaleUpSound;
	public AudioClip scaleDownSound;
	public AudioClip failedInput;

	private const string powerInputName = "Mouse Y";

	private State state;
	private int currentStroke = 1;
	private float powerLevel = 0.0f;
	private float powerLevelCumulativeDelta = 0.0f;
	private float aimHoldTime = 0.0f;
	private float aimYaw = 0.0f;
	private ScalingWorld scalingWorld;
	private GolfInput golfInput;
	private int scaleLevel = 0;   // worldScale = scaleBase ^ scaleLevel
	private int nextScaleLevel = 0;
	private int originalScaleLevel = 0;
	private float scaleProgress = 0.0f;
	private float worldScale = 1.0f;
	private float moveTime = 0.0f;
	private float stoppedTime = 0.0f;
	private Vector3 lastSwingPosition;
	private float respawnTimer = 0.0f;
	private bool lastMouseInput = false;
	private bool canDoMouseInput = false;

	private void Awake()
	{
		scalingWorld = FindAnyObjectByType<ScalingWorld>();
		golfInput = GetComponent<GolfInput>();
	}

	private void Start()
	{
		BeginIdleState();
	}

	private void LateUpdate()
	{
		// At the very beginning of the level, we don't want to accidentally use a leftover hold from the previous scene
		// So, don't accept any mouse inputs until there's a frame without a mouse input
		if (!canDoMouseInput && !Input.GetMouseButton(0))
		{
			canDoMouseInput = true;
		}

		// Follow the golf ball
		transform.position = golfBall.transform.position;

		// Buffer inputs, etc
		golfInput.UpdateInput();

		// Updating camera before everything else keeps aiming perfectly aligned with camera direction
		bool bIsAimState = state == State.Aim;
		golfCamera.CameraUpdate(!bIsAimState, bIsAimState);

		if (state == State.Idle)
		{
			UpdateIdleState();
		}
		
		// If we exit the idle state, then we should do the update for the new state this frame
		switch (state)
		{
			case State.Aim:
				UpdateAimState();
				break;
			case State.Scale:
				UpdateScaleState();
				break;
			case State.Move:
				UpdateMoveState();
				break;
			case State.Respawning:
				UpdateRespawnState();
				break;
			case State.Done:
				UpdateDoneState();
				break;
		}

		// Updating the golf club at the end gives immediate feedback for any inputs
		if (state == State.Idle || state == State.Aim || state == State.Scale)
		{
			golfClub.SetPosition(golfBall.GetBottomPosition(), golfCamera.yaw);
		}
		golfClub.UpdateGolfClub();

		lastMouseInput = IsMouseInputDown();
	}

	private bool IsMouseInputDown()
	{
		return canDoMouseInput && Input.GetMouseButton(0);
	}

	private float GetAimedPitch()
	{
		return -Mathf.Lerp(minAimPitch, maxAimPitch, powerLevel);
	}

	private void IncrementStroke()
	{
		currentStroke++;
		GameManager.instance.GetUI().UpdateGameplayOverlayStroke(currentStroke);
	}

	// Called by GolfBall when it hits the water
	public void OnBallDeath()
	{
		if (state == State.Move)
		{
			BeginRespawn();
		}
	}

	// Called by HoleTrigger when a golf ball enters the hole
	public void OnLevelCompletion()
	{
		if (state != State.Done)
		{
			BeginDone();
			GameManager.instance.LevelComplete(false, currentStroke);
		}
	}

	public bool ShouldIgnoreBallCollisions()
	{
		return scaleLevel >= originalScaleLevel;
	}

	#region Idle

	private void BeginIdleState()
	{
		state = State.Idle;
		golfBall.SetTriggerModeEnabled(true);
	}

	private void UpdateIdleState()
	{
		bool mouseInput = IsMouseInputDown();
		bool golfBallOverlapping = golfBall.IsOverlappingAnything();
		if (!golfBallOverlapping && mouseInput)
		{
			BeginAimState();
		}
		else
		{
			int scaleInput = golfInput.GetScaleInput();
			if(scaleInput != 0)
			{
				int possibleNextScaleLevel = scaleLevel + scaleInput;
				if (possibleNextScaleLevel >= minScaleLevel && possibleNextScaleLevel <= maxScaleLevel)
				{
					nextScaleLevel = possibleNextScaleLevel;
					BeginScaleState();
					AudioManager.PlaySound(scaleInput > 0 ? scaleDownSound : scaleUpSound, 0.5f);
				}
				else
				{
					AudioManager.PlaySound(failedInput, 0.5f);
				}
			}
		}

		if (!lastMouseInput && mouseInput && golfBallOverlapping)
		{
			AudioManager.PlaySound(failedInput, 0.5f);
		}
	}

	#endregion

	#region Aim

	private void BeginAimState()
	{
		state = State.Aim;
		powerLevel = initialPowerLevel;
		powerLevelCumulativeDelta = 0.0f;
		aimHoldTime = 0.0f;
		golfClub.BeginAiming();
	}

	private void UpdateAimState()
	{
		// Process input
		bool mouseInput = IsMouseInputDown();
		float aimPitch = 0.0f;
		if (mouseInput)
		{
			float powerLevelInput = Input.GetAxis(powerInputName) * powerLevelSensitivity;
			powerLevel += powerLevelInput;
			powerLevel = Mathf.Clamp01(powerLevel);
			powerLevelCumulativeDelta += Mathf.Abs(powerLevelInput);
			aimHoldTime += Time.deltaTime;
			aimPitch = GetAimedPitch();
			golfClub.UpdateAiming(powerLevel >= minPowerLevel ? powerLevel : 0.0f);
		}
		else
		{
			bool heldTimeGood = aimHoldTime >= swingHoldTimeThreshold;
			bool cumulativeDeltaGood = powerLevelCumulativeDelta >= powerLevelCumulativeDeltaThreshold;
			if((heldTimeGood || cumulativeDeltaGood) && powerLevel >= minPowerLevel)
			{
				aimYaw = golfCamera.yaw;
				BeginSwingState();
			}
			else
			{
				golfClub.CancelAiming();
				BeginIdleState();
			}
		}

		// Update the indicator
		bool showAimPredictor = (state == State.Aim) && powerLevel >= minPowerLevel;
		aimPredictor.UpdateAimPredictor(showAimPredictor, powerLevel * maxBallSpeed, golfCamera.yaw, aimPitch, transform.position);
	}

	#endregion

	#region Scale

	private void BeginScaleState()
	{
		state = State.Scale;
		scaleProgress = 0.0f;
	}

	private void UpdateScaleState()
	{
		scaleProgress += Time.deltaTime / scaleAnimDuration;
		float startScale = Mathf.Pow(scaleBase, scaleLevel);
		float endScale = Mathf.Pow(scaleBase, nextScaleLevel);
		if(scaleProgress >= 1.0f)
		{
			worldScale = endScale;
			scalingWorld.ChangeScale(golfBall.GetBottomPosition(), worldScale);
			scaleLevel = nextScaleLevel;
			BeginIdleState();
		}
		else
		{
			worldScale = Mathf.Lerp(startScale, endScale, scaleAnim.Evaluate(scaleProgress));
			scalingWorld.ChangeScale(golfBall.GetBottomPosition(), worldScale);
		}
	}

	#endregion

	#region Move

	private void BeginMovingState()
	{
		state = State.Move;
		golfBall.SetTriggerModeEnabled(false);
		if (powerLevel >= minPowerLevel)
		{
			lastSwingPosition = transform.position;
			golfBall.Swing(aimYaw, GetAimedPitch(), powerLevel * maxBallSpeed);

			List<AudioClip> hitClips;
			if (powerLevel >= heavyHitPowerCutoff)
			{
				hitClips = heavyHitSounds;
			}
			else if (powerLevel >= mediumHitPowerCutoff)
			{
				hitClips = mediumHitSounds;
			}
			else
			{
				hitClips = lightHitSounds;
			}
			AudioManager.PlayRandomSound(hitClips);
		}
		moveTime = 0.0f;
		stoppedTime = 0.0f;
	}

	private void UpdateMoveState()
	{
		moveTime += Time.deltaTime;
		float ballSpeed = golfBall.GetSpeed();
		if(ballSpeed <= ballStoppingSpeed)
		{
			stoppedTime += Time.deltaTime;
		}
		else
		{
			stoppedTime = 0;
		}

		if (moveTime >= minMoveStateDuration && stoppedTime >= ballStoppingTime)
		{
			IncrementStroke();
			golfClub.ResetGolfClub();
			originalScaleLevel = scaleLevel;
			BeginIdleState();
		}
	}

	#endregion

	#region Swing

	private void BeginSwingState()
	{
		state = State.Swing;
		AudioManager.PlaySound(swingSwooshSound, 0.5f);
		golfClub.BeginSwinging();
	}

	// Called by GolfClub when its swing reaches the ball
	public void OnClubReachBall()
	{
		if (state == State.Swing)
		{
			BeginMovingState();
		}
	}

	#endregion

	#region Respawn

	private void BeginRespawn()
	{
		state = State.Respawning;
		golfBall.gameObject.SetActive(false);
		respawnTimer = 0.0f;
	}

	private void UpdateRespawnState()
	{
		respawnTimer += Time.deltaTime;
		if (respawnTimer >= respawnDuration)
		{
			IncrementStroke();
			golfBall.gameObject.SetActive(true);
			golfBall.transform.position = lastSwingPosition;
			golfClub.ResetGolfClub();
			BeginIdleState();
		}
	}

	#endregion

	#region Done

	private void BeginDone()
	{
		state = State.Done;
	}

	private void UpdateDoneState()
	{
		
	}

	#endregion
}
