using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using PaymentProvider;
using PaymentVirtualTableProvider;
using PaymentVirtualTableProvider.Services.PaymentApi;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaymentVirtualTableProvider.Tests.UnitTests
{
    [TestClass]
    public class RetrievePluginTests : PluginUnitTestBase
    {

        [TestMethod]
        public void Retrieve_ThrowObjectNotFound()
        {
            // Arrange
            var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(new List<Payment>());
            mockLocalPluginContext.Object.PluginExecutionContext.InputParameters["Target"] = new EntityReference("contoso_payment",Guid.NewGuid());

            var plugin = new RetrievePlugin("", "");

            // Act & Assert that the execption was thrown
            Assert.ThrowsException<GenericDataAccessException>(() => plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object));
           
        }

    }
}
