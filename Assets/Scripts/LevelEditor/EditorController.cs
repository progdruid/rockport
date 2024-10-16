using System;
using UnityEngine;

namespace LevelEditor
{
    public class EditorController : MonoBehaviour
    {
        private enum EditorMode
        {
            DirtPlacement,
            DirtCarving
        }
    
        [SerializeField] private Generator DirtGenerator;
        [SerializeField] private Camera ControlCamera;

        private EditorMode _currentMode;
        private Action<Vector2> _currentToolAction;
    
        private void Start()
        {
            _currentMode = EditorMode.DirtPlacement;
            _currentToolAction = PerformDirtPlacement;
        }

        void Update()
        {
            CheckModeInput();
        
            var constructive = Input.GetMouseButton(0);
            var destructive = Input.GetMouseButton(1);
        
            if (constructive == destructive)
                return;
        
            var mousePos = Input.mousePosition;
            var ray = ControlCamera.ScreenPointToRay(mousePos);

            var t = (DirtGenerator.GetZ() - ray.origin.z) / ray.direction.z;
            var worldPos = (ray.origin + ray.direction * t);
        
            _currentToolAction(worldPos);
        }

        private void CheckModeInput()
        {
            var changeToPlacement = Input.GetKeyDown(KeyCode.Q);
            var changeToCarving = Input.GetKeyDown(KeyCode.W);
            if (_currentMode != EditorMode.DirtPlacement && changeToPlacement && !changeToCarving)
            {
                _currentMode = EditorMode.DirtPlacement;
                _currentToolAction = PerformDirtPlacement;
            }
            else if (_currentMode != EditorMode.DirtCarving && changeToCarving && !changeToPlacement)
            {
                _currentMode = EditorMode.DirtCarving;
                _currentToolAction = PerformDirtCarving;
            }
        }

        private void PerformDirtPlacement(Vector2 pos) => DirtGenerator.ChangeTileAtWorldPos(pos, true);

        private void PerformDirtCarving(Vector2 pos) => DirtGenerator.ChangeTileAtWorldPos(pos, false);
    }
}
