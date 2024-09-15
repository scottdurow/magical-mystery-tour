using Contoso;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Moq;
using PaymentProvider;
using PaymentVirtualTableProvider.Services.PaymentApi.Model;
using System;
using System.Collections.Generic;

namespace PaymentVirtualTableProvider.Tests.UnitTests;

[TestClass]
[TestCategory("UnitTest")]
public class RetrieveMultiplePluginTests : PluginUnitTestBase
{

    [TestMethod]
    public void ExecuteDataversePlugin_ShouldReturnMockedPayments()
    {
        // Arrange
        var payments = new List<Payment>
{
    new() { id = 1, userId = "1", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
    new() { id = 2, userId = "1", provider = 1, status = 1, amount = 456, currency = "USD", createdAt = DateTime.Now },
    new() { id = 3, userId = "1", provider = 1, status = 1, amount = 444, currency = "GBP", createdAt = DateTime.Now }
};

        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(new QueryExpression());

        var plugin = new RetrieveMultiplePlugin("", "");

        // Act
        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Assertions to check that the BusinessEntityCollection contains 3 entities with correct attributes
        var outputParameters = mockLocalPluginContext.Object.PluginExecutionContext.OutputParameters;
        var businessEntityCollection = outputParameters["BusinessEntityCollection"] as EntityCollection;
        Assert.AreEqual(3, businessEntityCollection.Entities.Count);

        for (int i = 0; i < businessEntityCollection.Entities.Count; i++)
        {
            var entity = businessEntityCollection.Entities[i];
            var payment = payments[i];

            Assert.AreEqual(payment.id.ToGuid(), entity.GetAttributeValue<Guid>("contoso_paymentid"));
            Assert.AreEqual(payment.userId, entity.GetAttributeValue<string>("contoso_userid"));
            Assert.AreEqual(payment.reservationId, entity.GetAttributeValue<string>("contoso_reservationid"));
            Assert.AreEqual(payment.provider, entity.GetAttributeValue<OptionSetValue>("contoso_provider").Value);
            Assert.AreEqual(payment.status, entity.GetAttributeValue<OptionSetValue>("contoso_status").Value);
            Assert.AreEqual(payment.amount, entity.GetAttributeValue<decimal>("contoso_amount"));
            Assert.AreEqual(payment.currency, entity.GetAttributeValue<string>("contoso_currencycode"));
            Assert.AreEqual(payment.createdAt, entity.GetAttributeValue<DateTime>("contoso_createdon"));
        }
    }

    [TestMethod]
    public void ExecuteDataversePlugin_QueryByStatusAndAmount()
    {
        // Arrange
        var payments = new List<Payment>
            {
                new() { id = 1, userId = "1", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
            };

        // Create a queryexpression and add to the input parameters
        var queryExpression = new QueryExpression("contoso_payment")
        {
            ColumnSet = new ColumnSet("contoso_paymentid", "contoso_userid", "contoso_provider", "contoso_status", "contoso_amount", "contoso_currencycode", "contoso_createdon"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("contoso_status", ConditionOperator.Equal, 1),
                    new ConditionExpression("contoso_amount", ConditionOperator.LessThan, 1000)
                }
            }
        };

        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(queryExpression);


        // Act
        var plugin = new RetrieveMultiplePlugin("", "");

        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Check that the GetPayments method was called with the correct query
        mockPaymentsApiService.Verify(service => service.GetPayments("$filter=Status eq 1 and Amount lt 1000&top=1000"), Times.Once);


    }

    [TestMethod]
    public void ExecuteDataversePlugin_QueryByPortalUser()
    {
        // Arrange
        var payments = new List<Payment>
            {
                new() { id = 1, userId = "1", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
            };

        // Create a queryexpression and add to the input parameters
        var queryExpression = new QueryExpression("contoso_payment")
        {
            ColumnSet = new ColumnSet("contoso_paymentid", "contoso_userid", "contoso_provider", "contoso_status", "contoso_amount", "contoso_currencycode", "contoso_createdon"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("contoso_portaluser", ConditionOperator.Equal, new EntityReference("contact",Guid.Empty)),
                    new ConditionExpression("contoso_amount", ConditionOperator.LessThan, 1000)
                }
            }
        };

        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(queryExpression);

        // Act
        var plugin = new RetrieveMultiplePlugin("", "");
        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Check that the GetPayments method was called with the correct query
        mockPaymentsApiService.Verify(service => service.GetPayments("$filter=UserId eq 'portal-00000000-0000-0000-0000-000000000000' and Amount lt 1000&top=1000"), Times.Once);


    }


    [TestMethod]
    public void ExecuteDataversePlugin_QueryByReservation()
    {
        // Arrange
        var payments = new List<Payment>
            {
                new() { id = 1, userId = "1", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
            };

        // Create a queryexpression and add to the input parameters
        var queryExpression = new QueryExpression
        {
            EntityName = "contoso_payment",
            ColumnSet = new ColumnSet(new List<string>
            {
                "contoso_paymentid",
                "contoso_portaluser",
                "contoso_provider",
                "contoso_amount",
                "contoso_currencycode",
                "contoso_createdon",
                "contoso_status",
                "contoso_name"
            }.ToArray())
           ,
            Criteria = new FilterExpression
            {
                FilterOperator = LogicalOperator.And,
                Conditions =
                {
                    new ConditionExpression
                    {
                        AttributeName = "contoso_reservation",
                        Operator = ConditionOperator.Equal,
                        Values = { new Guid("36be76b3-1e33-ef11-8409-0022480bc9e8") }
                    }
                }
            },
            Orders =
            {
                new OrderExpression
                {
                    AttributeName = "contoso_createdon",
                    OrderType = OrderType.Descending
                }
            },
            PageInfo = new PagingInfo
            {
                PageNumber = 1,
                Count = 50,
                ReturnTotalRecordCount = true
            }
        };


        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(queryExpression);

        // Act
        var plugin = new RetrieveMultiplePlugin("", "");
        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Check that the GetPayments method was called with the correct query
        mockPaymentsApiService.Verify(service => service.GetPayments("$filter=ReservationId eq 'portal-reservation-36be76b3-1e33-ef11-8409-0022480bc9e8'&orderby=CreatedAt desc&top=1000"), Times.Once);


    }

    // Test that when retrieving a payment from the api, the user id field populates the contoso_portaluserid field with an entityreference - that has a guid id with the portal-user-prefix stripped
    [TestMethod]
    public void ExecuteDataversePlugin_ShouldPopulatePortalUserIdField()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new() { id = 1,reservationId = "portal-reservation-00000000-0000-0000-0000-000000000002", userId = "portal-00000000-0000-0000-0000-000000000001", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
        };

        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(new QueryExpression());

        var plugin = new RetrieveMultiplePlugin("", "");

        // Act
        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Assert
        var outputParameters = mockLocalPluginContext.Object.PluginExecutionContext.OutputParameters;
        var businessEntityCollection = outputParameters["BusinessEntityCollection"] as EntityCollection;
        Assert.AreEqual(1, businessEntityCollection.Entities.Count);

        var entity = businessEntityCollection.Entities[0].ToEntity<contoso_payment>();
        var expectedUserId = new Guid("00000000-0000-0000-0000-000000000001");
        Assert.IsNotNull(entity.contoso_PortalUser);
        var actualUserId = entity.contoso_PortalUser.Id;
        Assert.AreEqual(expectedUserId, actualUserId);

        var expectedReservationId = new Guid("00000000-0000-0000-0000-000000000002");
        Assert.IsNotNull(entity.contoso_Reservation);
        var actualReservationId = entity.contoso_Reservation.Id;
        Assert.AreEqual(expectedReservationId, actualReservationId);

    }


    [TestMethod]
    public void ExecuteDataversePlugin_QueryByDate()
    {
        // Arrange
        var payments = new List<Payment>
            {
                new() { id = 1, userId = "1", provider = 1, status = 1, amount = 123, currency = "USD", createdAt = DateTime.Now },
            };

        // Create a queryexpression and add to the input parameters
        var queryExpression = new QueryExpression
        {
            EntityName = "contoso_payment",
            Distinct = true,
            ColumnSet = new ColumnSet(new List<string>
        {
            "contoso_paymentid",
            "contoso_amount",
            "contoso_currencycode",
            "contoso_provider",
            "contoso_status",
            "contoso_createdon",
            "contoso_portaluser",
            "contoso_reservation",
            "contoso_name"
        }.ToArray()),
            
            Criteria = new FilterExpression
            {
                FilterOperator = LogicalOperator.And,
                Filters =
        {
            new FilterExpression
            {
                FilterOperator = LogicalOperator.Or,
                Filters =
                {
                    new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "contoso_createdon",
                                Operator = ConditionOperator.OnOrAfter,
                                Values = { DateTime.Parse("2025-01-06T08:00:00Z") }
                            }
                        }
                    }
                }
            },
            new FilterExpression
            {
                FilterOperator = LogicalOperator.And,
                Filters =
                {
                    new FilterExpression
                    {
                        FilterOperator = LogicalOperator.Or,
                        FilterHint = "union",
                        Conditions =
                        {
                            new ConditionExpression
                            {
                                AttributeName = "contoso_portaluser",
                                Operator = ConditionOperator.Equal,
                                Values = { new Guid("4f38798e-f84a-ef11-a317-7c1e52150b3d") }
                            }
                        }
                    }
                }
            }
        }
            },
            Orders =
    {
        new OrderExpression
        {
            AttributeName = "contoso_amount",
            OrderType = OrderType.Ascending
        }
    },
            PageInfo = new PagingInfo
            {
                PageNumber = 1,
                Count = 10,
                ReturnTotalRecordCount = true
            }
        };


        var (mockLocalPluginContext, mockPaymentsApiService) = SetupMockLocalPluginContext(payments);
        mockLocalPluginContext.Setup(context => context.PluginExecutionContext.InputParameters["Query"]).Returns(queryExpression);

        // Act
        var plugin = new RetrieveMultiplePlugin("", "");
        plugin.ExecuteDataversePlugin(mockLocalPluginContext.Object);

        // Check that the GetPayments method was called with the correct query
        mockPaymentsApiService.Verify(service => service.GetPayments("$filter=CreatedAt ge '2025-01-06T00:00:00.0000000-08:00' and UserId eq 'portal-4f38798e-f84a-ef11-a317-7c1e52150b3d'&orderby=Amount asc&top=1000"), Times.Once);


    }

}
