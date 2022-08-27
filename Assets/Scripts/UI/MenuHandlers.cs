using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuHandlers : MonoBehaviour
{
    public void HandlePlayButton ()
    {
        SceneManager.LoadScene("Main");
    }

    public void HandleQuitButton ()
    {
        Application.Quit();
    }
}
