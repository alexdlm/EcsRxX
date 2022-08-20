// Copyright (C) Alex de la Mare, 2020. All Rights Reserved.

using System.Collections;
using System.Diagnostics.Contracts;
using Autofac;
using Autofac.Builder;
using Autofac.Features.AttributeFilters;
using SystemsRx.Infrastructure.Dependencies;
using static Autofac.ResolutionExtensions;

namespace SystemsRxX.Infrastructure.Autofac;

/// <summary>
/// Based on SystemsRx.Infrastructure.Ninject
/// </summary>
public sealed class AutofacDependencyContainer : IDependencyContainer
{
    private IContainer? container;

    public AutofacDependencyContainer() : this(new ContainerBuilder())
    {
    }

    public AutofacDependencyContainer(ContainerBuilder builder) => this.Builder = builder;

    public ContainerBuilder Builder { get; private set; }
    
    public IContainer Container => this.AssertContainer();

    public object NativeContainer => this.AssertContainer();

    private IContainer AssertContainer()
    {
        Contract.Requires(this.container != null);
        return this.container!;
    }

    public void Bind(
        Type fromType,
        Type toType,
        BindingConfiguration? configuration = null)
    {
        if (configuration == null)
        {
            this.Builder
                .RegisterType(toType)
                .As(fromType)
                .SingleInstance()
                .WithAttributeFiltering();
            return;
        }

        IRegistrationBuilder<object, object, object> binding;

        if (configuration.ToInstance != null)
        {
            binding = this.Builder.RegisterInstance(configuration.ToInstance);
        }
        else if (configuration.ToMethod != null)
        {
            binding = this.Builder
                .Register(x => configuration.ToMethod(this))
                .As(fromType);
        }
        else
        {
            var typedBinding = this.Builder.RegisterType(toType).As(fromType).WithAttributeFiltering();
            binding = typedBinding;

            foreach (var constructorArg in configuration.WithNamedConstructorArgs)
            {
                typedBinding.WithParameter(
                    constructorArg.Key,
                    constructorArg.Value);
            }

            foreach (var constructorArg in configuration.WithTypedConstructorArgs)
            {
                typedBinding.WithParameter(
                    (info, context) => info.ParameterType == constructorArg.Key,
                    (info, context) => constructorArg.Value);
            }
        }

        if (configuration.AsSingleton)
        {
            binding.SingleInstance();
        }

        if (!string.IsNullOrEmpty(configuration.WithName))
        {
            binding.Named(configuration.WithName, fromType);
        }

        if (configuration.OnActivation != null)
        {
            binding.OnActivated(instance => configuration.OnActivation(this, instance));
        }

        if (configuration.WhenInjectedInto.Count != 0)
        {
            throw new NotImplementedException("WhenInjectedInto not supported");
            // configuration.WhenInjectedInto.ForEachRun(x => binding.WhenInjectedInto(x));
        }
    }

    public void Bind(
        Type type,
        BindingConfiguration? configuration = null) => this.Bind(type, type, configuration);

    public bool HasBinding(Type type, string? name = null)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return AssertContainer().IsRegisteredWithName(name!, type);
        }

        return AssertContainer().IsRegistered(type);
    }

    public object Resolve(Type type, string? name = null)
    {
        if (string.IsNullOrEmpty(name))
        {
            return AssertContainer().Resolve(type);
        }

        return AssertContainer().ResolveNamed(name!, type);
    }

    public void Unbind(Type type)
    {
        // TODO: maybe log at least.
    }

    public IEnumerable ResolveAll(Type type) =>
        (IEnumerable)AssertContainer().Resolve(typeof(IEnumerable<>).MakeGenericType(type));

    public void LoadModule(IDependencyModule module) => module.Setup(this);

    public void Dispose() => this.container?.Dispose();

    public void SetContainer(IContainer newContainer)
    {
        Contract.Requires(this.container == null, "Container can not have been set/built before");
        this.Builder = null!;
        this.container = newContainer;
    }
    
    public IContainer Build()
    {
        SetContainer(this.Builder.Build());
        return this.container!;
    }
}