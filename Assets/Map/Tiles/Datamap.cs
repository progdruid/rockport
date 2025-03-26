using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleJSON;
using UnityEngine;

namespace Map
{

[Serializable]
public struct Datamap<T> : IReplicable
{
    private T[] _buffer;
    private Vector2Int _size;

    public Datamap(Vector2Int size, T defaultValue)
    {
        _size = size;
        _buffer = new T[size.x * size.y];
        for (int i = 0; i < _buffer.Length; ++i)
            _buffer[i] = defaultValue;
    }

    public ref T At(Vector2Int pos) => ref _buffer[pos.y * _size.x + pos.x];
    
    public Vector2Int Size => _size;
    public int Width => _size.x;
    public int Height => _size.y;
    public int Count => _buffer.Length;

    public JSONNode ExtractData()
    {
        using var ms = new MemoryStream();
        var bf = new BinaryFormatter();
        bf.Serialize(ms, _buffer);
        var base64Data = Convert.ToBase64String(ms.ToArray());

        var jsonObject = new JSONString(base64Data);

        return jsonObject;
    }

    public void Replicate(JSONNode data)
    {
        var base64Data = data;
        
        var bytes = Convert.FromBase64String(base64Data);
        using var ms = new MemoryStream(bytes);
        var bf = new BinaryFormatter();
        _buffer = (T[])bf.Deserialize(ms);
    }

    public void Print()
    {
        var s = "";
        for (var y = 0; y < _size.y; ++y)
        {
            for (var x = 0; x < _size.x; ++x)
                s += _buffer[y * _size.x + x].ToString();
            s += Environment.NewLine;
        }

        Debug.Log(s);
    }
}

}