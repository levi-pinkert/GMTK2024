using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            GameManager.instance.MainMenuGoNext();
        }
    }
}
