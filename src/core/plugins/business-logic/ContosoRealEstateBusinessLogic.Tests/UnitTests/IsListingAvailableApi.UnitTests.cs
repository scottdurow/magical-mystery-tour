using ContosoRealEstate.BusinessLogic.Resources;
using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk.Extensions;

namespace ContosoRealEstateBusinessLogic.Tests.UnitTests
{
    [TestClass]
    public class IsListingAvailableApiTests : PluginUnitTestBase
    {
        [TestMethod]
        public void ExecuteDataversePlugin_ShouldThrowInvalidPluginExecutionException_WhenInputParametersAreMissing()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext();

            //mockLocalPluginContext.Setup(x => x.InitiatingUserService).Returns(mockOrganizationService.Object);
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.MessageName).Returns("contoso_IsListingAvailableAPI");
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.Stage).Returns(30);
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters).Returns(new ParameterCollection
            {
                { "From", "2023-10-01" },
                { "To", "2023-10-10" },
                { "ExcludeReservation", Guid.Empty.ToString() }
                // Do not set ListingID
            });

            var plugin = new IsListingAvailable(null, null);

            // Act
            Action executePlugin = () => plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert
            var expectedException = Assert.ThrowsException<InvalidPluginExecutionException>(executePlugin);
            Assert.AreEqual(ExceptionMessages.MISSING_INPUT_PARAMETERS, expectedException.Message);
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldSetAvailableToTrue_WhenListingIsAvailable()
        {
            // Arrange
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext();

            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.MessageName).Returns("contoso_IsListingAvailableAPI");
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.Stage).Returns(30);
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.OutputParameters).Returns(new ParameterCollection());
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters).Returns(new ParameterCollection
            {
                { "From", "2023-10-01" },
                { "To", "2023-10-10" },
                { "ExcludeReservation", Guid.Empty.ToString() },
                { "ListingID", Guid.NewGuid().ToString() }
            });

            mockOrganizationService.Setup(x => x.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection());

            var plugin = new IsListingAvailable(null, null);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert
            Assert.IsTrue((bool)mockLocalPluginContext.Object.PluginExecutionContext.OutputParameters["Available"]);
        }

        [TestMethod]
        public void ExecuteDataversePlugin_ShouldSetAvailableToFalse_WhenListingIsNotAvailable()
        {
            var (mockLocalPluginContext, mockOrganizationService) = SetupMockLocalPluginContext();

            Guid listingId = Guid.NewGuid();
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.MessageName).Returns("contoso_IsListingAvailableAPI");
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.Stage).Returns(30);
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.OutputParameters).Returns(new ParameterCollection());
            mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters).Returns(new ParameterCollection
            {
                { "From", "2023-10-01" },
                { "To", "2023-10-10" },
                { "ExcludeReservation", Guid.Empty.ToString() },
                { "ListingID", listingId.ToString() }
            });

            var reservation = new Entity("contoso_Reservation")
            {
                Id = Guid.NewGuid()
            };
            reservation["contoso_From"] = DateTime.Parse("2023-10-05");
            reservation["contoso_To"] = DateTime.Parse("2023-10-15");
            reservation["contoso_Listing"] = new EntityReference("contoso_listing", Guid.NewGuid());

            mockOrganizationService.Setup(x => x.RetrieveMultiple(It.IsAny<QueryBase>()))
                .Returns(new EntityCollection(new List<Entity> { reservation }));

            var plugin = new IsListingAvailable(null, null);

            // Act
            plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

            // Assert
            Assert.IsFalse((bool)mockLocalPluginContext.Object.PluginExecutionContext.OutputParameters["Available"]);
        }

    }

}
