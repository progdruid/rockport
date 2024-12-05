
using UnityEngine.EventSystems;

namespace ChapterEditor
{

public class ButtonWithEvents : UnityEngine.UI.Button
{
    public event System.Action<bool> InteractChangeEvent;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        InteractChangeEvent?.Invoke(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        InteractChangeEvent?.Invoke(false);
    }
}

}