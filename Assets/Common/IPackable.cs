namespace Map
{

public interface IPackable
{
    public abstract string Pack();
    public abstract void Unpack(string data);
}

}