using System;
using System.Collections.Generic;
using System.Linq;

namespace ContosoRealEstate.BusinessLogic.Services;

/// <summary>
/// Represents a container for registering and resolving services.
/// This Service Container is not intended to be a full-featured IoC container that is theadsafe.
/// It is always scoped at the plugin execution level
/// For a more comprehensive implementation of an IOC Container for Plugins, see https://github.com/daryllabar/DLaB.Xrm/wiki/IOC
/// </summary>
public class ServiceIOCContainer : IServiceProvider, IServiceIOCContainer
{
    private readonly Dictionary<(Type, string), Type> _serviceRegistry = new Dictionary<(Type, string), Type>();
    private readonly Dictionary<(Type, string), object> _instanceRegistry = new Dictionary<(Type, string), object>();
    private readonly Dictionary<(Type, string), Func<object>> _factoryRegistry = new Dictionary<(Type, string), Func<object>>();
    private readonly IServiceProvider _internalServiceProvider;

    public ServiceIOCContainer()
    {

    }
    public ServiceIOCContainer(IServiceProvider internalServiceProvider)
    {
        _internalServiceProvider = internalServiceProvider;
    }

    /// <summary>
    /// Registers a service with its implementation.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    public void Register<TService, TImplementation>(string name = "") where TImplementation : TService
    {
        _serviceRegistry[(typeof(TService), name)] = typeof(TImplementation);
    }

    /// <summary>
    /// Registers a service with its implementation and instance.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="instance">The instance of the implementation.</param>
    /// <param name="name">Optional name of the service.</param>
    public void Register<TService, TImplementation>(TImplementation instance, string name = "") where TImplementation : TService
    {
        _instanceRegistry[(typeof(TService), name)] = instance;
    }

    /// <summary>
    /// Register that accepts a service factory method
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="serviceFactoryMethod">The factory to create the type</param>
    /// <param name="name">Optional name of the service.</param>
    public void Register<TService>(Func<IServiceProvider, TService> serviceFactoryMethod, string name = "")
    {
        _factoryRegistry[(typeof(TService), name)] = () => serviceFactoryMethod(this);
    }

    /// <summary>
    /// Resolves a service and its dependencies.
    /// </summary>
    /// <typeparam name="Type">The service type.</typeparam>
    /// <returns>The resolved service instance.</returns>
    public object GetService(Type serviceType)
    {
        return Resolve(serviceType, "");
    }

    /// <summary>
    /// Resolves a service and its dependencies.
    /// </summary>
    /// <typeparam name="Type">The service type.</typeparam>
    /// <param name="name">Optional name of the service.</param>
    /// <returns>The resolved service instance.</returns>
    public object GetService(Type serviceType, string name = "")
    {
        return Resolve(serviceType, name);
    }


    /// <summary>
    /// Resolves a service and its dependencies.
    /// </summary>
    /// <param name="serviceType">The type of the service</param>
    /// <param name="name">Optional name of the service</param>
    /// <returns>Instance of the requested type</returns>
    /// <exception cref="InvalidOperationException">No service registered for the type</exception>
    private object Resolve(Type serviceType, string name = "")
    {
        // Check the internal Service Provider
        if (_internalServiceProvider != null)
        {
            var internalInstance = _internalServiceProvider.GetService(serviceType);
            if (internalInstance != null)
            {
                return internalInstance;
            }
        }
        // Check the factory registry
        if (_factoryRegistry.ContainsKey((serviceType, name)))
        {
            return _factoryRegistry[(serviceType, name)]();
        }

        // Check the instance registry
        if (_instanceRegistry.ContainsKey((serviceType, name)))
        {
            return _instanceRegistry[(serviceType, name)];
        }

        // Check that there is a service registered for the type
        if (!_serviceRegistry.ContainsKey((serviceType, name)))
        {
            throw new InvalidOperationException($"No service registered for type {serviceType} with name {name}");
        }

        // Create an instance of the type and supply any dependencies to the constructor
        var implementationType = _serviceRegistry[(serviceType, name)];
        var constructor = implementationType.GetConstructors().First();
        var parameters = constructor.GetParameters()
            .Select(param => Resolve(param.ParameterType))
            .ToArray();

        var instance = Activator.CreateInstance(implementationType, parameters);
        _instanceRegistry[(serviceType, name)] = instance;
        return instance;
    }
}
