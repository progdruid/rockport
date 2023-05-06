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

    public LevelTree.LevelData? TryGetLevel (int id)
    {
        int index = levelTree.GetLevelIndex(id);
        if (index != -1)
            return levelTree.levels[index];
        else
            return null;
    }

    public int GetLevelCount() => levelTree.levels.Length;
}
