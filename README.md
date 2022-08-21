### EcsRxX

Various extensions to [EcsRx](https://github.com/EcsRx/ecsrx) & related projects.

#### SystemsRxX.Infrastructure.Autofac

Autofac DependencyContainer implementation. This is largely so [SystemsRx](https://github.com/EcsRx/SystemsRx) injection works, though you may continue using
IDependencyContainer in your own code if you desire, sticking with the Autofac interfaces seems nicer to me.

```csharp
class MyApplication : SystemsRxApplication {
    
    private AutofacDependencyContainer AutofacContainer {get;} = new AutofacDependendencyContainer();
    public abstract IDependencyContainer Container { get; } => AutofacContainer;
    
    protected override virtual void LoadModules() {
        // Registers the SystemsRx framework
        base.LoadModules();
        // Use SystemsRx interfaces to load custom module
        Container.LoadModule(new MyGameModule());
        // Hit autofac directly for more control
        ContainerBuilder builder = AutofacContainer.Builder;
        builder.RegisterType<Foo>.as<IFoo>();
        builder.RegisterModule<MyAutofacGameModule>();
        builder.RegisterAssemblyTypes(myAssembly)
            .Where(t => t.Name.EndsWith("System"))
            .AsImplementedInterfaces();
       
        // Finally build the container. No more binding possible, but now Resolve is possible.
        AutofacContainer.Build();
    }
    
    protected override virtual void ResolveApplicationDependencies() {
        base.ResolveApplicationDependencies();
        
        Foo = Container.Resolve<IFoo>();
        
        // But it's better to skip the middle man and use autofac directly
        Foo = AutofacContainer.Container.Resolve<IFoo>();
    }
}
```

Alternately, you can pass in your own `ContainerBuilder` and/or `Build` it yourself:

```csharp
    ContainerBuilder builder = new ContainerBuilder();
    AutofacDependencyContainer autofacContainer = new AutofacDependencyContainer(builder);
    // ...
    autofacContainer.SetContainer(builder.Build());
```
