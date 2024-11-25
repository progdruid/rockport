using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

[Serializable]
public class SerializableMap<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [Serializable]
    private struct Item
    {
        public TKey Key;
        public TValue Value;
    }

    [SerializeField]
    private Item[] serializedItems;
    
    public SerializableMap () {}

    protected SerializableMap(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        serializedItems = (Item[]) info.GetValue("SerializedItems", typeof(Item[]));
        OnAfterDeserialize();
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        OnBeforeSerialize();
        info.AddValue("SerializedItems", serializedItems, typeof(Item[]));
    }

    public void OnBeforeSerialize()
    {
        serializedItems = new Item[Count];
        var index = 0;
        foreach (var pair in this)
        {
            serializedItems[index] = new Item() { Key = pair.Key, Value = pair.Value };
            index++;
        }

        //var message = "OnBeforeSerialize called:";
        //foreach (var item in serializedItems)
        //    message += $"\n* {item.Key}: {item.Value}";
        //Debug.Log(message);
    }

    public void OnAfterDeserialize()
    {
        if (serializedItems == null)
            return;
        
        Clear();
        foreach (var item in serializedItems)
            TryAdd(item.Key, item.Value);
        
        //var message = "OnAfterDeserialize called:";
        //foreach (var (key, value) in this)
        //    message += $"\n* {key}: {value!=null}";
        //Debug.Log(message);
    }
}
