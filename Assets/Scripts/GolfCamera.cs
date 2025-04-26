using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolfCamera : MonoBehaviour
{
    [Header("References")]
    public GolfPlayer player;
    [Header("Tweaks")]
    public float backOffset;
    public float upOffset;
    public float maxRightOffset;
    public float minPitch;
    public float maxPitch;
    public float sensitivity;
    public float sideOffsetLambda;
    public float raycastOffset;
    public LayerMask raycastLayerMask;

    [HideInInspector]
    public float yaw = 0.0f;
    [HideInInspector]
    public float pitch = 0.0f;
    private float sideOffsetProgress = 0.0f;

	private const string xInputName = "Mouse X";
	private const string yInputName = "Mouse Y";
	
	private void Start()
    {
		UpdatePosition();
	}

    // Called by GolfPlayer, after it moves
	public void CameraUpdate(bool canCameraPitch, bool doSideOffset)
	{
        UpdateInput(canCameraPitch, doSideOffset);
		UpdatePosition();
	}

    private void UpdateInput(bool canCameraPitch, bool doSideOffset)
    {
		float sideOffsetGoalProg = doSideOffset ? 1.0f : 0.0f;
		sideOffsetProgress = Damp(sideOffsetProgress, sideOffsetGoalProg, sideOffsetLambda, Time.deltaTime);

		if (!Application.isFocused) { return; }

        yaw += Input.GetAxis(xInputName) * sensitivity;
        if (canCameraPitch)
        {
            pitch += -1.0f * Input.GetAxis(yInputName) * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
	}

    private void UpdatePosition()
    {
        Quaternion cameraRotation = Quaternion.Euler(pitch, yaw, 0.0f);
        transform.rotation = cameraRotation;
        Vector3 ballPosition = player.transform.position;
        Vector3 back = cameraRotation * Vector3.back;
        Vector3 up = cameraRotation * Vector3.up;
        Vector3 right = cameraRotation * Vector3.right;
        // When we look at the ball from the top, we want to offset less by the up vector
        float upOffsetScale = 1.0f - ((pitch - minPitch) / (maxPitch - minPitch));
		Vector3 offset = (up * upOffset * upOffsetScale) + (back * backOffset) + (right * sideOffsetProgress * maxRightOffset);

		// Raycast so that we keep the camera out of the wall
		// We decompose the vector into 3 parts, and do 3 raycasts in series to get the final point
		// These 3 directions are slightly different than the 3 vectors we added to create "offset", since the up component is the global up vector, not the rotated up vector
		Vector3 upComponent = Vector3.up * offset.y;
        offset -= upComponent;
        Vector3 rightComponent = right * Vector3.Dot(right, offset);
        offset -= rightComponent;
        Vector3 backComponent = offset;

        Vector3 newCameraPosition = ballPosition;
        newCameraPosition += RaycastOffset(newCameraPosition, upComponent);
        newCameraPosition += RaycastOffset(newCameraPosition, rightComponent);
        newCameraPosition += RaycastOffset(newCameraPosition, backComponent);
		transform.position = newCameraPosition;

        // Update the flag arrow, so that it can always be pointed towards the camera
        foreach (FlagArrow arrow in FlagArrow.all)
        {
            arrow.UpdateCameraPosition(transform.position);
        }
    }

    private float Damp(float a, float b, float lambda, float dt)
    {
        return Mathf.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
	}

    private Vector3 RaycastOffset(Vector3 worldPosition, Vector3 offset)
    {
        float distance = offset.magnitude;
        if (distance <= float.Epsilon)
        {
            return Vector3.zero;
        }

        Vector3 direction = offset / distance;
        Ray ray = new(worldPosition, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, distance + raycastOffset, raycastLayerMask))
        {
            return direction * Mathf.Max(0.0f, hit.distance - raycastOffset);
        }
        else
        {
            return offset;
        }
    }
}
