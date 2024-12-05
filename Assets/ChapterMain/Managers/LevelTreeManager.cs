using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTreeManager : MonoBehaviour
{
    [SerializeField] string leveltreePath;

    private LevelTree levelTree;

    void Awake()
    {
        levelTree = LevelTree.Extract(leveltreePath);
    }

    public LevelTree.LevelData TryGetLevel (int id)
    {
        int index = levelTree.GetLevelIndex(id);
        if (index != -1)
            return levelTree.levels[index];
        else
            return null;
    }

    public int GetFullLevelCount() => levelTree.levels.Length;

    public int GetPublicLevelCount()
    {
        int count = 0;
        for (int i = 0; i < levelTree.levels.Length; i++)
            if (levelTree.levels[i].published)
                count++;
        return count;
    }
}
