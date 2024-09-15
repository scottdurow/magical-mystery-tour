using System;

namespace PaymentVirtualTableProvider.Services;

/// <summary>
/// Represents a service IOC container.
/// </summary>
public interface IServiceIOCContainer
{
    /// <summary>
    /// Registers a service implementation.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="name">Optional name of the service.</param>
    void Register<TService, TImplementation>(string name = "") where TImplementation : TService;

    /// <summary>
    /// Registers a service implementation instance.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <typeparam name="TImplementation">The implementation type.</typeparam>
    /// <param name="instance">The instance of the implementation type.</param>
    /// <param name="name">Optional name of the service.</param>
    void Register<TService, TImplementation>(TImplementation instance, string name = "") where TImplementation : TService;

    /// <summary>
    /// Registers a service factory method.
    /// </summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="serviceFactoryMethod">The factory method that creates the service instance.</param>
    /// <param name="name">Optional name of the service.</param>
    void Register<TService>(Func<IServiceProvider, TService> serviceFactoryMethod, string name = "");
}