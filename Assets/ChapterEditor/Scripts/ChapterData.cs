
using System;
using UnityEngine;

namespace ChapterEditor
{

[Serializable]
public struct ChapterData : IPackable
{
    public Vector2Int SpaceSize;
    public string[] LayerNames;
    public string[] LayerData;
    
    public string Pack()
    {
        return JsonUtility.ToJson(this);
    }

    public void Unpack(string data)
    {
        this = JsonUtility.FromJson<ChapterData>(data);
    }
}

}