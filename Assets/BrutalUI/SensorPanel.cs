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

    public bool Pressed
    {
        get => _pressed;
        private set
        {
            PreviousPressed = _pressed;
            _pressed = value;
            
            if (_pressed && !PreviousPressed)
                senseStartEvent?.Invoke();
            else if (!_pressed && PreviousPressed)
                senseEndEvent?.Invoke();
        }
    }

    public bool PreviousPressed { get; private set; } = false;
    
    private bool _pressed = false;
    private RectTransform _rect;

    private void Awake() => _rect = GetComponent<RectTransform>();

    private void Update()
    {
        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && Check(Input.mousePosition))
        {
            Pressed = true;
            return;
        }
        if (Input.touchCount > 0)
            for (var i = 0; i < Input.touchCount; i++)
            {
                if (!Check(Input.GetTouch(i).position)) continue;
                Pressed = true;
                return;
            }
        
        Pressed = false;
    }
    
    private bool Check (Vector2 position) => RectTransformUtility.RectangleContainsScreenPoint(_rect, position);
}

}