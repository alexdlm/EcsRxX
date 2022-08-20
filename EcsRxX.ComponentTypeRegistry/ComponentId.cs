namespace EcsRx.ComponentTypeRegistry;

public readonly record struct ComponentId(ushort Id)
{
    public ushort Id { get; } = Id;
}
