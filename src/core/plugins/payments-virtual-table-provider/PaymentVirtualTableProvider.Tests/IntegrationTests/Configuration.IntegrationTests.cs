
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
    public class ConfigurationUnitTests
    {
        [TestMethod]
        public void Configuration_ShouldReturnSecret()
        {

            // Get the connection string IntegrationTestEnvironment from the app.config file
            var tenantId =
                      System.Configuration.ConfigurationManager.AppSettings["env_contoso_PaymentsApiTenantId"];
           
            // Check that the value is not empty string
            Assert.IsNotNull(tenantId);
            // Check that the user secrets are set - otherwise the value will be **** from the app.config
            // See README
            Assert.AreNotEqual(tenantId, "****");
        }
    }
}
