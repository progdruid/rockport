namespace Map
{

public interface IEntityAccessor
{
    protected MapEntity Entity { get; set; }
    protected string AccessorName { get; set; }

    public void Initialise(string moduleName, MapEntity entity)
    {
        Entity = entity;
        AccessorName = moduleName;
    }
    
    public EntityAccessorPath GetAccessorPath() => new(Entity.Layer, AccessorName);
    public MapEntity GetEntity() => Entity;
}

}