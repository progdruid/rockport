using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject AdditionalCameraPrefab;
    public GameObject PlayerPrefab;
    public GameObject CorpsePrefab;

    public int LoadLevelIndex;
    public GameObject[] Levels;

    [HideInInspector]
    public List<GameObject> corpses;

    private int currentLevelIndex;
    private GameObject currentLevel;
    private Player player;
    private GameObject additionalCamera;

    public int DefaultMaxDeaths;
    private int deaths;

    void Start()
    {
        additionalCamera = Instantiate(AdditionalCameraPrefab);
        LoadLevel(LoadLevelIndex);
        currentLevelIndex = LoadLevelIndex;
    }

    public IEnumerator KillPlayer()
    {
        deaths++;
        if (deaths > DefaultMaxDeaths)
        {
            ReloadLevel();
            deaths = 0;
        }
        else
        {
            Vector2 pos = player.transform.position;
            player.PlayDeathAnimation();
            Destroy(player.gameObject);
            additionalCamera.SetActive(true);
            corpses.Add(Instantiate(CorpsePrefab, pos, Quaternion.identity));
            yield return new WaitForSeconds(1f);
            player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
            additionalCamera.SetActive(false);
        }
    }

    public void LoadLevel (int index)
    {
        StartCoroutine(LoadLevelCoroutine(index));
    }

    private IEnumerator LoadLevelCoroutine (int index)
    {
        if (currentLevel != null)
        {
            for (int i = 0; i < corpses.Count; i++)
                Destroy(corpses[i]);
            corpses.Clear();

            Destroy(currentLevel);
            Destroy(player.gameObject);
            additionalCamera.SetActive(true);
        }

        currentLevel = Instantiate(Levels[index]);
        currentLevelIndex = index;
        Debug.Log("Before: " + Time.timeScale);
        yield return new WaitForSeconds(1f);
        Debug.Log("After");
        additionalCamera.SetActive(false);
        player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
        //StopCoroutine(LoadLevelCoroutine(index));
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelIndex);
    }
}
