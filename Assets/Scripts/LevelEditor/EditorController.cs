using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace LevelEditor
{
    public class EditorController : MonoBehaviour
    {
        [SerializeField] private LevelSpaceHolder holder;
        [SerializeField] private Camera controlCamera;
        [Space]
        [SerializeField] private GameObject treeBackgroundPrefab;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject dirtPrefab;
        [SerializeField] private GameObject objectPrefab;
        
        private int _selectedLayer = -1;
        private IPlaceRemoveHandler _placeRemoveHandler = null;

        private void Awake()
        {
            Assert.IsNotNull(holder);
        }

        private void Update()
        {
            CheckLayerCreation();
            if (holder.HasLayer(_selectedLayer))
                CheckSelection();
            if (_placeRemoveHandler != null)
                CheckPlaceRemove();
        }

        private void CheckLayerCreation()
        {
            if (Input.GetKeyDown(KeyCode.B))
                CreateLayer(treeBackgroundPrefab);
            else if (Input.GetKeyDown(KeyCode.T))
                CreateLayer(treePrefab);
            else if (Input.GetKeyDown(KeyCode.D))
                CreateLayer(dirtPrefab);
            else if (Input.GetKeyDown(KeyCode.O)) 
                CreateLayer(objectPrefab);
        }

        private void CreateLayer(GameObject prefab)
        {
            if (holder.HasLayer(_selectedLayer))
                holder.GetManipulator(_selectedLayer).UnsubscribeInput();
            
            var go = Instantiate(prefab);
            var manipulator = go.GetComponent<ManipulatorBase>();
            _selectedLayer++;
            holder.RegisterAt(manipulator, _selectedLayer);
            
            manipulator.SubscribeInput(this);
            
            Debug.Log($"Created {manipulator.ManipulatorName} at index {_selectedLayer}");
        }

        private void CheckSelection()
        {
            var dir = (Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0) + (Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0);
            var newLayer = Mathf.Clamp(_selectedLayer + dir, 0, holder.ManipulatorsCount-1);
            if (newLayer == _selectedLayer)
                return;
            
            holder.GetManipulator(_selectedLayer).UnsubscribeInput();
            
            var manipulator = holder.GetManipulator(newLayer);
            manipulator.SubscribeInput(this);
            
            _selectedLayer = newLayer;

            Debug.Log($"Selected {manipulator.ManipulatorName} at index {_selectedLayer}");
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
