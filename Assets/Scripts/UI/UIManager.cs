using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform titleScreen;
    public RectTransform endOfLevelScreen;
    public TextMeshProUGUI endOfLevelTitle;
    public Scorecard endOfLevelScorecard;
    public RectTransform gameplayOverlay;
    public TextMeshProUGUI gameplayHoleText;
    public TextMeshProUGUI gameplayParText;
    public TextMeshProUGUI gameplayStrokeText;
    public RectTransform endGameScreen;
    public Scorecard endGameScorecard;

	private void Awake()
	{
        HideTitleScreen();
        HideEndOfLevel();
        HideGameplayOverlay();
        HideEndGameScreen();
	}

    public void ShowTitleScreen()
    {
		titleScreen.gameObject.SetActive(true);
	}

    public void HideTitleScreen()
    {
		titleScreen.gameObject.SetActive(false);
	}

	public void ShowEndOfLevel(string message, List<HoleInfo> holes, List<HoleResult> results, int currentHole)
    {
        endOfLevelScreen.gameObject.SetActive(true);
        endOfLevelTitle.text = message;
		endOfLevelScorecard.Display(holes, results, currentHole);
    }

    public void HideEndOfLevel()
    {
        endOfLevelScreen.gameObject.SetActive(false);
    }

    public void ShowGameplayOverlay(HoleInfo hole)
    {
		gameplayOverlay.gameObject.SetActive(true);
        gameplayParText.text = hole.par.ToString();
        gameplayHoleText.text = (hole.index + 1).ToString();
        gameplayStrokeText.text = "1";
	}

	public void UpdateGameplayOverlayStroke(int stroke)
    {
        gameplayStrokeText.text = stroke.ToString();
    }

    public void HideGameplayOverlay()
    {
        gameplayOverlay.gameObject.SetActive(false);
    }

    public void ShowEndGameScreen(List<HoleInfo> holes, List<HoleResult> results)
	{
		endGameScreen.gameObject.SetActive(true);
		endGameScorecard.Display(holes, results, -1);
	}

    public void HideEndGameScreen()
    {
        endGameScreen.gameObject.SetActive(false);
    }

}
