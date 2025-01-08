using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Map
{

[CreateAssetMenu(menuName = "Lyport/GlobalSettings")]
public class GlobalConfig : ScriptableObject
{
    public static GlobalConfig Ins => (Instance ??= Load());
    
    
    private static GlobalConfig Instance;
    private static GlobalConfig Load()
    {
        var asset = Resources.Load<GlobalConfig>("GlobalConfig");
        
        Assert.IsNotNull(asset.standardMaterial);
        Assert.IsNotNull(asset.worldTextureMaskMaterial);

        asset.StandardMaterial = new Material(asset.standardMaterial);
        asset.WorldTextureMaskMaterial = new Material(asset.worldTextureMaskMaterial);
        
        asset.standardMaterial.SetColor(Lytil.FogColorID, asset.fogColor);
        asset.WorldTextureMaskMaterial.SetColor(Lytil.FogColorID, asset.fogColor);
        
        Assert.IsNotNull(asset.entityFactory);
        Assert.IsFalse(string.IsNullOrEmpty(asset.spawnPointEntityName)); 
        
        return asset;
    }
    
    

    [SerializeField] public Color fogColor;
    [SerializeField] private Material standardMaterial;
    [SerializeField] private Material worldTextureMaskMaterial;
    [Space]
    [SerializeField] public EntityFactory entityFactory;
    [SerializeField] public string spawnPointEntityName;
    
    public Material StandardMaterial { get; private set; }
    public Material WorldTextureMaskMaterial { get; private set; }
}

}