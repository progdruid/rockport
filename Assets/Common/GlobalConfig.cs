using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Map
{

[CreateAssetMenu(menuName = "Lyport/GlobalSettings")]
public class GlobalConfig : ScriptableObject
{
    //static part///////////////////////////////////////////////////////////////////////////////////////////////////////
    public static GlobalConfig Ins => (Instance ??= Load());
    
    
    private static GlobalConfig Instance;
    private static GlobalConfig Load()
    {
        var asset = Resources.Load<GlobalConfig>("GlobalConfig");
        
        Assert.IsNotNull(asset.standardMaterial);
        Assert.IsNotNull(asset.worldTextureMaskMaterial);

        asset.StandardMaterial = new Material(asset.standardMaterial);
        asset.WorldTextureMaskMaterial = new Material(asset.worldTextureMaskMaterial);
        
        asset.standardMaterial.SetColor(RockUtil.FogColorID, asset.fogColor);
        asset.WorldTextureMaskMaterial.SetColor(RockUtil.FogColorID, asset.fogColor);
        
        Assert.IsNotNull(asset.entityFactory);
        Assert.IsFalse(string.IsNullOrEmpty(asset.spawnPointEntityName)); 
        
        return asset;
    }
    
    
    //fields////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [Header("Materials")]
    [SerializeField] public Color fogColor;
    [SerializeField] private Material standardMaterial;
    [SerializeField] private Material worldTextureMaskMaterial;
    
    [Header("Entities")]
    [SerializeField] public EntityFactory entityFactory;
    [SerializeField] public string spawnPointEntityName;

    //public interface//////////////////////////////////////////////////////////////////////////////////////////////////
    public Material StandardMaterial { get; private set; }
    public Material WorldTextureMaskMaterial { get; private set; }
}

}