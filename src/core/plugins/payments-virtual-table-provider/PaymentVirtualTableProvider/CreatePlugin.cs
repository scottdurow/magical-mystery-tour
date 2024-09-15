using Contoso;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using Microsoft.Xrm.Sdk.Extensions;
using PaymentVirtualTableProvider;
using PaymentVirtualTableProvider.Services.PaymentApi;
using PaymentVirtualTableProvider.Services.PaymentApi.Mappers;
using System;

namespace PaymentProvider;

public class CreatePlugin : PaymentVirtualTablePluginBase, IPlugin
{
    public CreatePlugin(string unsecureConfiguration, string secureConfiguration)
           : base(typeof(CreatePlugin), unsecureConfiguration, secureConfiguration)
    {

    }

    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        // Trace the execution
        localPluginContext.Trace("Creating payment via the Payment API.");

        try
        {
            if (localPluginContext.PluginExecutionContext.InputParameters.Contains("Target") && localPluginContext.PluginExecutionContext.InputParameters["Target"] is Entity)
            {
                contoso_payment entity = ((Entity)localPluginContext.PluginExecutionContext.InputParameters["Target"]).ToEntity<contoso_payment>();

                // Map the entity to the api payment model
                var apiPayment = entity.ToApi();

                int paymentId = localPluginContext.ServiceProvider.Get<IPaymentsApiService>().Create(apiPayment).Result;
                localPluginContext.PluginExecutionContext.OutputParameters["id"] = paymentId.ToGuid();
            }
        }
        catch (Exception ex)
        {
            localPluginContext.TraceException($"Call to Payment API failed", ex);
            throw new GenericDataAccessException("Call to Payment API failed", ex);
        }
    }

}

