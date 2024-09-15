using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Moq;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using PaymentVirtualTableProvider.Services.PaymentApi;
using PaymentVirtualTableProvider.Services;
using System;
using Microsoft.Xrm.Tooling.Connector;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections.Generic;

namespace PaymentVirtualTableProvider.Tests.IntegrationTests
{
    [TestClass]
    public class PaymentsApiIntegrationTests
    {
        public static string GetBearerToken(IEnvironmentVariableService environmentVariableService)
        {
            var tenantId = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiTenantId"); 
            var authority = $"https://login.microsoftonline.com/{tenantId}";
            var resource = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiResourceUrl");
            var clientId = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiAppId");
            var clientSecret = environmentVariableService.RetrieveEnvironmentVariableValue("contoso_PaymentsApiSecret");

            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(clientId, clientSecret);

            AuthenticationResult result = authContext.AcquireTokenAsync(resource, clientCredential).Result;

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }

        [TestMethod]
        public void TestCreatePaymentViaApi()
        {

            // Arrange
            PaymentsApiService api = GetIntegrationTestApi();

            var payment = new Payment
            {
                userId = "1",
                provider = 1,
                reservationId = "1",
                status = 1,
                amount = 123,
                currency = "USD",
                createdAt = DateTime.Now
            };

            // Act
            var result = (api.Create(payment)).Result;

            // Assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void TestRetrieve()
        {

            // Arrange
            PaymentsApiService api = GetIntegrationTestApi();

            // Act
            var result = (api.GetPayment(9999999)).Result;

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void TestGetPaymentViaApi()
        {
            // Arrange
            PaymentsApiService api = GetIntegrationTestApi();

            // Act
            var query = "$filter=Status eq 1&$sortby CreatedAt desc";
            var result = (api.GetPayments(query)).Result;

            // Assert
            Assert.IsNotNull(result);
        }

        private static PaymentsApiService GetIntegrationTestApi()
        {
            // Create a mock ITracingService
            var tracingService = new Mock<ITracingService>();

            // Mock the Trace method
            tracingService.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object[]>())).Callback<string, object[]>((message, parameters) => Console.WriteLine(message, parameters));

            // Create a mock version of the EnvironmentVariableService
            var mockEnvironmentVariableService = new Mock<IEnvironmentVariableService>();
            mockEnvironmentVariableService.Setup(m => m.RetrieveEnvironmentVariableValue(It.IsAny<string>()))
                .Returns((string name) => System.Configuration.ConfigurationManager.AppSettings[$"env_{name}"]);

            // Create mock IManagedIdentityService
            var mockManagedIdentityService = new Mock<IManagedIdentityService>();
            mockManagedIdentityService.Setup(m => m.AcquireToken(It.IsAny<List<string>>())).Returns(GetBearerToken(mockEnvironmentVariableService.Object));

            var api = new PaymentsApiService(mockEnvironmentVariableService.Object, mockManagedIdentityService.Object, tracingService.Object);
            return api;
        }
    }
}
