using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LayerFactory : MonoBehaviour
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject[] prefabs;
    private Dictionary<string, GameObject> _prefabMap;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////

    private void Awake()
    {
        Assert.IsNotNull(prefabs);
        
        _prefabMap = new Dictionary<string, GameObject>();
        
        foreach (var prefab in prefabs)
        {
            var manipulator = prefab.GetComponent<ManipulatorBase>();
            Assert.IsNotNull(manipulator);
            var uniqueTitle = manipulator.ManipulatorName;
            _prefabMap.Add(uniqueTitle, prefab);
        }
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public ManipulatorBase CreateManipulator(string uniqueTitle)
    {
        var createdObject = Instantiate(_prefabMap[uniqueTitle]);
        var manipulator = createdObject.GetComponent<ManipulatorBase>();
        return manipulator;
    }
}
