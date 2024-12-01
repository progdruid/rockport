public interface IPolySerializable
{
    public abstract string SerializeData();
    public abstract void DeserializeData(string data);
}