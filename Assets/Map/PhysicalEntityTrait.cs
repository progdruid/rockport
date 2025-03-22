using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Map
{

public class PhysicalEntityTrait : IPropertyHolder, IReplicable
{
    //static part///////////////////////////////////////////////////////////////////////////////////////////////////////
    private static void TogglePhysicsInObject(GameObject obj, bool value)
    {
        foreach (var col in obj.GetComponentsInChildren<Collider2D>(true)) 
            col.enabled = value;
        
        foreach (var body in obj.GetComponentsInChildren<Rigidbody2D>(true))
        {
            if (value) body.WakeUp();
            else body.Sleep();
        }
    }
    
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private bool _generatePhysics;
    
    private readonly HashSet<TilemapCollider2D> _physicalTilemaps = new HashSet<TilemapCollider2D>();
    private readonly HashSet<GameObject> _physicalObjects = new HashSet<GameObject>();
    
    public event Action PropertiesChangeEvent;

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Generate Physics",
            PropertyType = PropertyType.Text,
            Getter = () => _generatePhysics ? "true" : "false",
            Setter = (object input) => _generatePhysics = (string)input == "true"
        };
    }

    public JSONObject ExtractData()
    {
        var json = new JSONObject {
            ["generatePhysics"] = _generatePhysics
        };
        return json;
    }

    public void Replicate(JSONObject data)
    {
        _generatePhysics = data["generatePhysics"].AsBool;
        PropertiesChangeEvent?.Invoke();
    }

    public void AddObject(GameObject physicalObject)
    {
        if (!physicalObject || _physicalObjects.Contains(physicalObject)) return;
        TogglePhysicsInObject(physicalObject, false);
        _physicalObjects.Add(physicalObject);
    }

    public void RemoveObject(GameObject physicalObject)
    {
        if (!physicalObject || !_physicalObjects.Contains(physicalObject)) return;
        _physicalObjects.Remove(physicalObject);
    }
    
    public void AddTilemap(Tilemap tilemap)
    {
        if (!tilemap) return;
        
        var collider = tilemap.gameObject.GetComponent<TilemapCollider2D>();
        if (_physicalTilemaps.Contains(collider)) return;
        if (!collider) collider = tilemap.gameObject.AddComponent<TilemapCollider2D>();   
        collider.enabled = false;
        tilemap.gameObject.layer = 8;
        
        _physicalTilemaps.Add(collider);
    }

    public void RemoveTilemap(Tilemap tilemap)
    {
        if (!tilemap) return;
        var collider = tilemap.gameObject.GetComponent<TilemapCollider2D>();
        if (!collider || !_physicalTilemaps.Contains(collider)) return;
        
        _physicalTilemaps.Remove(collider);
    }
    
    public void RequestGeneratePhysics()
    {
        if (!_generatePhysics) return;

        foreach (var tilemap in _physicalTilemaps) tilemap.enabled = true;
        foreach (var physicalObject in _physicalObjects) TogglePhysicsInObject(physicalObject, true);
    }
}

}