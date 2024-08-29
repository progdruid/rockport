using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class MapWrapper<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    private struct Item
    {
        public TKey Key;
        public TValue Value;
    }

    private Item[] serializedItems;
    
    public void OnBeforeSerialize()
    {
        serializedItems = new Item[Count];
        int index = 0;
        foreach (var pair in this)
        {
            serializedItems[index] = new Item() { Key = pair.Key, Value = pair.Value };
            index++;
        }
    }

    public void OnAfterDeserialize()
    {
        if (serializedItems == null)
            return;
        
        foreach (var item in serializedItems)
            TryAdd(item.Key, item.Value);
    }
}
