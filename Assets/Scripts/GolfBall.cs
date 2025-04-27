using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfBall : MonoBehaviour
{
	[Header("Refs")]
	public GolfPlayer golfPlayer;
	public GameObject water;
	public string grassTag;
	public string obstacleTag;
	[Header("Params")]
	public float baseRadius;
	public Material invalidMaterial;
	public float triggerModeRadiusDelta;
	public float grassForce;
	[Header("Audio")]
	public float impactSoundVelocityCutoff;
	public List<AudioClip> impactSounds;
	public float minImpactSoundVolume;
	public float maxImpactSoundVolume;
	public float velocityForMaxImpactSoundVolume;
	public AudioClip waterSound;

	private Rigidbody ballRigidbody;
	private SphereCollider ballCollider;
	private MeshRenderer meshRenderer;
	private Material baseMaterial;
	private int triggerOverlapCount = 0;
	private int rbGrassCollisionCount = 0;
	private float baseColliderRadius;
	private bool triggerModeEnabled = false;
	private bool initializedTriggerMode = false;

	private void Awake()
	{
		ballRigidbody = GetComponent<Rigidbody>();
		ballCollider = GetComponentInChildren<SphereCollider>();
		baseColliderRadius = ballCollider.radius;
		meshRenderer = GetComponentInChildren<MeshRenderer>();
		baseMaterial = meshRenderer.material;

		SetTriggerModeEnabled(false);
	}

	private void Start()
	{
		foreach (FollowBall followBall in FollowBall.all)
		{
			followBall.InitializeRelativePosition(transform.position);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger) { return; }
		triggerOverlapCount++;
		UpdateTriggerMaterial();
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.isTrigger) { return; }
		triggerOverlapCount = Mathf.Max(0, triggerOverlapCount - 1);
		UpdateTriggerMaterial();
	}

	private void OnCollisionEnter(Collision collision)
	{
		string tag = collision.gameObject.tag;
		if (tag == grassTag)
		{
			rbGrassCollisionCount++;
		}

		float velocity = collision.relativeVelocity.magnitude;
		if (velocity >= impactSoundVelocityCutoff)
		{
			AudioManager.PlayRandomSound(impactSounds, Mathf.Lerp(minImpactSoundVolume, maxImpactSoundVolume, (velocity / velocityForMaxImpactSoundVolume)));
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.tag == grassTag)
		{
			rbGrassCollisionCount = Mathf.Max(0, rbGrassCollisionCount - 1);
		}
	}

	private void FixedUpdate()
	{
		// Simulate deceleration caused by texture of grass
		if (!triggerModeEnabled && rbGrassCollisionCount > 0)
		{
			Vector3 velocity = ballRigidbody.velocity;
			float speed = velocity.magnitude;
			if (speed > 0)
			{
				float grassForceMagnitude = Time.deltaTime * grassForce;
				grassForceMagnitude = Mathf.Min(speed, grassForceMagnitude);
				ballRigidbody.AddForce(grassForceMagnitude * -1.0f * velocity.normalized);
			}
		}

		// Check if we've fallen into water
		if (transform.position.y <= water.transform.position.y)
		{
			AudioManager.PlaySound(waterSound);
			golfPlayer.OnBallDeath();
		}
	}

	private void Update()
	{
		UpdateTriggerMaterial();
		foreach (FollowBall followBall in FollowBall.all)
		{
			followBall.UpdateBallPosition(transform.position);
		}
	}

	public void Swing(float yaw, float pitch, float speed)
	{
		Quaternion rotation = Quaternion.Euler(pitch, yaw, 0.0f);
		Vector3 direction = rotation * Vector3.forward;
		ballRigidbody.velocity = direction * speed;
	}

	public float GetRadius()
	{
		return baseRadius * transform.localScale.x;
	}

	public Vector3 GetBottomPosition()
	{
		return transform.position + Vector3.down * GetRadius();
	}

	public void SetTriggerModeEnabled(bool inTriggerModeEnabled)
	{
		if (initializedTriggerMode && inTriggerModeEnabled == triggerModeEnabled) { return; }
		triggerModeEnabled = inTriggerModeEnabled;
		initializedTriggerMode = true;

		// I assume that Unity won't ever update overlaps between these sets
		// However, there's a chance that it will, so I've tried to order the changes to make that minimally desctructive
		triggerOverlapCount = 0;
		rbGrassCollisionCount = 0;
		ballRigidbody.isKinematic = triggerModeEnabled;
		ballRigidbody.useGravity = !triggerModeEnabled;
		ballCollider.radius = baseColliderRadius + (triggerModeEnabled ? triggerModeRadiusDelta : 0.0f);
		ballCollider.isTrigger = triggerModeEnabled;
		UpdateTriggerMaterial();
	}

	public void UpdateTriggerMaterial()
	{
		Material newMat = IsOverlappingAnything() ? invalidMaterial : baseMaterial;
		if(meshRenderer.material != newMat)
		{
			meshRenderer.material = newMat;
		}
	}

	public bool IsOverlappingAnything()
	{
		return triggerOverlapCount > 0 && !golfPlayer.ShouldIgnoreBallCollisions();
	}

	public float GetSpeed()
	{
		return ballRigidbody.velocity.magnitude;
	}
}
