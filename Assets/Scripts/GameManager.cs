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
        StartCoroutine(RevokeFirstCorpseCoroutine());
    }

    private IEnumerator RevokeFirstCorpseCoroutine ()
    {
        var animator = corpses[0].GetComponent<Animator>();
        animator.SetTrigger("Revoke");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "End");
        var corpse = corpses[0];
        corpses.RemoveAt(0);
        Destroy(corpse);
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
            player.active = false;
            deathsbar.SetActive(false);
            StartCoroutine(transitionController.TransiteIn());
            yield return new WaitUntil(() => transitionController.transitionMade);
            Destroy(player.gameObject);
            
            corpses.Add(Instantiate(CorpsePrefab, pos, Quaternion.identity));
            CorpsesUpdateEvent.Invoke(corpses);
            player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
            transitionController.AddAttachedCamera(player.transform.GetChild(0).gameObject);
            StartCoroutine(transitionController.TransiteOut());
            yield return new WaitUntil(() => transitionController.transitionMade);
            player.active = true;
            deathsbar.SetActive(true);
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
            player.active = false;
            deathsbar.SetActive(false);
            StartCoroutine(transitionController.TransiteIn());
            yield return new WaitUntil(() => transitionController.transitionMade);

            for (int i = 0; i < corpses.Count; i++)
                Destroy(corpses[i]);
            corpses.Clear();
            CorpsesUpdateEvent.Invoke(corpses);

            Destroy(player.gameObject);
            Destroy(currentLevel);
        }

        currentLevel = Instantiate(Levels[index]);
        currentLevelIndex = index;
        player = Instantiate(PlayerPrefab, Vector2.zero, Quaternion.identity).GetComponent<Player>();
        transitionController.AddAttachedCamera(player.transform.GetChild(0).gameObject);
        StartCoroutine(transitionController.TransiteOut());
        yield return new WaitUntil(() => transitionController.transitionMade);
        player.active = true;
        deathsbar.SetActive(true);
        CorpsesUpdateEvent.Invoke(corpses);
    }

    public void ReloadLevel()
    {
        LoadLevel(currentLevelIndex);
    }
}
