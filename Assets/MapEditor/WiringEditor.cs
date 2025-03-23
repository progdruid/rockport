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
    [Space] 
    [SerializeField] private float pinPadding;
    
    private MapSpace _map;
    private SignalCircuit _signalCircuit;
    
    private readonly Dictionary<SignalEmitter, SpriteRenderer> _outputs = new();
    private readonly Dictionary<SignalListener, SpriteRenderer> _inputs = new();
    private readonly Dictionary<SignalListener, LineRenderer> _linkLines = new();
    private readonly Dictionary<MapEntity, Dictionary<SignalListener, int>> _entityToListeners = new();
    private readonly HashSet<MapEntity> _entityToEmitter = new();
    
    private SignalEmitter _draggedEmitter;
    private SignalListener _draggedListener;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        Assert.IsNotNull(target);
        Assert.IsNotNull(inputPinPrefab);
        Assert.IsNotNull(outputPinPrefab);
        Assert.IsNotNull(linkLinePrefab);
        Assert.AreNotApproximatelyEqual(pinPadding, 0f);
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public void Inject (MapSpace map) => _map = map;
    public void Inject(SignalCircuit circuit) => _signalCircuit = circuit;

    public void Enter()
    {
        Assert.IsTrue(_outputs.Count == 0);
        Assert.IsTrue(_inputs.Count == 0);
        Assert.IsTrue(_linkLines.Count == 0);
        Assert.IsTrue(_entityToListeners.Count == 0);
        Assert.IsTrue(_entityToEmitter.Count == 0);
        
        foreach (var emitter in _signalCircuit.Emitters)
        {
            var spriteRenderer = Instantiate(outputPinPrefab, target).GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer);
            _outputs.Add(emitter, spriteRenderer);

            var entity = ((IEntityModule)emitter).GetEntity();
            Assert.IsFalse(_entityToEmitter.Contains(entity));
            _entityToEmitter.Add(entity);
        }

        foreach (var listener in _signalCircuit.Listeners)
        {
            var spriteRenderer = Instantiate(inputPinPrefab, target).GetComponent<SpriteRenderer>();
            Assert.IsNotNull(spriteRenderer);
            _inputs.Add(listener, spriteRenderer);
            
            var entity = ((IEntityModule)listener).GetEntity();
            _entityToListeners.TryAdd(entity, new Dictionary<SignalListener, int>());
            
            var listeners = _entityToListeners[entity];
            Assert.IsFalse(listeners.ContainsKey(listener));
            listeners.Add(listener, listeners.Count);
        }

        UpdatePinPositions();

        foreach (var (listener, emitter) in _signalCircuit.Links) 
            CreateLink(listener, emitter);
    }

    public void Exit()
    {
        _entityToEmitter.Clear();
        _entityToListeners.Clear();
        
        foreach (var (_, line) in _linkLines) Destroy(line.gameObject);
        _linkLines.Clear();

        foreach (var (_, spriteRenderer) in _outputs) Destroy(spriteRenderer.gameObject);
        foreach (var (_, spriteRenderer) in _inputs) Destroy(spriteRenderer.gameObject);
        _outputs.Clear();
        _inputs.Clear();

        _draggedEmitter = null;
        _draggedListener = null;
    }

    public void HandleInput(Vector2 worldMousePos)
    {
        var lmbDown = Input.GetMouseButtonDown(0);
        var lmbUp = Input.GetMouseButtonUp(0);
        var rmbDown = Input.GetMouseButtonDown(1);
        
        if (!lmbDown && !lmbUp && !rmbDown)
            return;
        
        
        //try act on listeners
        SignalListener pointedListener = null;
        SignalEmitter pointedEmitter = null;
        foreach (var (listener, pin) in _inputs)
        {
            if (!RockUtil.IsInRendererBounds(worldMousePos, pin)) continue;
            pointedListener = listener;
            break;
        }
        foreach (var (emitter, pin) in _outputs)
        {
            if (!RockUtil.IsInRendererBounds(worldMousePos, pin)) continue;
            pointedEmitter = emitter;
            break;
        }

        if (rmbDown)
        {
            if (pointedListener != null && _signalCircuit.Links.ContainsKey(pointedListener))
            {
                DeleteLink(pointedListener);
                _signalCircuit.Unlink(pointedListener);
            }
            else if (pointedEmitter != null && _signalCircuit.TryGetLinks(pointedEmitter, out var listeners))
            {
                foreach (var listener in listeners)
                    DeleteLink(listener);
                _signalCircuit.Unlink(pointedEmitter);
            }
        }
        else if (lmbDown)
        {
            if (pointedListener != null)
                _draggedListener = pointedListener;
            else if (pointedEmitter != null)
                _draggedEmitter = pointedEmitter;
        }
        else
        {
            Assert.IsFalse(_draggedEmitter != null && _draggedListener != null);
            var emitterToLink = _draggedEmitter ?? pointedEmitter;
            var listenerToLink = _draggedListener ?? pointedListener;

            if (emitterToLink != null && listenerToLink != null)
            {
                if (_signalCircuit.Links.ContainsKey(listenerToLink))
                {
                    DeleteLink(listenerToLink);
                    _signalCircuit.Unlink(listenerToLink);
                }

                _signalCircuit.Link(emitterToLink, listenerToLink);
                CreateLink(listenerToLink, emitterToLink);
            }
            
            _draggedEmitter = null;
            _draggedListener = null;
        }
    }
    
    
    
    //private logic/////////////////////////////////////////////////////////////////////////////////////////////////////
    private void UpdatePinPositions()
    {
        var z = _map.GetMapTop() - 1f;
        
        foreach (var (module, pin) in _outputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var pos = _map.ConvertMapToWorld(entity.GetOverlayAnchor());
            
            if (_entityToListeners.ContainsKey(entity))
                pos.x += pinPadding;
            
            pin.transform.SetWorld(pos, z);
        }
        
        foreach (var (module, pin) in _inputs)
        {
            var entity = ((IEntityModule)module).GetEntity();
            var pos = _map.ConvertMapToWorld(entity.GetOverlayAnchor());
            
            var foundListeners = _entityToListeners.TryGetValue(entity, out var listeners);
            Assert.IsTrue(foundListeners && listeners.Any());
            var foundIndex = listeners.TryGetValue(module, out var index);
            Assert.IsTrue(foundIndex);

            pos.y += pinPadding * (2f * index - listeners.Count + 1f);
            
            if (_entityToListeners.ContainsKey(entity))
                pos.x -= pinPadding;

            pin.transform.SetWorld(pos, z);
        }
        
    }
    
    private void CreateLink(SignalListener listener, SignalEmitter emitter)
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
    
    private void DeleteLink(SignalListener listener)
    {
        var found = _linkLines.TryGetValue(listener, out var line);
        Assert.IsTrue(found);
        Destroy(line.gameObject);
        _linkLines.Remove(listener);
    }
}

}