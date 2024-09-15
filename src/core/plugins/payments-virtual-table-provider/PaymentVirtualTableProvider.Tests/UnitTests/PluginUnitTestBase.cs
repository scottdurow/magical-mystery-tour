using Microsoft.Xrm.Sdk;
using Moq;
using PaymentVirtualTableProvider.Services;
using PaymentVirtualTableProvider.Services.PaymentApi;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System.Collections.Generic;

namespace PaymentVirtualTableProvider.Tests.UnitTests;

public class PluginUnitTestBase
{
    protected (Mock<ILocalPluginContext> mockLocalPluginContext, Mock<IPaymentsApiService> mockPaymentsApiService) SetupMockLocalPluginContext(List<Payment> payments)
    {
        // Create a Service Provider 
        var serviceContainer = new ServiceIOCContainer();

     
        // Mock the Payment API
        var mockPaymentsApiService = new Mock<IPaymentsApiService>();

        mockPaymentsApiService.Setup(service => service.GetPayments(It.IsAny<string>()))
                .ReturnsAsync(payments);

        if (payments != null && payments.Count>0)
        {
            mockPaymentsApiService.Setup(service => service.GetPayment(It.IsAny<int>()))
                .ReturnsAsync(payments[0]);
        }
        else
        {
            Payment mockPayment = null;
            mockPaymentsApiService.Setup(service => service.GetPayment(It.IsAny<int>()))
                .ReturnsAsync(mockPayment);
        }
        serviceContainer.Register<IPaymentsApiService, IPaymentsApiService>(mockPaymentsApiService.Object);

        // Mock the OrganizationService
        var mockOrganizationService = new Mock<IOrganizationService>();
        serviceContainer.Register<IOrganizationService,IOrganizationService>(mockOrganizationService.Object);

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
        serviceContainer.Register<ITracingService,ITracingService>( new Mock<ITracingService>().Object);
        
        // Add the service provider to the mock local plugin context
        mockLocalPluginContext.Setup(context => context.ServiceProvider)
            .Returns(serviceContainer);

        return (mockLocalPluginContext, mockPaymentsApiService);
    }
}
