using System;
using UnityEngine;
using UnityEngine.Serialization;
using SimpleJSON;

namespace Map
{
[Serializable]
public struct MapData : IReplicable
{
    public Vector2Int SpaceSize;
    public string[] LayerNames;
    public JSONObject[] LayerData;
    public JSONObject SignalData;

    public JSONObject ExtractData()
    {
        var json = new JSONObject();
        
        json["SpaceSize"] = SpaceSize.ToJson();
        
        json["LayerNames"] = new JSONArray();
        var layerNames = new JSONArray();
        foreach (var layerName in LayerNames) 
            json["LayerNames"].Add(layerName);
        
        json["LayerData"] = new JSONArray();
        foreach (var layerData in LayerData) 
            json["LayerData"].Add(layerData);
        
        json["SignalData"] = SignalData;
        
        return json;
    }

    public void Replicate(JSONObject data)
    {
        SpaceSize = data["SpaceSize"].ReadVector2Int();
        
        var layerNamesArray = data["LayerNames"].AsArray;
        LayerNames = new string[layerNamesArray.Count];
        for (var i = 0; i < layerNamesArray.Count; i++) 
            LayerNames[i] = layerNamesArray[i];
        
        var layerDataArray = data["LayerData"].AsArray;
        LayerData = new JSONObject[layerDataArray.Count];
        for (var i = 0; i < layerDataArray.Count; i++) 
            LayerData[i] = layerDataArray[i].AsObject;
        
        SignalData = data["SignalData"].AsObject;
    }
}
}