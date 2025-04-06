using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Map
{

public class EntityOverlapAccessor : IEntityAccessor
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    MapEntity IEntityAccessor.Entity { get; set; }
    string IEntityAccessor.AccessorName { get; set; }
    
    private readonly Func<Vector2, bool> _overlapCheck;
    
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    public EntityOverlapAccessor(Func<Vector2, bool> overlapCheck)
    {
        Assert.IsNotNull(overlapCheck);
        _overlapCheck = overlapCheck;
    }
    
    
    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public bool CheckOverlap(Vector2 position)
    {
        Assert.IsNotNull(_overlapCheck);
        return _overlapCheck.Invoke(position);
    }
}

}