using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public struct LevelTree
{
    [System.Serializable]
    public struct LevelData
    {
        public int id;
        public string name;
        public int[] necessaryLevelsIDs;
        public string path;
        public bool completed;
    }

    public LevelData[] levels;

    public static LevelTree Extract (string path)
    {
        StreamReader reader = new StreamReader(path);
        string json = reader.ReadToEnd();
        reader.Close();

        LevelTree tree = JsonUtility.FromJson<LevelTree>(json);
        return tree;
    }
}
