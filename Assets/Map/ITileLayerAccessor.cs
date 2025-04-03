using UnityEngine;

namespace Map
{

public interface ITileLayerAccessor : IEntityAccessor
{
    public void ChangeAtWorldPos(Vector2 worldPos, bool constructive);
    public void ChangeAtMapPos(Vector2Int rootPos, bool constructive);
}

}