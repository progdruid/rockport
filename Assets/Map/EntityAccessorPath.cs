using System;
using MapEditor;
using SimpleJSON;

namespace Map
{

[Serializable]
public struct EntityAccessorPath : IEquatable<EntityAccessorPath>, IReplicable
{
    public int EntityIndex { get; private set; }
    public string AccessorName { get; private set; }

    public EntityAccessorPath(int entityIndex, string accessorName)
    {
        EntityIndex = entityIndex;
        AccessorName = accessorName;
    }

    public override bool Equals(object obj) =>
        obj is EntityAccessorPath other && Equals(other);

    public bool Equals(EntityAccessorPath other) =>
        EntityIndex == other.EntityIndex && AccessorName == other.AccessorName;

    public override int GetHashCode() =>
        HashCode.Combine(EntityIndex, AccessorName);

    public static bool operator ==(EntityAccessorPath left, EntityAccessorPath right) =>
        left.Equals(right);

    public static bool operator !=(EntityAccessorPath left, EntityAccessorPath right) =>
        !left.Equals(right);

    public JSONNode ExtractData()
    {
        var json = new JSONString ($"{EntityIndex}:{AccessorName}");
        return json;
    }

    public void Replicate(JSONNode data)
    {
        var split = data.Value.Split(':');
        EntityIndex = int.Parse(split[0]);
        AccessorName = split[1];
    }
}

}