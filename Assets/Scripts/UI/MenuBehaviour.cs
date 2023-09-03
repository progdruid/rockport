using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    [SerializeField] LevelListMover levelListMover;
    [SerializeField] LevelTreeManager levelTreeManager;
    [SerializeField] SequentialSoundPlayer soundPlayer;
    
    private void Start()
    {
        Debug.Log(PlayerPrefs.GetInt("Last_Completed_Level_ID"));
        soundPlayer.StartPlaying();
    }

    public void HandlePlayButton ()
    {
        StartCoroutine(HandlePlayButtonRoutine());
    }

    private IEnumerator HandlePlayButtonRoutine ()
    {
        yield return soundPlayer.StopPlaying();
        PlayerPrefs.SetInt("Level_ID_Selected_in_Menu", levelListMover.GetSelectedLevel());
        SceneManager.LoadScene("Main");
    }

    public void HandleQuitButton ()
    {
        Application.Quit();
    }
}
