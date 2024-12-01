public interface IPolySerializable
{
    public abstract string Serialize();
    public abstract void Deserialize(string data);
}