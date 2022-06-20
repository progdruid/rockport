using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public GameObject AdditionalCameraPrefab;
    public GameObject PlayerPrefab;

    public int LoadLevelIndex;
    public GameObject[] Levels;
    
    private GameObject currentLevel;
    private GameObject player;
    private GameObject additionalCamera;

    void Start()
    {
        additionalCamera = Instantiate(AdditionalCameraPrefab);
        LoadLevel(LoadLevelIndex);
    }

    public void LoadLevel (int index)
    {
        if (currentLevel != null)
        {
            Destroy(currentLevel);
            Destroy(player);
            additionalCamera = Instantiate(AdditionalCameraPrefab);
        }
        
        currentLevel = Instantiate(Levels[index]);

        StartCoroutine(Delay(3000));

        Destroy(additionalCamera);
        player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity);
    }

    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }
}
