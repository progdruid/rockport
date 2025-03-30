namespace Map
{

public class SignalListener : IEntityAccessor
{
    MapEntity IEntityAccessor.Entity { get; set; }
    string IEntityAccessor.AccessorName { get; set; }
    
    public System.Action<bool> ActionOnSignal { get; set; }
    public void ReceiveSignal(bool value) => ActionOnSignal?.Invoke(value);
}

}