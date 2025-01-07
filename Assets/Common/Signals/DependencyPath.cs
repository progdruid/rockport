using System;

namespace Common
{
public readonly struct DependencyPath : IEquatable<DependencyPath>
{
    public readonly int EntityIndex { get; }
    public readonly string DependencyKey { get; }

    public DependencyPath(int entityIndex, string dependencyKey)
    {
        EntityIndex = entityIndex;
        DependencyKey = dependencyKey;
    }

    public override bool Equals(object obj) =>
        obj is DependencyPath other && Equals(other);

    public bool Equals(DependencyPath other) =>
        EntityIndex == other.EntityIndex && DependencyKey == other.DependencyKey;

    public override int GetHashCode() =>
        HashCode.Combine(EntityIndex, DependencyKey);

    public static bool operator ==(DependencyPath left, DependencyPath right) =>
        left.Equals(right);

    public static bool operator !=(DependencyPath left, DependencyPath right) =>
        !left.Equals(right);
}
}