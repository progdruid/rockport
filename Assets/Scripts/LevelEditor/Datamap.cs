using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Datamap
{
    public object[,] Map;
    public event Action<Vector2Int> ModificationEvent;
    public virtual void NotifyModified(Vector2Int pos) => ModificationEvent?.Invoke(pos);

    public Datamap(Vector2Int size, object defaultValue)
    {
        Map = new object[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        for (int y = 0; y < size.y; y++)
            Map[x, y] = defaultValue;
    }

    public ref object At(Vector2Int pos) => ref Map[pos.x, pos.y];
    public T At<T>(Vector2Int pos) => (T)Map[pos.x, pos.y];
}
