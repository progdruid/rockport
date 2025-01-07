using UnityEngine;

namespace ChapterEditor
{

public class SignalListener : MonoBehaviour
{
    public System.Action<bool> ActionOnSignal { get; set; }
    public void ReceiveSignal(bool value) => ActionOnSignal?.Invoke(value);
}

}