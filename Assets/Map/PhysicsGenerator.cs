using System.Collections.Generic;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

public class PhysicsGenerator : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private bool exposeToEditor;
    [SerializeField] private bool generatePhysics;
    
    private readonly List<Collider2D> _colliders = new();
    private readonly List<Rigidbody2D> _rigidbodies = new();
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake() { }
    public override void Initialise()
    {
        _colliders.AddRange(Target.GetComponentsInChildren<Collider2D>(true));  
        _rigidbodies.AddRange(Target.GetComponentsInChildren<Rigidbody2D>(true));
        foreach (var col in _colliders) col.enabled = false;
        foreach (var body in _rigidbodies) body.Sleep();
    }

    public override void Activate()
    {
        if (!generatePhysics) return;
        
        foreach (var col in _colliders) col.enabled = true;
        foreach (var body in _rigidbodies) body.WakeUp();
    }
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "physicsGenerator";
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        if (!exposeToEditor) yield break;
        yield return new PropertyHandle()
        {
            PropertyName = "Generate Physics",
            PropertyType = PropertyType.Text,
            Getter = () => generatePhysics ? "true" : "false",
            Setter = (object input) => generatePhysics = (string)input == "true"
        };
    }

    public override void Replicate(JSONNode data)
    {
        generatePhysics = data["generatePhysics"].AsBool;
        InvokePropertiesChangeEvent();
    }

    public override JSONNode ExtractData()
    {
        var json = new JSONObject  {
            ["generatePhysics"] = generatePhysics
        };
        return json;
    }
}
