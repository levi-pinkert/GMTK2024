using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GolfInput : MonoBehaviour
{
	public class Timestamp<T>
	{
		public float timestamp;
		public T data;
	}

	public float mouseScrollThreshold;
	public float scaleBufferTime;

	private List<Timestamp<int>> scaleInputBuffer = new();

	public void UpdateInput()
	{
		UpdateScaleInput();
		
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			GameManager.instance.LevelComplete(true, -1);
		}
	}

	private void UpdateScaleInput()
	{
		// Drop old scale inputs
		float currentTime = GetTime();
		scaleInputBuffer.RemoveAll(t => currentTime - t.timestamp > scaleBufferTime);

		// Buffer scale inputs
		int scaleInput = 0;
		if (Input.GetKeyDown(KeyCode.W)) { scaleInput++; }
		if (Input.GetKeyDown(KeyCode.UpArrow)) { scaleInput++; }
		if (Input.GetKeyDown(KeyCode.S)) { scaleInput--; }
		if (Input.GetKeyDown(KeyCode.DownArrow)) { scaleInput--; }
		float scrollInput = Input.mouseScrollDelta.y;
		if (scrollInput > mouseScrollThreshold) { scaleInput++; }
		if (scrollInput < -mouseScrollThreshold) { scaleInput--; }
		scaleInput = Mathf.Max(Mathf.Min(1, scaleInput), -1);
		if (scaleInput != 0)
		{
			scaleInputBuffer.Add(new Timestamp<int>()
			{
				timestamp = GetTime(),
				data = scaleInput
			});
		}
	}

	public int GetScaleInput()
	{
		while(scaleInputBuffer.Count > 0)
		{
			float timestamp = scaleInputBuffer[0].timestamp;
			int scaleInput = scaleInputBuffer[0].data;
			scaleInputBuffer.RemoveAt(0);
			if(GetTime() - timestamp <= scaleBufferTime)
			{
				return scaleInput;
			}
		}
		return 0;
	}

	private float GetTime()
	{
		return Time.time;
	}
}
