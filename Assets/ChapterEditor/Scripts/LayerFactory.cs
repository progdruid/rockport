using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

[CreateAssetMenu(menuName = "Lyport/Layer Factory")]
public class LayerFactory : ScriptableObject
{
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [SerializeField] private GameObject[] prefabs;
    private Dictionary<string, GameObject> _prefabMap;
    
    [NonSerialized]
    private bool _initialized = false;
    
    //initialisation////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Initialise()
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
        if (!_initialized)
        {
            Initialise();
            _initialized = true;
        }
        
        var createdObject = Instantiate(_prefabMap[uniqueTitle]);
        var manipulator = createdObject.GetComponent<ManipulatorBase>();
        return manipulator;
    }
}

}