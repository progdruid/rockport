using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Datamap
{
    public static Datamap Create<T>(Vector2Int size, T defaultValue)
    {
        Datamap datamap = new() { Map = new object[size.x, size.y] };
        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
            datamap.Map[x, y] = defaultValue;
        return datamap;
    }
    public static Datamap Create<T>(Vector2Int size) => Create(size, Activator.CreateInstance<T>());


    public event Action<Vector2Int, object, object> ModificationEvent;
    private object[,] Map;
    
    
    public void NotifyModified(Vector2Int pos, object prev, object current) => ModificationEvent?.Invoke(pos, prev, current);
    
    public T GetAt<T>(Vector2Int pos) => (T)Map[pos.x, pos.y];
    public void SetAt<T>(Vector2Int pos, T value)
    {
        object previous = Map[pos.x, pos.y];
        Map[pos.x, pos.y] = value;
        NotifyModified(pos, previous, value);
    }
    public void ModifyAt<T> (Vector2Int pos, Func<T, T> modifier) => SetAt(pos, modifier.Invoke(GetAt<T>(pos)));
}
