using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnButtonPress()
    {
        SceneManager.LoadScene("MainScene");
        SceneManager.UnloadScene("MainMenu");
    }
}
