
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Moq;
using PaymentVirtualTableProvider.Services;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System;
using System.Xml.Linq;

namespace PaymentVirtualTableProvider.Tests.IntegrationTests
{
  [TestClass]
  public class EnvironmentVariableServiceIntegrationTests
  {
    [TestMethod]
    public void TestEnvironmentVariables()
    {

      // Tests that the Environment Service correctly reads from Dataverse environment variables (until such times as the platform does this for us)

      // Get the connection string IntegrationTestEnvironment from the app.config file
      var connectionString =
                System.Configuration.ConfigurationManager.AppSettings["IntegrationTestConnectionString"];
      var service = new CrmServiceClient(connectionString);

      // Create a mock ITracingService using Moq so that we can test the service
      var tracingService = new Mock<ITracingService>();
      // Mock the Trace method
      tracingService.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message,parameters) => Console.WriteLine(message, parameters));

      var environmentVariableService = new EnvironmentVariableService(service, tracingService.Object);

      // Test the RetrieveEnvironmentVariableValue method with an existing environment variable
      var environmentVariableValue = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiBaseUrl");
      // Check that the value is not empty string
      Assert.IsFalse(string.IsNullOrEmpty(environmentVariableValue));
    }
  }
}
