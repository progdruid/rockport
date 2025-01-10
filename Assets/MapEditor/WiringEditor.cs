using System.Collections.Generic;
using Map;
using UnityEngine;
using UnityEngine.Assertions;

namespace MapEditor
{

public class WiringEditor : MonoBehaviour, IMapEditorMode
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private Transform target;
    [SerializeField] private GameObject inputPinPrefab;
    [SerializeField] private GameObject outputPinPrefab;
    [SerializeField] private GameObject linkLinePrefab;

    private MapSpace _map;
    private SignalCircuit _signalCircuit;
    
    private readonly Dictionary<SignalEmitter, Transform> _outputs = new();
    private readonly Dictionary<SignalListener, Transform> _inputs = new();
    
    private readonly HashSet<LineRenderer> _linkLines = new();
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(inputPinPrefab);
        Assert.IsNotNull(outputPinPrefab);
        Assert.IsNotNull(linkLinePrefab);
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void Inject (MapSpace map) => _map = map;
    public void Inject(SignalCircuit circuit) => _signalCircuit = circuit;

    public void Enter()
    {
        foreach (var emitter in _signalCircuit.Emitters)
        {
            var rect = Instantiate(outputPinPrefab, target).transform;
            _outputs.Add(emitter, rect);
        }
        
        foreach (var listener in _signalCircuit.Listeners)
        {
            var rect = Instantiate(inputPinPrefab, target).transform;
            _inputs.Add(listener, rect);
        }
        
        UpdatePinPositions();
        
        foreach (var (listener, emitter) in _signalCircuit.Links)
        {
            var foundInput = _inputs.TryGetValue(listener, out var inputPin);
            var foundOutput = _outputs.TryGetValue(emitter, out var outputPin);
            Assert.IsTrue(foundInput);
            Assert.IsTrue(foundOutput);
            
            var line = Instantiate(linkLinePrefab, target).GetComponent<LineRenderer>();
            Assert.IsNotNull(line);
            line.SetPosition(0, inputPin.transform.position + Vector3.forward * 0.1f);
            line.SetPosition(1, outputPin.transform.position + Vector3.forward * 0.1f);
            
            _linkLines.Add(line);
        }
    }

    public void Exit()
    {
        foreach (var line in _linkLines) Destroy(line.gameObject);
        _linkLines.Clear();
        
        foreach (var (_, rect) in _outputs) Destroy(rect.gameObject);
        foreach (var (_, rect) in _inputs) Destroy(rect.gameObject);
        _outputs.Clear();
        _inputs.Clear();
    }

    public void HandleInput(Vector2 worldMousePos)
    {
        //UpdatePinPositions();
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void UpdatePinPositions()
    {
        var z = _map.GetMapTop() - 1f;
        
        foreach (var (module, pin) in _outputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var anchor = _map.ConvertMapToWorld(entity.GetAnchorPoint());
            pin.SetWorld(anchor, z);
        }
        
        foreach (var (module, pin) in _inputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var anchor = _map.ConvertMapToWorld(entity.GetAnchorPoint());
            pin.SetWorld(anchor, z);
        }
        
    }
}

}