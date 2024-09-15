using ContosoRealEstate.BusinessLogic.Models;
using ContosoRealEstate.BusinessLogic.Plugins;
using ContosoRealEstate.BusinessLogic.Resources;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Contexts;

namespace ContosoRealEstate.BusinessLogic.Plugins;

/// <summary>
/// Plugin development guide: https://docs.microsoft.com/powerapps/developer/common-data-service/plug-ins
/// Best practices and guidance: https://docs.microsoft.com/powerapps/developer/common-data-service/best-practices/business-logic/
/// </summary>
[CrmPluginRegistration("contoso_IsListingAvailableAPI")]
public class IsListingAvailable : BusinessLogicPluginBase, IPlugin
{
    public IsListingAvailable(string unsecureConfiguration, string secureConfiguration)
        : base(typeof(IsListingAvailable), unsecureConfiguration, secureConfiguration)
    {
    }

    // Entry point for custom business logic execution
    public override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
    {
        if (localPluginContext == null)
        {
            throw new ArgumentNullException(nameof(localPluginContext));
        }

        var context = localPluginContext.PluginExecutionContext;
        var service = localPluginContext.ServiceProvider.Get<IOrganizationService>();
        var environmentVariableService = localPluginContext.ServiceProvider.Get<IEnvironmentVariableService>();

        if (UsePowerFxPlugins(environmentVariableService))
        {
            localPluginContext.Trace("PowerFx plugins are enabled. Skipping plugin execution.");
            return;
        }

        ValidateCustomApiExectionContext(context, "contoso_IsListingAvailableAPI");

        try
        {
            var request = MapInputParametersToRequest(context.InputParameters);

            // check if the listing is available
            var query = new QueryExpression()
            {
                EntityName = contoso_Reservation.EntityLogicalName,
                ColumnSet = new ColumnSet(contoso_Reservation.Fields.contoso_ReservationId)
            };
            query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_Listing, ConditionOperator.Equal, new Guid(request.ListingID));
            query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_ReservationStatus, ConditionOperator.NotEqual, (int)contoso_reservationstatus.Cancelled);
            query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_From, ConditionOperator.LessThan, DateTime.Parse(request.To, CultureInfo.InvariantCulture));
            query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_To, ConditionOperator.GreaterThan, DateTime.Parse(request.From, CultureInfo.InvariantCulture));
            if (request.ExcludeReservation != null) query.Criteria.AddCondition(contoso_Reservation.Fields.contoso_ReservationId, ConditionOperator.NotEqual, new Guid(request.ExcludeReservation));

            var reservations = service.RetrieveMultiple(query);
            var reservationFound = reservations.Entities.FirstOrDefault();

            // set the output parameter
            localPluginContext.Trace(
                "Output Parameters\n" +
                "------------------\n" +
                $"Available: {reservationFound == null}");

            context.OutputParameters["Available"] = reservationFound == null;
        }
        catch (Exception ex)
        {
            localPluginContext.Trace($"Error: {ex.Message}");
            throw new InvalidPluginExecutionException(ex.Message);
        }
    }

    private static contoso_IsListingAvailableAPIRequest MapInputParametersToRequest(ParameterCollection inputs) 
    {
        // Map the keys from the inputs to create a new contoso_IsListingAvailableAPIRequest
        var request = new contoso_IsListingAvailableAPIRequest();

        if (inputs.TryGetValue<string>("From", out var fromValue)) request.From = fromValue;
        if (inputs.TryGetValue<string>("To", out var toValue)) request.To = toValue;
        if (inputs.TryGetValue<string>("ExcludeReservation", out var excludeReservationValue)) request.ExcludeReservation = excludeReservationValue;
        if (inputs.TryGetValue<string>("ListingID", out var listingIDValue)) request.ListingID = listingIDValue;
 

        // Check that ListingID, From, To are set
        if (string.IsNullOrEmpty(request.ListingID) || string.IsNullOrEmpty(request.From) || string.IsNullOrEmpty(request.To))
        {
            throw new InvalidPluginExecutionException(ExceptionMessages.MISSING_INPUT_PARAMETERS);
        }

        // To be compatible with the Power Fx Plugin
        request.ExcludeReservation = request.ExcludeReservation == "None" ? null : request.ExcludeReservation;

        // Validate ExcludeReservation Guid if set
        if (request.ExcludeReservation != null)
        {
            if (!Guid.TryParse(request.ExcludeReservation, out _))
            {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, ExceptionMessages.INVALID_PARAMETER, "ExcludeReservation", request.ExcludeReservation));
            }
        }

        // Validate Listing ID Guid
        ValidateGuid("ListingID", request.ListingID);

        // Validate From Date
        ValidateDateTime("From", request.From);

        // Validate To Date
        ValidateDateTime("To", request.To);
        return request;
    }

}
