using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuBehaviour : MonoBehaviour
{
    [SerializeField] LevelTreeManager levelTreeManager;
    [Space]
    [SerializeField] TMP_Text levelTitle;
    [SerializeField] TMP_Text levelIdText;
    [Space]
    [SerializeField] SequentialSoundPlayer soundPlayer;
    
    private LevelTree.LevelData selectedLevelData;
    
    private int maxID = 16;
    private int minId = 0;

    private void Start()
    {
        soundPlayer.StartPlaying();
        UpdateTextFields(1);
    }

    private void UpdateTextFields (int newId)
    {
        if (selectedLevelData.id == newId || newId > maxID || newId < minId)
            return;
        
        selectedLevelData = levelTreeManager.TryGetLevel(newId);

        levelTitle.text = "Level: " + selectedLevelData.name;
        levelIdText.text = selectedLevelData.id.ToString();
    }

    public void HandlePlayButton ()
    {
        StartCoroutine(HandlePlayButtonRoutine());
    }

    private IEnumerator HandlePlayButtonRoutine ()
    {
        yield return soundPlayer.StopPlaying();
        PlayerPrefs.SetInt("Level_ID_Selected_in_Menu", selectedLevelData.id);
        SceneManager.LoadScene("Main");
    }

    public void HandleQuitButton ()
    {
        Application.Quit();
    }

    public void HandleLeftArrowButton ()
    {
        UpdateTextFields(selectedLevelData.id - 1);
    }

    public void HandleRightArrowButton()
    {
        UpdateTextFields(selectedLevelData.id + 1);
    }
}
