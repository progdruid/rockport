using UnityEngine;

namespace MapEditor
{

public interface IMapEditorMode
{
    public void Enter();
    public void Exit();
    public void HandleInput(Vector2 worldMousePos);
}

}