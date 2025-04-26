using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorStartup : MonoBehaviour
{
    static EditorStartup()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.OnEditorStartup();
        }

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

    public static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnEditorStartup();
            }
        }
	}
}
