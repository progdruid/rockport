using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Map
{

[Serializable]
public struct Datamap<T> : IPackable
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

    public string Pack()
    {
        using var ms = new MemoryStream();
        var bf = new BinaryFormatter();
        bf.Serialize(ms, _buffer);
        return Convert.ToBase64String(ms.ToArray());
    }

    public void Unpack(string data)
    {
        var bytes = Convert.FromBase64String(data);
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