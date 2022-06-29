using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject PlayerPrefab;
    public GameObject CorpsePrefab;

    public int LoadLevelIndex;
    public GameObject[] Levels;

    private List<GameObject> corpses;
    public event System.Action<List<GameObject>> CorpsesUpdateEvent;

    private int currentLevelIndex;
    private GameObject currentLevel;
    [HideInInspector]
    public Vector2 respawnPoint;

    private Player player;
    private TransitionController transitionController;
    private DeathsBarManager deathsbar;

    public int DefaultMaxDeaths;
    public int MaxDeaths { get; private set; }

    void Start()
    {
        MaxDeaths = DefaultMaxDeaths;
        
        transitionController = GetComponent<TransitionController>();
        deathsbar = GetComponent<DeathsBarManager>();
        corpses = new List<GameObject>();

        LoadLevel(LoadLevelIndex);
        currentLevelIndex = LoadLevelIndex;
    }

    public void RevokeFirstCorpse ()
    {
        if (corpses.Count == 0)
            return;
        StartCoroutine(RevokeCorpseCoroutine(0));
    }

    public void RevokeCorpseAt (Vector2 point)
    {
        if (corpses.Count == 0)
            return;
        int i;
        bool found = false;
        for (i = 0; i < corpses.Count; i++)
        {
            var col = corpses[i].GetComponent<Collider2D>();
            found = col.OverlapPoint(point);
            if (found)
                break;
        }
        if (found)
            StartCoroutine(RevokeCorpseCoroutine(i));
    }

    private IEnumerator RevokeCorpseCoroutine (int index)
    {
        var animator = corpses[index].GetComponent<Animator>();
        animator.SetTrigger("Revoke");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "End");

        if (corpses.Count != 0)
        {
            var corpse = corpses[index];
            corpses.RemoveAt(index);
            Destroy(corpse);
        }
        
        CorpsesUpdateEvent.Invoke(corpses);
    }

    public void KillPlayer () => StartCoroutine(KillPlayerCoroutine());

    private IEnumerator KillPlayerCoroutine()
    {
        if (corpses.Count + 1 > MaxDeaths)
        {
            ReloadLevel();
        }
        else
        {
            Vector2 pos = player.transform.position;
            player.PlayDeathAnimation();
            yield return TransiteIn();
            Destroy(player.gameObject);
            
            corpses.Add(Instantiate(CorpsePrefab, pos, Quaternion.identity));
            CorpsesUpdateEvent.Invoke(corpses);
            player = Instantiate(PlayerPrefab, respawnPoint, Quaternion.identity).GetComponent<Player>();
            transitionController.AddAttachedCamera(player.transform.GetChild(0).gameObject);
            yield return TransiteOut();
        }
    }

    public IEnumerator TransiteIn ()
    {
        player.active = false;
        deathsbar.SetActive(false);
        yield return transitionController.TransiteIn();
    }

    public IEnumerator TransiteOut ()
    {
        yield return transitionController.TransiteOut();
        player.active = true;
        deathsbar.SetActive(true);
    }

    public void LoadLevel (int index)
    {
        StartCoroutine(LoadLevelCoroutine(index));
    }

    private IEnumerator LoadLevelCoroutine (int index)
    {
        if (currentLevel != null)
        {
            yield return TransiteIn();

            for (int i = 0; i < corpses.Count; i++)
                Destroy(corpses[i]);
            corpses.Clear();
            CorpsesUpdateEvent.Invoke(corpses);

            Destroy(player.gameObject);
            Destroy(currentLevel);
        }

        currentLevel = Instantiate(Levels[index]);
        currentLevelIndex = index;
        respawnPoint = Vector2.zero;
        player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
        transitionController.AddAttachedCamera(player.transform.GetChild(0).gameObject);
        yield return TransiteOut();
        CorpsesUpdateEvent.Invoke(corpses);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelIndex);
    }
}
