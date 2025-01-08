using System;
using MapEditor;

namespace Map
{

[Serializable]
public struct EntityModulePath : IEquatable<EntityModulePath>, IPackable
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

    public string Pack() => $"{EntityIndex}:{DependencyKey}";

    public void Unpack(string data)
    {
        var parts = data.Split(':');
        if (parts.Length != 2 || !int.TryParse(parts[0], out var entityIndex))
            throw new FormatException("Invalid entity module path data");
        EntityIndex = entityIndex;
        DependencyKey = parts[1];
    }
}

}