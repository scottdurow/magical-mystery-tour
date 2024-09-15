using Contoso;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using PaymentVirtualTableProvider;
using PaymentVirtualTableProvider.Services.PaymentApi.Mappers;
using System;
using PaymentVirtualTableProvider.Services.PaymentApi;

namespace PaymentProvider;

public class RetrievePlugin : PluginBase, IPlugin
{
    public RetrievePlugin(string unsecureConfiguration, string secureConfiguration)
           : base(typeof(RetrievePlugin), unsecureConfiguration, secureConfiguration)
    {

    }

    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        // Trace the execution
        localPluginContext.Trace("Retrieving payments from the Payment API.");

        if (
          localPluginContext.PluginExecutionContext.InputParameters.Contains("Target") &&
          localPluginContext.PluginExecutionContext.InputParameters["Target"] is EntityReference)
        {
            try
            {
                EntityReference entityRef = (EntityReference)localPluginContext.PluginExecutionContext.InputParameters["Target"];
                var payment = localPluginContext.ServiceProvider.Get<IPaymentsApiService>().GetPayment(entityRef.Id.ToInt()).Result;

                contoso_payment paymentRow = payment.ToEntity();

                localPluginContext.PluginExecutionContext.OutputParameters["BusinessEntity"] = paymentRow.ToEntity<Entity>();
            }
            catch (Exception ex)
            {
                localPluginContext.TraceException("GetPayment failed:",ex);
                throw new GenericDataAccessException("GetPayment failed:", ex);
            }
        }
    }

}
