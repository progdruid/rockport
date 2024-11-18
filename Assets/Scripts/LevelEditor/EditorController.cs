using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace LevelEditor
{
    public class EditorController : MonoBehaviour
    {
        [SerializeField] private LevelSpaceHolder holder;
        [SerializeField] private Camera controlCamera;

        private int selectedManipulatorIndex;
        private IPlaceRemoveHandler _placeRemoveHandler = null;

        private void Awake()
        {
            Assert.IsNotNull(holder);
        }

        private void Start()
        {
            selectedManipulatorIndex = 0;
            holder.GetManipulator(selectedManipulatorIndex).SubscribeInput(this);
        }

        private void Update()
        {
            CheckSelection();
            
            if (_placeRemoveHandler != null)
                CheckPlaceRemove();
        }

        private void CheckSelection()
        {
            var dir = (Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0);
            var newIndex = Mathf.Clamp(selectedManipulatorIndex + dir, 0, holder.ManipulatorsCount-1);
            if (newIndex == selectedManipulatorIndex)
                return;
            
            holder.GetManipulator(selectedManipulatorIndex).UnsubscribeInput();
            holder.GetManipulator(newIndex).SubscribeInput(this);
            
            selectedManipulatorIndex = newIndex;
        }
        
        private void CheckPlaceRemove()
        {
            var constructive = Input.GetMouseButton(0);
            var destructive = Input.GetMouseButton(1);
        
            if (constructive == destructive)
                return;
        
            var mousePos = Input.mousePosition;
            var ray = controlCamera.ScreenPointToRay(mousePos);
            
            var t = (_placeRemoveHandler.GetZForInteraction() - ray.origin.z) / ray.direction.z;
            var worldPos = (ray.origin + ray.direction * t);

            _placeRemoveHandler.ChangeAt(worldPos, constructive && !destructive);
        }
        
        public void SetPlaceRemoveHandler (IPlaceRemoveHandler handler) => _placeRemoveHandler = handler;
        public void UnsetPlaceRemoveHandler () => _placeRemoveHandler = null;
        
    }
}
