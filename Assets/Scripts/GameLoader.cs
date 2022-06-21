using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public GameObject AdditionalCameraPrefab;
    public GameObject PlayerPrefab;

    public int LoadLevelIndex;
    public GameObject[] Levels;

    private int currentLevelIndex;
    private GameObject currentLevel;
    private GameObject player;
    private GameObject additionalCamera;

    void Start()
    {
        additionalCamera = Instantiate(AdditionalCameraPrefab);
        LoadLevel(LoadLevelIndex);
        currentLevelIndex = LoadLevelIndex;
    }

    public void LoadLevel (int index)
    {
        StartCoroutine(LoadLevelCoroutine(index));
    }

    public void LoadNextLevel ()
    {
        LoadLevel(currentLevelIndex + 1);
    }

    IEnumerator LoadLevelCoroutine (int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
            Destroy(player);
            additionalCamera = Instantiate(AdditionalCameraPrefab);
        }

        currentLevel = Instantiate(Levels[index]);
        currentLevelIndex = index;

        yield return new WaitForSeconds(1f); ;

        Destroy(additionalCamera);
        player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity);
    }
}
