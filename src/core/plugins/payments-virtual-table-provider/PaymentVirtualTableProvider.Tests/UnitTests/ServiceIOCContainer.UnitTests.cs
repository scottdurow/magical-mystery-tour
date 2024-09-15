using Microsoft.VisualStudio.TestTools.UnitTesting;
using PaymentVirtualTableProvider.Services;
using Microsoft.Xrm.Sdk.Extensions;
using System;

namespace PaymentVirtualTableProvider.Tests.UnitTests;

[TestClass]
public class ServiceIOCContainerTests
{
  
    public interface IServiceA { }

    public class ServiceA : IServiceA { }

    public interface IServiceB { }

    public class ServiceB : IServiceB { }

    public interface IServiceC
    {
        IServiceA ServiceA { get; }

        IServiceB ServiceB { get; }
    }

    public class ServiceC : IServiceC
    {
        public IServiceA ServiceA { get; }

        public IServiceB ServiceB { get; }

        public ServiceC(IServiceA serviceA, IServiceB serviceB)
        {
            ServiceA = serviceA;
            ServiceB = serviceB;
        }
    }

    /// <summary>
    /// A test for resolving ServiceA by type.
    /// </summary>
    [TestMethod]
    public void ServiceA_Resolved_By_Type()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        container.Register<IServiceA, ServiceA>();

        // Act
        var serviceA = container.Get<IServiceA>();

        // Assert
        Assert.IsNotNull(serviceA);
        Assert.IsInstanceOfType(serviceA, typeof(ServiceA));
    }

    /// <summary>
    /// A test for resolving ServiceB by instance.
    /// </summary>
    [TestMethod]
    public void ServiceB_Resolved_By_Instance()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        var serviceBInstance = new ServiceB();
        container.Register<IServiceB, ServiceB>(serviceBInstance);

        // Act
        var serviceB = container.Get<IServiceB>();

        // Assert
        Assert.IsNotNull(serviceB);
        Assert.AreSame(serviceBInstance, serviceB);
    }

    /// <summary>
    /// A test for resolving ServiceC by factory.
    /// </summary>
    [TestMethod]
    public void ServiceC_Resolved_By_Factory()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        container.Register<IServiceA, ServiceA>();
        container.Register<IServiceB, ServiceB>();
        container.Register<IServiceC>(sp => new ServiceC(sp.Get<IServiceA>(), sp.Get<IServiceB>()));

        // Act
        var serviceC = container.Get<IServiceC>();

        // Assert
        Assert.IsNotNull(serviceC);
        Assert.IsInstanceOfType(serviceC, typeof(ServiceC));
        Assert.IsNotNull(serviceC.ServiceA);
        Assert.IsNotNull(serviceC.ServiceB);
    }

    /// <summary>
    /// A test for resolving ServiceC with dependencies.
    /// </summary>
    [TestMethod]
    public void ServiceC_With_Dependencies()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        container.Register<IServiceA, ServiceA>();
        container.Register<IServiceB, ServiceB>();
        container.Register<IServiceC, ServiceC>();

        // Act
        // The constructor of ServiceC has dependencies on IServiceA and IServiceB
        // These will automatically be supplied by the IOC Container
        var serviceC = container.Get<IServiceC>();

        // Assert
        Assert.IsNotNull(serviceC);
        Assert.IsInstanceOfType(serviceC, typeof(ServiceC));
        Assert.IsNotNull(serviceC.ServiceA);
        Assert.IsNotNull(serviceC.ServiceB);
    }

    /// <summary>
    /// Test where two services are registered, one has a name specified
    /// </summary>
    [TestMethod]
    public void Two_Services_One_With_Name_Specified()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        var defaultServiceAInstance = new ServiceA();
        var namedServiceAInstance = new ServiceA();
        container.Register<IServiceA, ServiceA>(defaultServiceAInstance);
        container.Register<IServiceA, ServiceA>(namedServiceAInstance, "named");

        // Act
        var defaultServiceA = container.GetService(typeof(IServiceA)) as IServiceA;
        var namedServiceA = container.GetService(typeof(IServiceA), "named") as IServiceA;

        // Assert
        Assert.IsNotNull(defaultServiceA);
        Assert.AreSame(defaultServiceAInstance, defaultServiceA);
        Assert.IsNotNull(namedServiceA);
        Assert.AreSame(namedServiceAInstance, namedServiceA);
        Assert.AreNotSame(defaultServiceA, namedServiceA);
    }

    /// <summary>
    /// Test where two services are registered using a factory method, one has a name specified
    /// </summary>
    [TestMethod]
    public void Two_Services_One_With_Name_Specified_Using_Factory()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        container.Register<IServiceA>(sp => new ServiceA());
        container.Register<IServiceA>(sp => new ServiceA(), "named");

        // Act
        var defaultServiceA = container.GetService(typeof(IServiceA)) as IServiceA;
        var namedServiceA = container.GetService(typeof(IServiceA), "named") as IServiceA;

        // Assert
        Assert.IsNotNull(defaultServiceA);
        Assert.IsInstanceOfType(defaultServiceA, typeof(ServiceA));
        Assert.IsNotNull(namedServiceA);
        Assert.IsInstanceOfType(namedServiceA, typeof(ServiceA));
        Assert.AreNotSame(defaultServiceA, namedServiceA);
    }

    [TestMethod]
    public void Request_Service_With_No_Name_Throws_Exception()
    {
        // Arrange
        var container = new ServiceIOCContainer();
        container.Register<IServiceA>(sp => new ServiceA(), "first");
        container.Register<IServiceA>(sp => new ServiceA(), "second");

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            var serviceA = container.GetService(typeof(IServiceA)) as IServiceA;
        });

    }
}
