using System.Collections.Generic;
using Map;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

public class MapFinish : EntityComponent
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private UniversalTrigger trigger;

    private string _mapName = "Test";
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Wake()
    {
        Assert.IsNotNull(trigger);
        trigger.EnterEvent += HandleTriggerEnter;
    }

    public override void Initialise() { }
    public override void Activate() { }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override string JsonName => "finish";
    public override IEnumerator<PropertyHandle> GetProperties()
    {
        yield return new PropertyHandle()
        {
            PropertyName = "Map Name",
            PropertyType = PropertyType.Text,
            Getter = () => _mapName,
            Setter = (value) => _mapName = (string)value
        };
    }
    
    public override void Replicate(JSONNode data)
    {
        _mapName = data["mapName"];
        InvokePropertiesChangeEvent();
    }

    public override JSONNode ExtractData()
    {
        var data = new JSONObject();
        data["mapName"] = _mapName;
        return data;
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void HandleTriggerEnter (Collider2D col, TriggeredType type)
    {
        if (type != TriggeredType.Player)
            return;

        GameSystems.Ins.MapManager.ProceedFurther(_mapName);
    }

}
