using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScalingWorld : MonoBehaviour
{
    private float currentScale = 1.0f;

	private void Start()
	{
		foreach (MaintainWorldPosition mwp in MaintainWorldPosition.all)
        {
            mwp.InitializeWorldTransform(transform);
        }

		GameManager.instance.OnWorldResize(transform.position, currentScale);
	}

	public void ChangeScale(Vector3 focusPoint, float newScale)
    {
        float scaleRatio = newScale / currentScale;
        Vector3 currentToFocus = focusPoint - transform.position;
        Vector3 newToFocus = currentToFocus * scaleRatio;
        
        transform.position = focusPoint - newToFocus;
        transform.localScale = Vector3.one * newScale;
        currentScale = newScale;

        foreach (MaintainWorldPosition mwp in MaintainWorldPosition.all)
        {
            if (mwp != null)
            {
                mwp.UpdateWorldTransform(transform);
            }
        }
        foreach (FlagArrow flagArrow in FlagArrow.all)
        {
            if (flagArrow != null)
            {
                flagArrow.UpdateWorldScale(newScale);
            }
        }

        GameManager.instance.OnWorldResize(transform.position, currentScale);
    }

    public float GetCurrentScale()
    {
        return currentScale;
    }
}
