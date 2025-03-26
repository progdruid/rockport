using System;
using MapEditor;
using SimpleJSON;

namespace Map
{

[Serializable]
public struct EntityModulePath : IEquatable<EntityModulePath>, IReplicable
{
    public int EntityIndex { get; private set; }
    public string DependencyKey { get; private set; }

    public EntityModulePath(int entityIndex, string dependencyKey)
    {
        EntityIndex = entityIndex;
        DependencyKey = dependencyKey;
    }

    public override bool Equals(object obj) =>
        obj is EntityModulePath other && Equals(other);

    public bool Equals(EntityModulePath other) =>
        EntityIndex == other.EntityIndex && DependencyKey == other.DependencyKey;

    public override int GetHashCode() =>
        HashCode.Combine(EntityIndex, DependencyKey);

    public static bool operator ==(EntityModulePath left, EntityModulePath right) =>
        left.Equals(right);

    public static bool operator !=(EntityModulePath left, EntityModulePath right) =>
        !left.Equals(right);

    public JSONNode ExtractData()
    {
        var json = new JSONString ($"{EntityIndex}:{DependencyKey}");
        return json;
    }

    public void Replicate(JSONNode data)
    {
        var split = data.Value.Split(':');
        EntityIndex = int.Parse(split[0]);
        DependencyKey = split[1];
    }
}

}