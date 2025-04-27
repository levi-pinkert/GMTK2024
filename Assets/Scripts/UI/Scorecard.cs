using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Scorecard : MonoBehaviour
{
	[Header("Refs")]
	public Image currentRoundSelector;
	public GameObject holeTextPrefab;
	public GameObject scoreTextPrefab;
	public TextMeshProUGUI totalParText;
	public TextMeshProUGUI totalScoreText;
	[Header("Config")]
	public int cellCountWidth = 18;
	public int cellCountHeight = 3;
	public float cellSize = 64.0f;

	private TextMeshProUGUI[][] textGrid = null;

	private void Awake()
	{
		SpawnTextGrid();
		currentRoundSelector.gameObject.SetActive(false);
	}

	private void SpawnTextGrid()
	{
		textGrid = new TextMeshProUGUI[cellCountHeight][];
		for (int i = 0; i < cellCountHeight; i++)
		{
			bool isHole = (i == 0);
			textGrid[i] = new TextMeshProUGUI[cellCountWidth];
			for (int j = 0; j < cellCountWidth; j++)
			{
				GameObject prefab = isHole ? holeTextPrefab : scoreTextPrefab;
				GameObject instance = Instantiate(prefab, transform);
				TextMeshProUGUI instanceText = instance.GetComponent<TextMeshProUGUI>();
				RectTransform rectTransform = instance.GetComponent<RectTransform>();

				rectTransform.anchoredPosition = GetCellOffset(i, j);
				instanceText.text = isHole ? (j + 1).ToString() : "";
				textGrid[i][j] = instanceText;
			}
		}
	}

	private Vector2 GetCellOffset(int row, int col)
	{
		Vector2 topLeft = (Vector2.left * ((cellCountWidth / 2) - 0.5f)) + (Vector2.up * (cellCountHeight / 2));
		Vector2 pos = topLeft + (Vector2.right * col) + (Vector2.down * row);
		return pos * cellSize;
	}

	public void Display(List<HoleInfo> holes, List<HoleResult> results, int currentHole)
	{
		int totalPar = 0;
		int totalScore = 0;
		for (int i = 0; i < holes.Count; i++)
		{
			TextMeshProUGUI holeText = textGrid[0][i];
			TextMeshProUGUI parText = textGrid[1][i];
			TextMeshProUGUI scoreText = textGrid[2][i];

			HoleInfo info = holes[i];
			HoleResult result = results[i];
			holeText.text = (info.index + 1).ToString();
			if (!result.isAttempted)
			{
				parText.text = "";
				scoreText.text = "";
			}
			else if (result.isCompleted)
			{
				parText.text = info.par.ToString();
				scoreText.text = result.strokes.ToString();
				totalPar += info.par;
				totalScore += result.strokes;
			}
			else
			{
				parText.text = "X";
				scoreText.text = "X";
			}
		}

		totalParText.text = totalPar.ToString();
		totalScoreText.text = totalScore.ToString();

		if (currentHole == -1)
		{
			currentRoundSelector.gameObject.SetActive(false);
		}
		else
		{
			currentRoundSelector.rectTransform.anchoredPosition = GetCellOffset(1, currentHole);
			currentRoundSelector.gameObject.SetActive(true);
		}
	}
}
