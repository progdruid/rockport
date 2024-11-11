
using UnityEngine;

public interface IPlaceRemoveHandler
{
    public abstract void ChangeAt(Vector2Int pos, bool shouldPlaceNotRemove);
    public abstract float GetZForInteraction();
}
