using Microsoft.Xrm.Sdk;
using Moq;
using ContosoRealEstate.BusinessLogic.Services;
using System.Collections.Generic;
using ContosoRealEstate.BusinessLogic.Plugins;
using System;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests;

public class PluginUnitTestBase
{
    protected (Mock<ILocalPluginContext> mockLocalPluginContext, Mock<IOrganizationService> mockOrganizationService) SetupMockLocalPluginContext(bool UsePowerFxPlugins = false)
    {
        // Create a Service Provider 
        var serviceContainer = new ServiceIOCContainer();

        // Mock the EnvironmentService
        var mockEnvironmentVariableService = new Mock<IEnvironmentVariableService>();
        mockEnvironmentVariableService.Setup(x => x.RetrieveEnvironmentVariableValue("contoso_FCB_UsePowerFxPlugins"))
            .Returns(UsePowerFxPlugins ? "yes" : "no");
        serviceContainer.Register<IEnvironmentVariableService, IEnvironmentVariableService>(mockEnvironmentVariableService.Object);

        // Mock the OrganizationService
        var mockOrganizationService = new Mock<IOrganizationService>();
        serviceContainer.Register<IOrganizationService, IOrganizationService>(mockOrganizationService.Object);

        // Mock the Execution Context
        var mockLocalPluginContext = new Mock<ILocalPluginContext>();
        var mockPluginExecutionContext = new Mock<IPluginExecutionContext>();
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext)
                .Returns(mockPluginExecutionContext.Object);

        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.OutputParameters)
            .Returns(new ParameterCollection());
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters)
            .Returns(new ParameterCollection());

        // Mock the tracing service
        serviceContainer.Register<ITracingService, ITracingService>(new Mock<ITracingService>().Object);

        // Add the service provider to the mock local plugin context
        mockLocalPluginContext.Setup(context => context.ServiceProvider)
            .Returns(serviceContainer);

        return (mockLocalPluginContext, mockOrganizationService);
    }

    // Helper method to create mock reservation entity
    public static Entity CreateReservationEntity(Guid listingId, DateTime from, DateTime to)
    {
        var reservation = new Entity("contoso_Reservation")
        {
            Id = Guid.NewGuid()
        };
        reservation["contoso_From"] = from;
        reservation["contoso_To"] = to;
        reservation["contoso_Listing"] = new EntityReference("contoso_listing", listingId);

        return reservation;
    }
}
