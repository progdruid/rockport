using SimpleJSON;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public enum SignalGateType
{
    And,
    Or,
    Not
}


public class SignalGate : MapEntity
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Gate")]
    [SerializeField] private SignalGateType type = SignalGateType.And;
    [SerializeField] private Renderer textRenderer;

    private SignalEmitter _emitter;
    private SignalListener[] _listeners;
    private bool[] _listenedValues;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    protected override void Awake()
    {
        Assert.IsNotNull(textRenderer);
        
        
        _emitter = new SignalEmitter();
        AddPublicModule("gate-output", _emitter);
        
        switch (type)
        {
            case SignalGateType.And:
                _listenedValues = new bool[2];
                _listeners = new[] { new SignalListener(), new SignalListener() };
                AddPublicModule("and-gate-input-0", _listeners[0]);
                AddPublicModule("and-gate-input-1", _listeners[1]);
                break;
            case SignalGateType.Or:
                _listenedValues = new bool[2];
                _listeners = new[] { new SignalListener(), new SignalListener() };
                AddPublicModule("or-gate-input-0", _listeners[0]);
                AddPublicModule("or-gate-input-1", _listeners[1]);
                break;
            case SignalGateType.Not:
                _listenedValues = new bool[2];
                _listeners = new[] { new SignalListener() };
                AddPublicModule("not-gate-input", _listeners[0]);
                break;
            default:
                Assert.IsTrue(false, "Invalid signal gate type");
                break;
        }
        
        base.Awake();
    }

    public override void Activate()
    {
        textRenderer.enabled = false;
        
        for (var i = 0; i < _listeners.Length; i++)
        {
            var savedI = i;
            _listeners[i].ActionOnSignal = (val) =>
            {
                _listenedValues[savedI] = val; 
                TryEmit(); 
            };
        }
    }

    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public override JSONNode ExtractData()
    {
        var json = new JSONObject();
        Space.SnapWorldToMap(Target.position, out var map);
        json["map"] = map.ToJson();
        return json;
    }

    public override void Replicate(JSONNode data)
    {
        EnsureInitialise();
        var map = data["map"].ReadVector2Int();
        var worldPos = Space.ConvertMapToWorld(map);
        Target.SetWorldXY(worldPos);
    }

    public override void ChangeAt(Vector2 worldPos, bool shouldPlaceNotRemove)
    {
        if (!Space.SnapWorldToMap(worldPos, out var mapPos)) return;
        var snappedWorldPos = Space.ConvertMapToWorld(mapPos);
        Target.SetWorldXY(snappedWorldPos);
    }
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void TryEmit()
    {
        var signalWas = _emitter.Signal;
        var valueNow = signalWas;
        switch (type)
        {
            case SignalGateType.And:
                valueNow = true;
                foreach (var value in _listenedValues)
                    valueNow = valueNow && value;
                break;
            case SignalGateType.Or:
                valueNow = false;
                foreach (var value in _listenedValues)
                    valueNow = valueNow || value;
                break;
            case SignalGateType.Not:
                valueNow = !_listenedValues[0];
                break;
            default:
                Assert.IsTrue(false, "Invalid signal gate type");
                break;
        }
        if (signalWas == valueNow)
            return;
            
        _emitter.Signal = valueNow;
    }
}

}