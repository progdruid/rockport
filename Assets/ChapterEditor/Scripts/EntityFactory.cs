using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace ChapterEditor
{

[CreateAssetMenu(menuName = "Lyport/Layer Factory")]
public class EntityFactory : ScriptableObject
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
            var entity = prefab.GetComponent<MapEntity>();
            Assert.IsNotNull(entity);
            var uniqueTitle = entity.Title;
            _prefabMap.Add(uniqueTitle, prefab);
        }
    }

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public MapEntity CreateEntity(string uniqueTitle)
    {
        if (!_initialized)
        {
            Initialise();
            _initialized = true;
        }
        
        var createdObject = Instantiate(_prefabMap[uniqueTitle]);
        var entity = createdObject.GetComponent<MapEntity>();
        return entity;
    }
}

}