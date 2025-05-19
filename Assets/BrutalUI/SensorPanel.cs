using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace BrutalUI
{

[RequireComponent(typeof(RectTransform))]
public class SensorPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private UnityEvent senseStartEvent;
    [SerializeField] private UnityEvent senseEndEvent;
    
    public void OnPointerDown(PointerEventData eventData) => senseStartEvent?.Invoke();
    public void OnPointerUp(PointerEventData eventData) => senseEndEvent?.Invoke();
}

}