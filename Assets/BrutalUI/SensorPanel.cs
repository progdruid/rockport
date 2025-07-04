using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class SensorPanel : MonoBehaviour
{
    [SerializeField] private UnityEvent senseStartEvent;
    [SerializeField] private UnityEvent senseEndEvent;

    public bool Holding
    {
        get => _holding;
        private set
        {
            PreviousHolding = _holding;
            _holding = value;

            if (_holding && !PreviousHolding)
                senseStartEvent?.Invoke();
            else if (!_holding && PreviousHolding)
                senseEndEvent?.Invoke();
        }
    }

    public bool PreviousHolding { get; private set; } = false;
    public bool Pressed => Holding && !PreviousHolding;
    public bool Released => !Holding && PreviousHolding;
    
    
    private bool _holding = false;
    private RectTransform _rect;

    private void Awake() => _rect = GetComponent<RectTransform>();

    private void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && Check(Input.mousePosition))
        {
            Holding = true;
            return;
        }
        if (Input.touchCount > 0)
            for (var i = 0; i < Input.touchCount; i++)
            {
                if (!Check(Input.GetTouch(i).position)) continue;
                Holding = true;
                return;
            }
        
        Holding = false;
    }
    
    private bool Check (Vector2 position) => RectTransformUtility.RectangleContainsScreenPoint(_rect, position);
}

}