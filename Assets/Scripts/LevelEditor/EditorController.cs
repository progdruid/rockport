using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace LevelEditor
{
    public class EditorController : MonoBehaviour
    {
        //fields////////////////////////////////////////////////////////////////////////////////////////////////////////
        [SerializeField] private LevelSpaceHolder holder;
        [SerializeField] private Camera controlCamera;
        [SerializeField] private ManipulatorUIPanel manipulatorUIPanel;
        [Space]
        [SerializeField] private GameObject treeBackgroundPrefab;
        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject dirtPrefab;
        [SerializeField] private GameObject objectPrefab;
        
        private int _selectedLayer = -1;
        private IPlaceRemoveHandler _placeRemoveHandler = null;

        private bool _canEdit = true;
        
        //initialisation////////////////////////////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            Assert.IsNotNull(holder);
            Assert.IsNotNull(controlCamera);
            Assert.IsNotNull(manipulatorUIPanel);
            Assert.IsNotNull(treeBackgroundPrefab);
            Assert.IsNotNull(treePrefab);
            Assert.IsNotNull(dirtPrefab);
            Assert.IsNotNull(objectPrefab);

            manipulatorUIPanel.EditingStateChangeEvent += fieldSelected => _canEdit = !fieldSelected;
        }
        
        //public interface//////////////////////////////////////////////////////////////////////////////////////////////
        public void SetPlaceRemoveHandler (IPlaceRemoveHandler handler) => _placeRemoveHandler = handler;
        public void UnsetPlaceRemoveHandler () => _placeRemoveHandler = null;

        public void SetPropertyHolder(IPropertyHolder propertyHolder) =>
            manipulatorUIPanel.CreateUIFields(propertyHolder.GetProperties());
        public void UnsetPropertyHolder () => manipulatorUIPanel.ClearProperties();

        //game loop/////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            if (!_canEdit) return;
            
            CheckLayerCreation();
            if (holder.HasLayer(_selectedLayer))
                CheckLayerDeletion();
            if (holder.HasLayer(_selectedLayer))
                CheckSelection();
            if (_placeRemoveHandler != null)
                CheckPlaceRemove();
        }

        //private logic/////////////////////////////////////////////////////////////////////////////////////////////////
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
        }
        
        private void CheckLayerDeletion()
        {
            if (!Input.GetKeyDown(KeyCode.Delete))
                return;
            
            var dead = holder.GetManipulator(_selectedLayer);
            dead.UnsubscribeInput();
            holder.UnregisterAt(_selectedLayer);
            Destroy(dead.Target.gameObject);
            _selectedLayer = holder.ClampLayer(_selectedLayer);
            if (holder.HasLayer(_selectedLayer))
                holder.GetManipulator(_selectedLayer).SubscribeInput(this);
            else
                _selectedLayer = -1;
        }

        private void CheckSelection()
        {
            var dir = (Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0) + (Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0);
            var newLayer = holder.ClampLayer(_selectedLayer + dir);
            if (newLayer == _selectedLayer)
                return;

            holder.GetManipulator(_selectedLayer).UnsubscribeInput();
            
            var manipulator = holder.GetManipulator(newLayer);
            manipulator.SubscribeInput(this);
            
            _selectedLayer = newLayer;
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
    }
}
