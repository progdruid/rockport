using SimpleJSON;

namespace Map
{

public interface IReplicable
{
    public abstract void Replicate(JSONObject data);
    public abstract JSONObject ExtractData();
}

}