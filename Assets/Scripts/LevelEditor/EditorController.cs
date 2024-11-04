using System;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace LevelEditor
{
    public class EditorController : MonoBehaviour
    {
        private enum EditorMode
        {
            Dirt,
            Tree
        }
    
        [SerializeField] private DirtManipulator dirtManipulator;
        [SerializeField] private TreeManipulator treeManipulator;
        [SerializeField] private Camera controlCamera;

        private EditorMode _currentMode;
        private Action<Vector2, bool> _currentToolAction;

        private void Awake()
        {
            Assert.IsNotNull(dirtManipulator);
            Assert.IsNotNull(treeManipulator);
        }

        private void Start()
        {
            _currentMode = EditorMode.Dirt;
            _currentToolAction = PerformDirtAction;
        }

        void Update()
        {
            CheckModeInput();
        
            var constructive = Input.GetMouseButton(0);
            var destructive = Input.GetMouseButton(1);
        
            if (constructive == destructive)
                return;
        
            var mousePos = Input.mousePosition;
            var ray = controlCamera.ScreenPointToRay(mousePos);

            var t = (dirtManipulator.GetZ() - ray.origin.z) / ray.direction.z;
            var worldPos = (ray.origin + ray.direction * t);
        
            _currentToolAction(worldPos, constructive && !destructive);
        }

        private void CheckModeInput()
        {
            var changeToDirt = Input.GetKeyDown(KeyCode.D);
            var changeToTree = Input.GetKeyDown(KeyCode.T);
            if (_currentMode != EditorMode.Dirt && changeToDirt && !changeToTree)
            {
                _currentMode = EditorMode.Dirt;
                _currentToolAction = PerformDirtAction;
            }
            else if (_currentMode != EditorMode.Tree && changeToTree && !changeToDirt)
            {
                _currentMode = EditorMode.Tree;
                _currentToolAction = PerformTreeAction;
            }
        }

        private void PerformDirtAction(Vector2 pos, bool constructiveNotDestructive) =>
            dirtManipulator.ChangeTileAtWorldPos(pos, constructiveNotDestructive);

        private void PerformTreeAction(Vector2 pos, bool constructiveNotDestructive) =>
            treeManipulator.ChangeTileAtWorldPos(pos, constructiveNotDestructive);
    }
}
