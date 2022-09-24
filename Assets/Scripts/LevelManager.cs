using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject corpsePrefab;

    public int levelIndex;
    public GameObject[] levels;

    private List<GameObject> corpses;
    public event System.Action<List<GameObject>> CorpsesUpdateEvent;

    private int currentLevelIndex;
    private GameObject currentLevel;
    [HideInInspector]
    public Vector2 respawnPoint;

    private Player player;
    private TransitionController transitionController;
    private DeathsBarManager deathsbar;

    public int defaultMaxDeaths;
    public int MaxDeaths 
    { 
        get { return maxDeaths; } 
        set 
        { 
            maxDeaths = value; 
            if (CorpsesUpdateEvent != null) 
                CorpsesUpdateEvent(corpses); 
        }
    }
    private int maxDeaths;

    void Start()
    {
        transitionController = GetComponent<TransitionController>();
        deathsbar = GetComponent<DeathsBarManager>();
        corpses = new List<GameObject>();

        InputSystem.ins.KillPlayerKeyPressEvent += KillPlayer;
        InputSystem.ins.RevokeKeyPressEvent += RevokeFirstCorpse;

        LoadLevel(levelIndex);
        currentLevelIndex = levelIndex;
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
    public void KillPlayer()
    {
        if (corpses.Count + 1 > MaxDeaths)
        {
            ReloadLevel();
        }
        else
        {
            StartCoroutine(KillPlayerRoutine());
        }
    }

    private IEnumerator KillPlayerRoutine()
    {
        player.PlayDeathAnimation();

        yield return transitionController.TransiteIn();

        Vector3 pos = player.transform.position;
        Destroy(player.gameObject);

        corpses.Add(Instantiate(corpsePrefab, pos, Quaternion.identity));
        CorpsesUpdateEvent.Invoke(corpses);

        player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();

        yield return transitionController.TransiteOut();
    }
#endregion

    #region load

    private IEnumerator DeloadLevelRoutine ()
    {
        yield return transitionController.TransiteIn();

        for (int i = 0; i < corpses.Count; i++)
            Destroy(corpses[i]);
        corpses.Clear();
        CorpsesUpdateEvent.Invoke(corpses);

        Destroy(player.gameObject);
        Destroy(currentLevel);
    }

    public void LoadLevel (int index)
    {
        StartCoroutine(LoadLevelRoutine(index));
    }

    private IEnumerator LoadLevelRoutine (int index)
    {
        if (currentLevel != null)
        {
            yield return DeloadLevelRoutine();
        }

        respawnPoint = Vector2.zero;
        MaxDeaths = 0;

        currentLevel = Instantiate(levels[index]);
        currentLevelIndex = index;

        player = Instantiate(playerPrefab, new Vector3(respawnPoint.x, respawnPoint.y, -1f), Quaternion.identity).GetComponent<Player>();
        
        yield return transitionController.TransiteOut();
        CorpsesUpdateEvent.Invoke(corpses);
    }

    public void ReloadLevel()
    {
        StartCoroutine(LoadLevelRoutine(currentLevelIndex));
    }
    #endregion

    #region API

    public Player GetPlayer ()
    {
        return player;
    }

    #endregion
}
