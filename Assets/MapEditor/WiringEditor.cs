using System.Collections.Generic;
using System.Linq;
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
    
    private readonly Dictionary<SignalEmitter, SpriteRenderer> _outputs = new();
    private readonly Dictionary<SignalListener, SpriteRenderer> _inputs = new();
    
    private readonly Dictionary<SignalListener, LineRenderer> _linkLines = new();
    
    
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
            var spriteRenderer = Instantiate(outputPinPrefab, target).GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer);
            _outputs.Add(emitter, spriteRenderer);
        }
        
        foreach (var listener in _signalCircuit.Listeners)
        {
            var spriteRenderer = Instantiate(inputPinPrefab, target).GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer);
            _inputs.Add(listener, spriteRenderer);
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
            
            _linkLines.Add(listener, line);
        }
    }

    public void Exit()
    {
        foreach (var (_, line) in _linkLines) Destroy(line.gameObject);
        _linkLines.Clear();
        
        foreach (var (_, rect) in _outputs) Destroy(rect.gameObject);
        foreach (var (_, rect) in _inputs) Destroy(rect.gameObject);
        _outputs.Clear();
        _inputs.Clear();
    }

    public void HandleInput(Vector2 worldMousePos)
    {
        var lmbDown = Input.GetMouseButtonDown(0);
        var rmbDown = Input.GetMouseButtonDown(1);
        
        if (!lmbDown && !rmbDown)
            return;
        
        //try act on listeners
        SignalListener selectedListener = null;
        foreach (var (listener, pin) in _inputs)
        {
            if (!_signalCircuit.Links.ContainsKey(listener) ||
                !Lytil.IsInRendererBounds(worldMousePos, pin)) continue;
            selectedListener = listener;
            break;
        }
        if (rmbDown && selectedListener != null)
        {
            DeleteLink(selectedListener);
            _signalCircuit.Unlink(selectedListener);
            return;
        }

        //try act on emitters
        SignalEmitter selectedEmitter = null;
        IReadOnlyCollection<SignalListener> selectedEmitterListeners = null;
        foreach (var (emitter, pin) in _outputs)
        {
            if (!_signalCircuit.TryGetLinks(emitter, out var listeners) ||
                !Lytil.IsInRendererBounds(worldMousePos, pin)) continue;
            selectedEmitter = emitter;
            selectedEmitterListeners = listeners;
            break;
        }
        if (rmbDown && selectedEmitter != null)
        {
            foreach (var listener in selectedEmitterListeners)
                DeleteLink(listener);
            _signalCircuit.Unlink(selectedEmitter);
            return;
        }
        
    }
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////

    private void DeleteLink(SignalListener listener)
    {
        var found = _linkLines.TryGetValue(listener, out var line);
        Assert.IsTrue(found);
        Destroy(line.gameObject);
        _linkLines.Remove(listener);
    }
    
    private void UpdatePinPositions()
    {
        var z = _map.GetMapTop() - 1f;
        
        foreach (var (module, pin) in _outputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var anchor = _map.ConvertMapToWorld(entity.GetAnchorPoint());
            pin.transform.SetWorld(anchor, z);
        }
        
        foreach (var (module, pin) in _inputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var anchor = _map.ConvertMapToWorld(entity.GetAnchorPoint());
            pin.transform.SetWorld(anchor, z);
        }
        
    }
}

}