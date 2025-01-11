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
    
    
    private SignalEmitter _draggedEmitter;
    private SignalListener _draggedListener;
    
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
            CreateLink(listener, emitter);
    }

    public void Exit()
    {
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
            if (!Lytil.IsInRendererBounds(worldMousePos, pin)) continue;
            pointedListener = listener;
            break;
        }
        foreach (var (emitter, pin) in _outputs)
        {
            if (!Lytil.IsInRendererBounds(worldMousePos, pin)) continue;
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