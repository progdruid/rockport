using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    [SerializeField] LevelListMover levelListMover;
    [SerializeField] LevelTreeManager levelTreeManager;
    [SerializeField] SequentialSoundPlayer soundPlayer;
    [SerializeField] TransitionVeil transitionVeil;
    
    private void Start()
    {
        soundPlayer.StartPlaying();
        StartCoroutine(transitionVeil.TransiteOut());
    }

    public void HandlePlayButton ()
    {
        StartCoroutine(HandlePlayButtonRoutine());
    }

    private IEnumerator HandlePlayButtonRoutine ()
    {
        StartCoroutine(transitionVeil.TransiteIn());
        yield return soundPlayer.StopPlaying();
        yield return new WaitWhile(() => transitionVeil.inTransition);

        PlayerPrefs.SetInt("Level_ID_Selected_in_Menu", levelListMover.GetSelectedLevel());
        SceneManager.LoadScene("Main");
    }

    public void HandleQuitButton ()
    {
        Application.Quit();
    }
}
