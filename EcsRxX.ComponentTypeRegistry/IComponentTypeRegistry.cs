namespace EcsRx.ComponentTypeRegistry;

public interface IComponentTypeRegistry
{
    Dictionary<Type, ComponentId> RegisteredTypes { get; } 
}