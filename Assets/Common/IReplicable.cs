using SimpleJSON;

namespace Map
{

public interface IReplicable
{
    public abstract void Replicate(JSONNode data);
    public abstract JSONNode ExtractData();
}

}