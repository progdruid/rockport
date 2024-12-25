using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ChapterEditor
{
[Serializable]
public struct MapData : IPackable
{
    public Vector2Int SpaceSize;
    public Vector2Int SpawnPoint;
    [FormerlySerializedAs("SpawnLayer")] public float SpawnZ;
    public string[] LayerNames;
    public string[] LayerData;

    public string Pack() => JsonUtility.ToJson(this);

    public void Unpack(string data) => this = JsonUtility.FromJson<MapData>(data);
}

}