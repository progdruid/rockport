namespace Map
{

public interface IEntityModule
{
    protected MapEntity Entity { get; set; }
    protected string ModuleName { get; set; }

    public void Initialise(string moduleName, MapEntity entity)
    {
        Entity = entity;
        ModuleName = moduleName;
    }
    
    public EntityModulePath GetModulePath() => new(Entity.Layer, ModuleName);
}

}