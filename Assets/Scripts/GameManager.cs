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

        InputSystem.ins.KillPlayerKeyPressEvent += KillPlayer;
        InputSystem.ins.RevokeKeyPressEvent += RevokeFirstCorpse;

        LoadLevel(LoadLevelIndex);
        currentLevelIndex = LoadLevelIndex;
    }


    #region revoke
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
#endregion

    #region kill
    public void KillPlayer () => StartCoroutine(KillPlayerCoroutine());

    private IEnumerator KillPlayerCoroutine()
    {
        if (corpses.Count + 1 > MaxDeaths)
        {
            ReloadLevel();
        }
        else
        {
            Vector3 pos = player.transform.position;
            player.PlayDeathAnimation();
            yield return transitionController.TransiteIn();
            Destroy(player.gameObject);
            
            corpses.Add(Instantiate(CorpsePrefab, pos, Quaternion.identity));
            CorpsesUpdateEvent.Invoke(corpses);
            player = Instantiate(PlayerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
            transitionController.SetPlayer(player.transform.GetChild(0).gameObject);
            yield return transitionController.TransiteOut();
        }
    }
#endregion

    #region load

    public void LoadLevel (int index)
    {
        respawnPoint = Vector2.zero;
        StartCoroutine(LoadLevelCoroutine(index));
    }

    private IEnumerator LoadLevelCoroutine (int index)
    {
        if (currentLevel != null)
        {
            yield return transitionController.TransiteIn();

            for (int i = 0; i < corpses.Count; i++)
                Destroy(corpses[i]);
            corpses.Clear();
            CorpsesUpdateEvent.Invoke(corpses);

            Destroy(player.gameObject);
            Destroy(currentLevel);
        }

        currentLevel = Instantiate(Levels[index]);
        currentLevelIndex = index;
        player = Instantiate(PlayerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        transitionController.SetPlayer(player.transform.GetChild(0).gameObject);
        yield return transitionController.TransiteOut();
        CorpsesUpdateEvent.Invoke(corpses);
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelCoroutine(currentLevelIndex));
    }
    #endregion

    #region API

    public Player GetPlayer ()
    {
        return player;
    }

    #endregion
}
