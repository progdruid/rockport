using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuHandler : MonoBehaviour
{
    public void HandleBackToMenuButtonPress ()
    {
        SceneManager.LoadSceneAsync("Menu");
    }
}
