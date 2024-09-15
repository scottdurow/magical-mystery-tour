using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using Microsoft.Xrm.Sdk.Query;
using PaymentVirtualTableProvider;
using PaymentVirtualTableProvider.Services.PaymentApi;
using PaymentVirtualTableProvider.Services.PaymentApi.Mappers;
using System;
using System.Linq;
using System.Text.Json;

namespace PaymentProvider;


public class RetrieveMultiplePlugin : PaymentVirtualTablePluginBase, IPlugin
{
    public RetrieveMultiplePlugin(string unsecureConfiguration, string secureConfiguration)
           : base(typeof(RetrieveMultiplePlugin), unsecureConfiguration, secureConfiguration)
    {

    }

    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {


        // Trace the execution
        localPluginContext.Trace("Retrieving payments from the Payment API.");

        string restQuery;
        try
        {
            var query = localPluginContext.PluginExecutionContext.InputParameters["Query"];
            QueryExpression queryExpression = null;
            if (query != null)
            {
                if (query is QueryExpression expression)
                {
                    queryExpression = expression;
                    localPluginContext.Trace($"Found QueryExpression {JsonSerializer.Serialize(queryExpression)}");
                }
                else if (query is FetchExpression fetchExpression)
                {
                    localPluginContext.Trace($"Found fetchxml {fetchExpression.Query}");
                    var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
                    var convertRequest = new FetchXmlToQueryExpressionRequest
                    {
                        FetchXml = fetchExpression.Query
                    };
                    var response = (FetchXmlToQueryExpressionResponse)service.Execute(convertRequest);
                    queryExpression = response.Query;
                    localPluginContext.Trace($"Converted to QueryExpression {JsonSerializer.Serialize(queryExpression)}");
                }
            }

            // Convert the QueryExpression into a REST query
            restQuery = new QueryExpressionMapper(new PaymentFieldValueMapper()).ToRestQuery(queryExpression);

            localPluginContext.Trace($"Converted to rest query {restQuery}");
        }
        catch (Exception ex)
        {
            localPluginContext.TraceException($"Error converting query", ex);
            throw new InvalidQueryException($"Invalid query: {ex.Message}");
        }

        try
        {
            var payments = localPluginContext.ServiceProvider.Get<IPaymentsApiService>().GetPayments(restQuery).Result;

            // Trace how many payments were retrieved
            localPluginContext.Trace($"Retrieved {payments.Count} payments");

            // For each payment, Create a new Entity and add the attributes for each of the payment properties
            EntityCollection collection = new();
            foreach (var apiPayment in payments)
            {
                var paymentRow = apiPayment.ToEntity();
                collection.Entities.Add(paymentRow.ToEntity<Entity>());
            }

            localPluginContext.PluginExecutionContext.OutputParameters["BusinessEntityCollection"] = collection;
        }
        catch (Exception ex)
        {
            localPluginContext.TraceException($"Call to Payment API failed",ex);
            throw new GenericDataAccessException("Call to Payment API failed", ex);
        }
    }

}
