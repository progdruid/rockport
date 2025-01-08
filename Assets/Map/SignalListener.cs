namespace Map
{

public class SignalListener : IEntityModule
{
    MapEntity IEntityModule.Entity { get; set; }
    string IEntityModule.ModuleName { get; set; }
    
    public System.Action<bool> ActionOnSignal { get; set; }
    public void ReceiveSignal(bool value) => ActionOnSignal?.Invoke(value);
}

}