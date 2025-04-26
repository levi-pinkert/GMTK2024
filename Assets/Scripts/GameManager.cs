using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class WorldMaterialType
{
	public string tagName;
	public PhysicMaterial physicMaterial;
	public Material renderMaterial;
}

[System.Serializable]
public class HoleInfo
{
	public string name;
	public int par;
	[HideInInspector]
	public int index;
}

public class HoleResult
{
	public bool isAttempted = false;
	public bool isCompleted = false;
	public int strokes = 0;
}

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	[Header("Game Structure")]
	public string mainMenuScene;
	public List<HoleInfo> holes;
	[Header("Level Materials")]
	public List<WorldMaterialType> worldMaterialTypes;
	[Header("Shader Globals")]
	public float gridSize;
	[Tooltip("An offset applied to grid sampling shaders, in the direction of the normal. Avoids noise on exact grid edges.")]
	public float edgeNudge;
	public Vector3 sunDirection;
	[Header("UI")]
	public GameObject uiPrefab;
	[Header("Timings")]
	public float postLevelDelay;
	[Header("Refs")]
	public AudioManager audioManager;

	private int currentHoleIndex = -1;
	private UIManager uiManager;
	private List<HoleResult> scores;
	private bool hasHoleEnded = true;
	private bool gameCompleted = false;
	private Vector3 gradientMinPosition;
	private float gradientMaxDistance;
	private Vector3 worldOrigin = Vector3.zero;
	private float worldScale = 1.0f;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
			return;
		}

		instance = this;
		DontDestroyOnLoad(gameObject);
		SceneManager.activeSceneChanged += OnSceneLoaded;
		InitializeGlobalShaderProperties();
		InitializeHoleData();
		InitializeUI();
	}

	// In-editor, called by EditorStartup
	public void OnEditorStartup()
	{
		InitializeGlobalShaderProperties();
	}

	private void InitializeGlobalShaderProperties()
	{
		Shader.SetGlobalFloat("_GridSize", gridSize);
		Shader.SetGlobalFloat("_EdgeNudge", edgeNudge);
		Shader.SetGlobalVector("_SunDirection", sunDirection.normalized);
	}

	private void InitializeHoleData()
	{
		for (int i = 0; i < holes.Count; i++)
		{
			holes[i].index = i;
		}

		scores = new List<HoleResult>();
		for (int i = 0; i < holes.Count; i++)
		{
			scores.Add(new HoleResult());
		}

		// If we start mid-game (in editor probably), account for that
		Scene startScene = SceneManager.GetActiveScene();
		int startHoleIndex = holes.FindIndex(info => info.name == startScene.name);
		if (startHoleIndex != -1)
		{
			for (int i = 0; i < startHoleIndex; i++)
			{
				scores[i].isAttempted = true;
				scores[i].isCompleted = false;
			}
			currentHoleIndex = startHoleIndex;
		}
	}

	private void InitializeUI()
	{
		GameObject uiManagerInstance = Instantiate(uiPrefab, Vector3.zero, Quaternion.identity);
		DontDestroyOnLoad(uiManagerInstance);
		uiManager = uiManagerInstance.GetComponent<UIManager>();
	}

	private void OnSceneLoaded(Scene oldScene, Scene newScene)
	{
		worldOrigin = Vector3.zero;
		worldScale = 1.0f;
		SetShaderGlobals();

		if (newScene.name == mainMenuScene)
		{
			Cursor.lockState = CursorLockMode.None;
			if (gameCompleted)
			{
				// End screen
				uiManager.ShowEndGameScreen(holes, scores);
			}
			else
			{
				// Start screen (title)
				uiManager.ShowTitleScreen();
			}
		}
		else
		{
			// Begin gameplay
			Cursor.lockState = CursorLockMode.Locked;
			hasHoleEnded = false;
			uiManager.ShowGameplayOverlay((currentHoleIndex == -1) ? holes[0] : holes[currentHoleIndex]);
		}
	}

	public void GoToNextHole()
	{
		if (currentHoleIndex == holes.Count - 1)
		{
			gameCompleted = true;
			currentHoleIndex = -1;
			SceneManager.LoadScene(mainMenuScene);
			return;
		}
		
		if (currentHoleIndex >= 0 && currentHoleIndex < holes.Count - 1)
		{
			currentHoleIndex++;
		}
		else
		{
			currentHoleIndex = 0;
		}
		SceneManager.LoadScene(holes[currentHoleIndex].name);
	}

	public void OnWorldResize(Vector3 newWorldOrigin, float newWorldScale)
	{
		// All of our shaders use world position to make procedural textures.
		// As a result, when the world scales up/down, the textures' scale appears to change.
		// To cancel this out, we have a few uniforms that will let the shader take world scale into account.
		worldOrigin = newWorldOrigin;
		worldScale = newWorldScale;
		SetShaderGlobals();
	}

	private void SetShaderGlobals()
	{
		Shader.SetGlobalVector("_GridOrigin", worldOrigin);
		Shader.SetGlobalFloat("_GridSize", worldScale * gridSize);
		Shader.SetGlobalVector("_GradientMinPosition", gradientMinPosition * worldScale + worldOrigin);
		Shader.SetGlobalFloat("_GradientMaxDistance", worldScale * gradientMaxDistance);
	}

	public void LevelComplete(bool skipped, int strokes)
	{
		if (hasHoleEnded) { return; }
		hasHoleEnded = true;

		string message = "Unknown current hole";
		if (currentHoleIndex != -1)
		{
			scores[currentHoleIndex].isAttempted = true;
			
			if (skipped)
			{
				message = $"Hole {currentHoleIndex + 1} Skipped";
				scores[currentHoleIndex].isCompleted = false;
				scores[currentHoleIndex].strokes = -1;
			}
			else
			{
				message = $"Hole {currentHoleIndex + 1} Complete";
				scores[currentHoleIndex].isCompleted = true;
				scores[currentHoleIndex].strokes = strokes;
			}
		}
		StartCoroutine(EndLevelCoroutine(message));
	}

	private IEnumerator EndLevelCoroutine(string message)
	{
		uiManager.HideGameplayOverlay();
		uiManager.ShowEndOfLevel(message, holes, scores, currentHoleIndex);
		yield return new WaitForSeconds(postLevelDelay);
		uiManager.HideEndOfLevel();
		GoToNextHole();
	}

	public UIManager GetUI()
	{
		return uiManager;
	}

	public void MainMenuGoNext()
	{
		if (!gameCompleted)
		{
			uiManager.HideTitleScreen();
			GoToNextHole();
		}
	}
	
	public void InitializeGradient(Vector3 inMinPosition, float inMaxDistance)
	{
		gradientMinPosition = inMinPosition;
		gradientMaxDistance = inMaxDistance;
		SetShaderGlobals();
	}

}
